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
    public class LeaderboardIn
    {
        public string username;
        public int score;
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyTeam : ContentPage
    {
        private static ServerCommunication s = ServerCommunication.Instance;
        public MyTeam()
        {
            InitializeComponent();
        }
        private async void GetLeaderboard()
        {
            var listOfEntries = new List<LeaderboardIn>();
            foreach(var member in ServerCommunication.MyTeam._memberIds)
            {
                var response = await s.GetLeaderboardUser(new User() { _id = member });
                if (!response.StartsWith("Failed"))
                {
                    var objectToUse = JsonConvert.DeserializeObject<LeaderboardIn>(response);
                    listOfEntries.Add(objectToUse);
                }
            }
            int i = 0;
            int wholeTotal = 0;
            leaderboard.Children.Clear();
            foreach (var entry in listOfEntries.OrderByDescending(o => o.score))
            {

                /*
                 
                 
                 <Frame BackgroundColor="#160F29" Padding="5,0" WidthRequest="20" HeightRequest="20" HorizontalOptions="Start" CornerRadius="4">
                                <Label Text="1" Grid.Column="0" HorizontalOptions="Center" VerticalOptions="Center" TextColor="White" FontSize="20" FontAttributes="Bold"/>
                            </Frame>

                            <Label Text="12345678" Grid.Column="1" HorizontalOptions="Center" VerticalOptions="Center" TextColor="#160F29" FontSize="20" FontAttributes="Bold"/>
                            <Label Text="2.1k" Grid.Column="2" HorizontalOptions="End" VerticalOptions="Center" TextColor="#160F29" FontSize="20"/>
                 */
                wholeTotal += entry.score;
                var aFrameToAdd = new Frame() { BackgroundColor = Color.FromHex("#160F29"), Padding = new Thickness(5, 0), WidthRequest = 20, HeightRequest = 20, HorizontalOptions = LayoutOptions.Start, CornerRadius = 4 };
                aFrameToAdd.Content = (new Label() { Text = (i + 1).ToString(), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, TextColor = Color.FromHex("#FFFFFF"), FontSize = 20, FontAttributes = FontAttributes.Bold });
                var label1 = new Label() { Text = entry.username, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, TextColor = Color.FromHex("#160F29"), FontSize = 20, FontAttributes = FontAttributes.Bold };
                var label2 = new Label() { Text = Math.Round((double)entry.score/1000,1).ToString() + "km", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, TextColor = Color.FromHex("#160F29"), FontSize = 20, FontAttributes = FontAttributes.Bold };
                leaderboard.Children.Add(aFrameToAdd, 0, i);
                leaderboard.Children.Add(label1, 1, i);
                leaderboard.Children.Add(label2, 2, i);
                i++;
            }
            if(wholeTotal > 100000)
            {
                awardImage.Source = "Badge_3.png";
                bigAward.Source = "Badge_3.png";
                awardDesc.Text = "This badge is earned when a team achieves a total of 100 kilometers as a token of appreciation for their dedication to the cause. Thanks so much!";
                trophyCase.IsVisible = true;
            }
            else if(wholeTotal > 20000 && i > 2)
            {
                awardImage.Source = "Badge_1.png";
                bigAward.Source = "Badge_1.png";
                trophyCase.IsVisible = true;
                awardDesc.Text = "The super speedy award is earned by getting 4 members to get a 5k! Wow, much fast!";
            }
            else if (i > 8)
            {
                awardImage.Source = "Badge_2.png";
                bigAward.Source = "Badge_2.png";
                awardDesc.Text = "This badge is earned by having at least 10 members on your team as a mark of your dedication to recruit members for the cause!";
                trophyCase.IsVisible = true;
            }
            else
            {
                trophyCase.IsVisible = false;
            }
        }
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            if (ServerCommunication.MyTeam != null)
            {
                teamName.Text = ServerCommunication.MyTeam.name;
                teamCode.Text = ServerCommunication.MyTeam._id;
                GetLeaderboard();
            }
            else
            {
                var response = await s.GetTeam(ServerCommunication.MyUserInstance);
                if (!response.StartsWith("Failed"))
                {
                    ServerCommunication.MyTeam = JsonConvert.DeserializeObject<Team>(response);
                    teamName.Text = ServerCommunication.MyTeam.name;
                    teamCode.Text = ServerCommunication.MyTeam._id;
                    GetLeaderboard();
                }
                else
                {
                    Navigation.PopAsync();
                }
            }
            
        }
        private async void JoinTeam(object sender, EventArgs e)
        {
            var teamId = await DisplayPromptAsync("Team ID", "Please enter in the team ID that you wish to join", "Join", "Never Mind", "ABCDEFGH", 8);
            if (teamId != "" && teamId != null)
            {
                var response = await s.JoinTeam(new Team()
                {
                    _id = teamId
                }, ServerCommunication.MyUserInstance);
                if (!response.StartsWith("Failed"))
                {
                    ServerCommunication.MyTeam = JsonConvert.DeserializeObject<Team>(response);
                    teamName.Text = ServerCommunication.MyTeam.name;
                    teamCode.Text = ServerCommunication.MyTeam._id;
                    GetLeaderboard();

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
                    teamName.Text = ServerCommunication.MyTeam.name;
                    teamCode.Text = ServerCommunication.MyTeam._id;
                    GetLeaderboard();
                }
                else
                {
                    await DisplayAlert("Uh oh", "Failed to create your team!", "Ok");
                }
            }
        }
        private void GoToHome(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void ExpandBigImage(object sender, EventArgs e)
        {
            bigAwardC.IsVisible = true;
        }
        private void ContractBigImage(object sender, EventArgs e)
        {
            bigAwardC.IsVisible = false;
        }
    }
}