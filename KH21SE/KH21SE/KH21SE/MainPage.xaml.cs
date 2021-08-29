using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace KH21SE
{
    public partial class MainPage : ContentPage
    {
        private static ServerCommunication s = ServerCommunication.Instance;
        public MainPage()
        {
            InitializeComponent();
            card_greetingAnimationOut = new Animation(v => card_greeting.HeightRequest = v, 100, 250, Easing.CubicInOut);
            card_greetingAnimationIn = new Animation(v => card_greeting.HeightRequest = v, 250, 100, Easing.CubicInOut);
            card_greetingContentAnimationOut = new Animation(v => card_greetingContent.Height = v, 50, 200, Easing.CubicInOut);
            card_greetingContentAnimationIn = new Animation(v => card_greetingContent.Height = v, 200, 50, Easing.CubicInOut);
            
        }
        public Race currentRace;
        protected async override void OnAppearing()
        {
            

            base.OnAppearing();
            if (!ServerCommunication.IsInitialized)
            {
                await s.Initialize();
            }
            currentRace = await s.GetRace();
            TimeSpan time = TimeSpan.FromMilliseconds(currentRace.date);
            DateTime startdate = new DateTime(1970, 1, 1) + time;

            if (ServerCommunication.MyUserInstance.logs != null)
            {
                var total = 0;
                foreach(int m in ServerCommunication.MyUserInstance.logs)
                {
                    total += m;
                }
                totalRun.Text = "You have ran a total of " + (Math.Round((double)total / 1000, 1)).ToString() + "km! Great job and keep it up with a bit of practice completing a 5k (tip: you can go with your team members as well!)";
            }

            if (ServerCommunication.CachedRaces != null && ServerCommunication.CachedRaces.Find(el => el._raceId == currentRace._id) != null)
            {
                var userRaceToRef = ServerCommunication.CachedRaces.Find(el => el._raceId == currentRace._id);
                card_registration.IsVisible = false;
                card_inevent.IsVisible = true;
                card_ineventDate.Text = startdate.ToString("dd MMM yyyy hh:mm:ss");
                card_ineventTicketType.Text = userRaceToRef.inperson ? "In Person" : "Online";
                if(startdate < DateTime.Now)
                {
                    joinRace.IsVisible = true;
                }
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
                    card_registration.IsVisible = false;
                    card_inevent.IsVisible = true;
                    card_ineventDate.Text = startdate.ToString("dd MMM yyyy hh:mm:ss");
                    card_ineventTicketType.Text = userRaceToRef.inperson ? "In Person" : "Online";
                    if (startdate < DateTime.Now)
                    {
                        joinRace.IsVisible = true;
                    }
                }
                else
                {
                    card_registration.IsVisible = true;
                    card_inevent.IsVisible = false;
                    card_registrationPrompt.Text = "Hey " + ServerCommunication.MyUserInstance.name + ", are you available on " + startdate.ToString("MMMM dd") + "? There's an awesome 5k waiting to happen and you are the perfect candidate for it!";
                }
            }

            card_greetingMainGreeting.Text = "Good " + (DateTime.Now.ToString("tt") == "AM" ? "morning, " : "afternoon, ") + ServerCommunication.MyUserInstance.name;

            if (ServerCommunication.MyTeam != null)
            {
                card_teamHasntJoined.IsVisible = false;
                card_teamHasJoined.IsVisible = true;
                card_teamTeamName.Text = ServerCommunication.MyTeam.name;
            }
            else
            {
                var response = await s.GetTeam(ServerCommunication.MyUserInstance);
                if (!response.StartsWith("Failed"))
                {
                    ServerCommunication.MyTeam = JsonConvert.DeserializeObject<Team>(response);
                    card_teamHasntJoined.IsVisible = false;
                    card_teamHasJoined.IsVisible = true;
                    card_teamTeamName.Text = ServerCommunication.MyTeam.name;
                    card_teamLabel.Text = "Your Team";
                    card_teamTeamId.Text = "Code: " + ServerCommunication.MyTeam._id;
                    card_teamMembers.Text = ServerCommunication.MyTeam._memberIds.Length.ToString() + " members";
                }
            }
        }
        private Animation card_greetingAnimationOut;
        private Animation card_greetingAnimationIn;
        private Animation card_greetingContentAnimationOut;
        private Animation card_greetingContentAnimationIn;
        private bool card_greetingExpanded;
        private async void ExpandGreeting(object sender, EventArgs e)
        {
            if (card_greetingExpanded)
            {
                card_greeting.Animate("card_greetingAnimationIn", card_greetingAnimationIn, length: 500);
                card_greetingExpand.RotateTo(180, 350, Easing.CubicInOut);
                card_greetingContentAnimationIn.Commit(this, "card_greetingContentAnimationIn", length: 500);
                await card_greetingMainGreeting.FadeTo(0, 100);
                card_greetingMainGreeting.Text = "Good " + (DateTime.Now.ToString("tt") == "AM" ? "morning, " : "afternoon, ") + ServerCommunication.MyUserInstance.name;
                card_greetingExpandedContent.FadeTo(0, 200);
                card_greetingMainGreeting.FadeTo(1, 100);
            }
            else
            {
                card_greetingWalks.Text = (ServerCommunication.MyUserInstance.logs == null ? "No" : ServerCommunication.MyUserInstance.logs.Length.ToString()) + " walk(s)";
                card_greeting.Animate("card_greetingAnimationOut", card_greetingAnimationOut, length: 500);
                card_greetingExpand.RotateTo(0, 350, Easing.CubicInOut);
                card_greetingContentAnimationOut.Commit(this, "card_greetingContentAnimationOut", length: 500);
                await card_greetingMainGreeting.FadeTo(0, 100);
                card_greetingMainGreeting.Text = ServerCommunication.MyUserInstance.name;
                
                card_greetingExpandedContent.FadeTo(1, 200);
                card_greetingMainGreeting.FadeTo(1, 100);
            }

            card_greetingExpanded = !card_greetingExpanded;
        }

        private async void Register(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Registration());
        }
        private async void Practice(object sender, EventArgs e)
        {
            Navigation.PushAsync(new VirtualMap(null));
        }
        private async void JoinTeam(object sender, EventArgs e)
        {
            var teamId = await DisplayPromptAsync("Team ID", "Please enter in the team ID that you wish to join", "Join", "Never Mind", "ABCDEFGH", 8);
            if(teamId != "" && teamId != null)
            {
                var response = await s.JoinTeam(new Team()
                {
                    _id = teamId
                }, ServerCommunication.MyUserInstance);
                if (!response.StartsWith("Failed"))
                {
                    ServerCommunication.MyTeam = JsonConvert.DeserializeObject<Team>(response);
                    card_teamHasntJoined.IsVisible = false;
                    card_teamHasJoined.IsVisible = true;
                    card_teamTeamName.Text = ServerCommunication.MyTeam.name;
                    card_teamLabel.Text = "Your Team";
                    card_teamTeamId.Text = "Code: " + ServerCommunication.MyTeam._id;
                    card_teamMembers.Text = ServerCommunication.MyTeam._memberIds.Length.ToString() + " members";
                }
                else
                {
                    await DisplayAlert("Uh oh", "Failed to join the team. Does it exist?", "Ok");
                }
            }
        }
        private async void CreateTeam(object sender, EventArgs e)
        {
            var teamId = await DisplayPromptAsync("Team Name", "Please enter in the team name that you wish to create", "Create", "Never Mind", "Your Amazing Team");
            if (teamId != "" && teamId != null)
            {
                var response = await s.CreateTeam(new Team()
                {
                    name = teamId,
                    _ownerId = ServerCommunication.MyUserInstance._id,
                    _memberIds = new string[] { ServerCommunication.MyUserInstance._id }
                }, ServerCommunication.MyUserInstance);
                if (!response.StartsWith("Failed"))
                {
                    ServerCommunication.MyTeam = JsonConvert.DeserializeObject<Team>(response);
                    card_teamHasntJoined.IsVisible = false;
                    card_teamHasJoined.IsVisible = true;
                    card_teamTeamName.Text = ServerCommunication.MyTeam.name;
                    card_teamLabel.Text = "Your Team";
                    card_teamTeamId.Text = "Code: " + ServerCommunication.MyTeam._id;
                    card_teamMembers.Text = ServerCommunication.MyTeam._memberIds.Length.ToString() + " members";
                }
                else
                {
                    await DisplayAlert("Uh oh", "Failed to create your team!", "Ok");
                }
            }
        }
        private async void SeeMyTeam(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MyTeam());
        }
        private async void SignOut(object sender, EventArgs e)
        {
            ServerCommunication.MyTeam = null;
            ServerCommunication.CachedRace = null;
            ServerCommunication.CachedRaces = null;
            ServerCommunication.MyUserInstance = null;
            Navigation.PushAsync(new Login());
        }

        private void joinRace_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new VirtualMap(currentRace));
        }
    }
}
