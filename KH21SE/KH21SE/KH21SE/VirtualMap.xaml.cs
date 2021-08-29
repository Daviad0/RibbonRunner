using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Xamarin.Forms.Maps;
using Google.Type;
using Newtonsoft.Json;

namespace KH21SE
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VirtualMap : ContentPage
    {
        private static ServerCommunication s = ServerCommunication.Instance;
        private Race selectedRace;
        public VirtualMap(Race race)
        {
            selectedRace = race;
            InitializeComponent();
        }
        private LatLng lastCoordinates;
        private bool shouldBeChecking = false;
        private double zoom = .001;
        private bool snapTo = true;
        private System.DateTime start = System.DateTime.Now;
        private List<string> teamIdsHere = new List<string>();
        protected override void OnAppearing()
        {

            if(selectedRace != null)
            {
                mode_label.Text = "Race";
            }

            shouldBeChecking = true;
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                Task.Factory.StartNew(async () =>
                {
                    if(lastCoordinates != null)
                    {
                        var response = await s.PostLocation(new SendableLocation() { Latitude = lastCoordinates.Latitude, Longitude = lastCoordinates.Longitude }, ServerCommunication.MyUserInstance);
                    }
                });
                foreach(var member in (ServerCommunication.MyTeam == null ? new string[] { } : ServerCommunication.MyTeam._memberIds))
                {
                    if(member != ServerCommunication.MyUserInstance._id)
                    {
                        if (!teamIdsHere.Contains(member))
                            teamIdsHere.Add(member);
                        Task.Factory.StartNew(async () =>
                        {
                            var response = await s.GetTeamLocation(ServerCommunication.MyUserInstance, new User() { _id = member });
                            if (!response.StartsWith("Failed"))
                            {
                                
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    var location = JsonConvert.DeserializeObject<SendableLocation>(response);
                                    if (dorasMap.MapElements.ToList().Find(el => el.ClassId == member) != null)
                                    {
                                        dorasMap.MapElements.Remove(dorasMap.MapElements.ToList().Find(el => el.ClassId == member));
                                    }
                                    Circle circle = new Circle
                                    {
                                        Center = new Position(location.Latitude, location.Longitude),
                                        Radius = new Distance(5),
                                        StrokeColor = Xamarin.Forms.Color.FromRgba(66, 182, 245, 255),
                                        StrokeWidth = 8,
                                        FillColor = Xamarin.Forms.Color.FromRgba(66, 182, 245, 50),
                                        ClassId = member
                                    };
                                    dorasMap.MapElements.Add(circle);
                                });
                            }
                        });
                    }
                    
                }
                return shouldBeChecking;
            });
            Device.StartTimer(TimeSpan.FromSeconds(2), () =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        var location = await Geolocation.GetLocationAsync();
                        
                        if(lastCoordinates != null)
                        {
                            var kmTotal = Meters.ComputeDistanceBetween(lastCoordinates, new LatLng() { Latitude = location.Latitude, Longitude = location.Longitude });
                            TotalDistance += kmTotal;
                            Console.WriteLine((TotalDistance).ToString() + " meters");
                        }
                        if (lastCoordinates != null && (location.Latitude != lastCoordinates.Latitude || lastCoordinates.Longitude != location.Longitude))
                        {
                            Circle circle = new Circle
                            {
                                Center = new Position(lastCoordinates.Latitude, lastCoordinates.Longitude),
                                Radius = new Distance(5),
                                StrokeColor = Xamarin.Forms.Color.FromHex("#88FF0000"),
                                StrokeWidth = 8,
                                FillColor = Xamarin.Forms.Color.FromHex("#88FFC0CB"),
                                ClassId = "ME!"
                            };
                            dorasMap.MapElements.Add(circle);
                        }
                        lastCoordinates = new LatLng() { Latitude = location.Latitude, Longitude = location.Longitude };
                        distanceWidget.Text = Math.Round(TotalDistance/1000, 1).ToString() + "km";
                        progressDistance.ProgressTo((TotalDistance) / (selectedRace == null ? 5000 : selectedRace.meters), 1000, Easing.CubicInOut);
                        if(snapTo)
                            dorasMap.MoveToRegion(new MapSpan(new Position(location.Latitude, location.Longitude), zoom, zoom));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
                return shouldBeChecking;
            });
            base.OnAppearing();
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                var timeSpan = System.DateTime.Now - start;
                timer.Text = timeSpan.Hours.ToString("00") + ":" + timeSpan.Minutes.ToString("00") + ":" + timeSpan.Seconds.ToString("00");
                team.Text = teamIdsHere.Count.ToString() + " members active";
                return shouldBeChecking;
            });
        }
        private double TotalDistance;
        private void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"MapClick: {e.Position.Latitude}, {e.Position.Longitude}");
            
            /*if(previousLocation != null)
            {
                var diffLong = Math.Abs(previousLocation.Longitude - e.Position.Longitude);
                var diffLat = Math.Abs(previousLocation.Latitude - e.Position.Latitude);
                TotalDistance += diffLat + diffLong;
            }
            
            previousLocation = e.Position;
            Console.WriteLine(TotalDistance);*/

        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            shouldBeChecking = false;

            if(selectedRace == null)
            {
                var response = await s.AddPracticeMeters(ServerCommunication.MyUserInstance, (int)Math.Round(TotalDistance));
                if (!response.StartsWith("Failed"))
                {
                    var userToUpdate = JsonConvert.DeserializeObject<User>(response);
                    ServerCommunication.MyUserInstance = userToUpdate;
                }
                else
                {
                    await DisplayAlert("Uh oh", "For some reason, we failed to save your practice meters on your account! We apologize for the inconvinence", "Ok");
                }
            }
            else
            {
                // handle adding successful race here
                if(TotalDistance >= selectedRace.meters)
                {
                    // was able to successfully complete a 5k
                }
            }
            

            Navigation.PopAsync();
        }
        private void ToggleSnap(object sender, EventArgs e)
        {
            snapTo = !snapTo;
            if(snapTo && lastCoordinates != null)
                dorasMap.MoveToRegion(new MapSpan(new Position(lastCoordinates.Latitude, lastCoordinates.Longitude), zoom, zoom));
        }
        private void ZoomIn(object sender, EventArgs e) 
        { 
            if(zoom > 0.0001)
            {
                zoom = zoom * .5;
            }
            if (lastCoordinates != null)
                dorasMap.MoveToRegion(new MapSpan(new Position(lastCoordinates.Latitude, lastCoordinates.Longitude), zoom, zoom));
        }
        private void ZoomOut(object sender, EventArgs e)
        {
            if (zoom < .1)
            {
                zoom = zoom * 2;
            }
            if (lastCoordinates != null)
                dorasMap.MoveToRegion(new MapSpan(new Position(lastCoordinates.Latitude, lastCoordinates.Longitude), zoom, zoom));
        }
    }
    public static class Meters
    {
        const double EARTH_RADIUS = 6371009;

        static double ToRadians(double input)
        {
            return input / 180.0 * Math.PI;
        }

        static double DistanceRadians(double lat1, double lng1, double lat2, double lng2)
        {
            double Hav(double x)
            {
                double sinHalf = Math.Sin(x * 0.5);
                return sinHalf * sinHalf;
            }
            double ArcHav(double x)
            {
                return 2 * Math.Asin(Math.Sqrt(x));
            }
            double HavDistance(double lat1b, double lat2b, double dLng)
            {
                return Hav(lat1b - lat2b) + Hav(dLng) * Math.Cos(lat1b) * Math.Cos(lat2b);
            }
            return ArcHav(HavDistance(lat1, lat2, lng1 - lng2));
        }

        public static double ComputeDistanceBetween(LatLng from, LatLng to)
        {
            double ComputeAngleBetween(LatLng From, LatLng To)
            {
                return DistanceRadians(ToRadians(from.Latitude), ToRadians(from.Longitude),
                                              ToRadians(to.Latitude), ToRadians(to.Longitude));
            }
            return ComputeAngleBetween(from, to) * EARTH_RADIUS;
        }

        public static double ComputeLength(List<LatLng> path)
        {
            if (path.Count < 2)
                return 0;

            double length = 0;
            LatLng prev = path[0];
            double prevLat = ToRadians(prev.Latitude);
            double prevLng = ToRadians(prev.Longitude);
            foreach (LatLng point in path)
            {
                double lat = ToRadians(point.Latitude);
                double lng = ToRadians(point.Longitude);
                length += DistanceRadians(prevLat, prevLng, lat, lng);
                prevLat = lat;
                prevLng = lng;
            }
            return length * EARTH_RADIUS;
        }
    }
}