using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KH21SE
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Registration : ContentPage
    {
        private static ServerCommunication s = ServerCommunication.Instance;
        public Registration()
        {
            InitializeComponent();
            needticket_containerAnimationIn = new Animation(v => needticket_container.HeightRequest = v, 600, 0, Easing.CubicInOut);
        }
        public Race currentRace;
        private DateTime launchTime;
        private async void StartUpdateTimer()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                var timeSpan = launchTime-DateTime.Now;
                time_d.Text = timeSpan.Days.ToString("00") + " D";
                time_h.Text = timeSpan.Hours.ToString("00") + " H";
                time_m.Text = timeSpan.Minutes.ToString("00") + " M";
                time_s.Text = timeSpan.Seconds.ToString("00") + " S";
                return true;
            });
        }
        protected async override void OnAppearing()
        {
            currentRace = await s.GetRace();
            TimeSpan time = TimeSpan.FromMilliseconds(currentRace.date);
            DateTime startdate = new DateTime(1970, 1, 1) + time;
            launchTime = startdate;
            if (ServerCommunication.CachedRaces != null && ServerCommunication.CachedRaces.Find(el => el._raceId == currentRace._id) != null)
            {
                var userRaceToRef = ServerCommunication.CachedRaces.Find(el => el._raceId == currentRace._id);
                ticketNumber.Text = (userRaceToRef.inperson ? "I" : "O") + "-" + userRaceToRef._id.ToString().PadLeft(5, '0');
                backbutton_text.Text = "< Go Back Home!";
                pagesubtitle.Text = "You already registered!";
                needticket_container.IsVisible = false;
                hasticket_container.IsVisible = true;
                hasticket_container.Opacity = 0;
                hasticket_container.FadeTo(1, 250);
                StartUpdateTimer();
            }
            else
            {
                var response = await s.GetIfRegistered(ServerCommunication.MyUserInstance, currentRace);
                if (!response.StartsWith("Failure"))
                {
                    var userRaceToRef = JsonConvert.DeserializeObject<UserRace>(response);
                    // response is yes
                    // this is different
                    if (ServerCommunication.CachedRaces == null)
                    {
                        ServerCommunication.CachedRaces = new List<UserRace>();
                    }
                    ServerCommunication.CachedRaces.Add(userRaceToRef);
                    ticketNumber.Text = (userRaceToRef.inperson ? "I" : "O") + "-" + userRaceToRef._id.ToString().PadLeft(5, '0');
                    backbutton_text.Text = "< Go Back Home!";
                    pagesubtitle.Text = "You already registered!";
                    needticket_container.IsVisible = false;
                    hasticket_container.IsVisible = true;
                    hasticket_container.Opacity = 0;
                    hasticket_container.FadeTo(1, 250);
                    StartUpdateTimer();
                }
                else
                {
                    needticket_name.Text = currentRace.name;
                    needticket_participants.Text = currentRace.lastUpdatedParticipants.ToString() + " participants";
                    needticket_date.Text = startdate.ToString("dd MMM yyyy hh:mm:ss");
                }
            }
            
            base.OnAppearing();
        }
        private Animation needticket_containerAnimationIn;
        private void GoToHome(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void ChangeRegistrationDetail(object sender, EventArgs e)
        {
            if(e1_picker.SelectedIndex != -1 && e2_picker.SelectedIndex != -1 && e3_picker.SelectedIndex != -1 && e4_picker.SelectedIndex != -1)
            {
                submitTicket.IsEnabled = true;
                submitTicket.Text = "Get My Ticket!";
            }
            else
            {
                submitTicket.IsEnabled = false;
                submitTicket.Text = "Fill Out All The Fields";
            }
        }

        private async void GetTicket(object sender, EventArgs e)
        {

            var response = await s.NewRegistration(ServerCommunication.MyUserInstance, currentRace, new UserRace()
            {
                tshirt = e2_picker.Items[e2_picker.SelectedIndex],
                inperson = e1_picker.Items[e1_picker.SelectedIndex] == "Virtual" ? false : true,
                exercise = e3_picker.Items[e1_picker.SelectedIndex]
            });

            if (!response.StartsWith("Failure"))
            {
                var userRaceToRef = JsonConvert.DeserializeObject<UserRace>(response);
                if(ServerCommunication.CachedRaces == null)
                {
                    ServerCommunication.CachedRaces = new List<UserRace>();
                }
                ServerCommunication.CachedRaces.Add(userRaceToRef);
                ticketNumber.Text = (userRaceToRef.inperson ? "I" : "O") + "-" + userRaceToRef._id.ToString().PadLeft(5, '0');
                await needticket_container.FadeTo(0, 250);
                await pagesubtitle.FadeTo(0, 100);
                backbutton_text.FadeTo(0, 100);
                backbutton_text.Text = "< Go Back Home!";
                backbutton_text.FadeTo(1, 100);
                pagesubtitle.Text = "You just registered! Congrats!";
                pagesubtitle.FadeTo(1, 100);
                needticket_container.IsVisible = false;
                hasticket_container.IsVisible = true;
                hasticket_container.Opacity = 0;
                hasticket_container.FadeTo(1, 250);
                StartUpdateTimer();
            }
            else
            {
                Console.WriteLine("OH NOES!");
            }

            




        }
    }
}