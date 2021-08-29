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
    public partial class Login : ContentPage
    {
        private static ServerCommunication s = ServerCommunication.Instance;
        public Login()
        {
            InitializeComponent();
            loginCardAnimationOut = new Animation(v => loginCard.HeightRequest = v, 50, 100, Easing.CubicInOut);
            loginCardAnimationOut2 = new Animation(v => loginCard.HeightRequest = v, 100, 200, Easing.CubicInOut);
            loginCardAnimationInS1 = new Animation(v => loginCard.HeightRequest = v, 200, 100, Easing.CubicInOut);
            loginCardAnimationIn = new Animation(v => loginCard.HeightRequest = v, 100, 50, Easing.CubicInOut);
            submitAnimationOut = new Animation(v => submitContainer.HeightRequest = v, 0, 50, Easing.CubicInOut);
            submitAnimationIn = new Animation(v => submitContainer.HeightRequest = v, 50, 0, Easing.CubicInOut);
            
        }
        protected async override void OnAppearing()
        {
            if (!ServerCommunication.IsInitialized)
            {
                await s.Initialize();
                await s.LoadEverything();
            }
            if(ServerCommunication.MyUserInstance != null)
            {
                loading.IsVisible = true;
                var response = await s.CheckIfAccount(new LoginAttempt() { name = ServerCommunication.MyUserInstance.name, password = ServerCommunication.MyUserInstance.password });
                if (!response.StartsWith("Failed"))
                {
                    var userToHandle = JsonConvert.DeserializeObject<User>(response);
                    Navigation.PushAsync(new MainPage());
                    ServerCommunication.MyUserInstance = userToHandle;
                }
                else
                {
                    loading.IsVisible = false;
                }
            }
            base.OnAppearing();
        }
        private Animation loginCardAnimationOut;
        private Animation loginCardAnimationOut2;
        private Animation loginCardAnimationInS1;
        private Animation loginCardAnimationIn;
        private Animation submitAnimationOut;
        private Animation submitAnimationIn;
        private bool accountExists = false;
        private async void username_Completed(object sender, EventArgs e)
        {
            if(username.Text != null && username.Text != "")
            {
                
                if (loginCard.HeightRequest == 50)
                {
                    loginCard.Animate("out", loginCardAnimationOut, length: 500);
                    password.FadeTo(1, 500);
                    password.IsEnabled = true;
                }
                else if(loginCard.HeightRequest == 200)
                {
                    loginCard.Animate("out", loginCardAnimationInS1, length: 500);
                    password.FadeTo(1, 500);
                    password.IsEnabled = true;
                    registrationInformation.FadeTo(0, 250);
                    age.IsEnabled = false;
                    email.IsEnabled = false;
                }
                await Task.Delay(500);
                accountExists = await s.AccountExists(new LoginAttempt() { name = username.Text, password = null });
                if (!accountExists)
                {
                    loginCard.Animate("out", loginCardAnimationOut2, length: 500);
                    registrationInformation.FadeTo(1, 500);
                    age.IsEnabled = true;
                    email.IsEnabled = true;
                    register.Text = "Register";
                }
                else
                {
                    register.Text = "Login";
                }
                
            }
            else
            {
                if (loginCard.HeightRequest == 100)
                {
                    loginCard.Animate("in", loginCardAnimationIn, length: 500);
                    password.FadeTo(0, 500);
                    password.IsEnabled = false;
                }
                    
            }
        }

        private void password_Completed(object sender, EventArgs e)
        {
            if(password.Text != null && password.Text != "" && (accountExists || (age.Text != null && age.Text != "" && email.Text != null && email.Text != "")))
            {
                    submitContainer.FadeTo(1, 500);
                register.IsEnabled = true;
            }
            else
            {
                    submitContainer.FadeTo(0, 500);
                register.IsEnabled = false;
            }
        }

        private async void username_Unfocused(object sender, FocusEventArgs e)
        {
            if (username.Text != null && username.Text != "")
            {

                if (loginCard.HeightRequest == 50)
                {
                    loginCard.Animate("out", loginCardAnimationOut, length: 500);
                    password.FadeTo(1, 500);
                    password.IsEnabled = true;
                }
                else if (loginCard.HeightRequest == 200)
                {
                    loginCard.Animate("out", loginCardAnimationInS1, length: 500);
                    password.FadeTo(1, 500);
                    password.IsEnabled = true;
                    registrationInformation.FadeTo(0, 250);
                    age.IsEnabled = false;
                    email.IsEnabled = false;
                }
                await Task.Delay(500);
                accountExists = await s.AccountExists(new LoginAttempt() { name = username.Text, password = null });
                if (!accountExists)
                {
                    loginCard.Animate("out", loginCardAnimationOut2, length: 500);
                    registrationInformation.FadeTo(1, 500);
                    age.IsEnabled = true;
                    email.IsEnabled = true;
                }
            }
            else
            {
                if (loginCard.HeightRequest == 100)
                {
                    loginCard.Animate("in", loginCardAnimationIn, length: 500);
                    password.FadeTo(0, 500);
                    password.IsEnabled = false;
                }

            }
        }

        private void password_Unfocused(object sender, FocusEventArgs e)
        {
            if (password.Text != null && password.Text != "" && (accountExists || (age.Text != null && age.Text != "" && email.Text != null && email.Text != "")))
            {
                submitContainer.FadeTo(1, 500);
                register.IsEnabled = true;
            }
            else
            {
                submitContainer.FadeTo(0, 500);
                register.IsEnabled = false;
            }
        }
        private string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }
        private async void register_Clicked(object sender, EventArgs e)
        {
            if (accountExists)
            {
                var response = await s.CheckIfAccount(new LoginAttempt() { name = username.Text, password = password.Text });
                if (!response.StartsWith("Failed"))
                {
                    var userToHandle = JsonConvert.DeserializeObject<User>(response);
                    
                    Navigation.PushAsync(new MainPage());
                    ServerCommunication.MyUserInstance = userToHandle;
                    await s.SaveEverything();
                }
                else
                {

                }
            }
            else
            {
                var response = await s.AddAccount(new User() { email = email.Text, lastlogin = DateTime.Now, name = username.Text, phone = ";)", password = password.Text, logs = new int[] { }, racesCompleted = new string[] { } });
                if (!response.StartsWith("Failed"))
                {
                    var userToUse = JsonConvert.DeserializeObject<User>(response);
                    
                    Navigation.PushAsync(new MainPage());
                    ServerCommunication.MyUserInstance = new User() { _id = userToUse._id, email = email.Text, lastlogin = DateTime.Now, name = username.Text, phone = ";)", password = password.Text, logs = new int[] { }, racesCompleted = new string[] { } };
                    await s.SaveEverything();
                }
                else
                {

                }
            }
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            logo.ScaleTo(1.1, 250, Easing.CubicIn);
            await logo.RotateTo(new Random().Next(-20, 20), 250, Easing.CubicIn);
            logo.ScaleTo(1, 250, Easing.CubicOut);
            await logo.RotateTo(0, 250, Easing.CubicOut);
        }
    }
}