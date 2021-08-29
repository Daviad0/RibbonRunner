using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KH21SE
{
    public class User
    {
        public string _id;
        public string name;
        public string email;
        public string phone;
        public DateTime lastlogin;
        public string password;
        public int[] logs;
    }
    public class Team
    {
        public string _id;
        public string name;
        public string _ownerId;
        public string[] _memberIds;
    }

    public class Race
    {
        public string _id;
        public string[] locations;
        public string name;
        public long date;
        public int lastUpdatedParticipants;
        public int meters;
    }

    public class UserRace
    {
        public string _userId;
        public string _raceId;
        public DateTime registered;
        public string _id;
        public string tshirt;
        public bool inperson;
        public string exercise;
    }
    public class LoginAttempt
    {
        public string name;
        public string password;
    }
    public class SendableLocation
    {
        public double Latitude;
        public double Longitude;
    }
    public sealed class ServerCommunication
    {
        public static bool IsInitialized = false;
        private static readonly ServerCommunication instance = new ServerCommunication();
        public static User MyUserInstance;
        public static List<UserRace> CachedRaces;
        public static Race CachedRace;
        public static Team MyTeam;
        private ServerCommunication()
        {

        }

        public static ServerCommunication Instance
        {
            get
            {
                return instance;
            }
        }
        private static HttpClient client = new HttpClient();

        public async Task<string> GetIfRegistered(User user, Race race)
        {
            try
            {
                object[] messageContents = { user, race };
                var stringContent = new StringContent(JsonConvert.SerializeObject(messageContents), Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PostAsync("isregistered", stringContent);
                res.EnsureSuccessStatusCode();
                return await res.Content.ReadAsStringAsync();
            }
            catch(Exception e)
            {
                return "Failure";
            }
            
        }

        
        public async Task<string> CheckIfAccount(LoginAttempt attempt)
        {
            try
            {

                var stringContent = new StringContent(JsonConvert.SerializeObject(attempt), Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PostAsync("login", stringContent);
                res.EnsureSuccessStatusCode();
                return await res.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                return "Failed!";
            }
        }

        public async Task<string> AddAccount(User userToAdd)
        {
            try
            {

                var stringContent = new StringContent(JsonConvert.SerializeObject(userToAdd), Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PostAsync("newaccount", stringContent);
                res.EnsureSuccessStatusCode();
                return await res.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                return "Failed!";
            }
        }

        public async Task<bool> AccountExists(LoginAttempt name)
        {
            try
            {
                var stringContent = new StringContent(JsonConvert.SerializeObject(name), Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PostAsync("isaccount", stringContent);
                res.EnsureSuccessStatusCode();
                return await res.Content.ReadAsStringAsync() == "Yes" ? true : false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public async Task<Race> GetRace()
        {
            try
            {
                HttpResponseMessage res = await client.GetAsync("currentrace");
                res.EnsureSuccessStatusCode();
                var stringContent = await res.Content.ReadAsStringAsync();
                var test = JsonConvert.DeserializeObject<Race>(stringContent);
                return test;
            }
            catch (Exception e)
            {
                return new Race();
            }
        }

        public async Task<string> CreateTeam(Team partialTeam, User user)
        {
            try
            {
                object[] messageContents = { partialTeam, user };
                var stringContent = new StringContent(JsonConvert.SerializeObject(messageContents), Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PostAsync("team/create", stringContent);
                res.EnsureSuccessStatusCode();
                return await res.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                return "Failed";
            }
        }
        public async Task<string> JoinTeam(Team partialTeam, User user)
        {
            try
            {
                object[] messageContents = { partialTeam, user };
                var stringContent = new StringContent(JsonConvert.SerializeObject(messageContents), Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PostAsync("team/join", stringContent);
                res.EnsureSuccessStatusCode();
                return await res.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                return "Failed";
            }
        }

        public async Task<string> GetTeam(User user)
        {
            try
            {
                var stringContent = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PostAsync("team/get", stringContent);
                res.EnsureSuccessStatusCode();
                return await res.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                return "Failed";
            }
        }

        
        public async Task<string> PostLocation(SendableLocation loc, User user)
        {
            try
            {
                object[] messageContents = { user, loc };
                var stringContent = new StringContent(JsonConvert.SerializeObject(messageContents), Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PostAsync("myloc", stringContent);
                res.EnsureSuccessStatusCode();
                return await res.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                return "Failed";
            }
        }
        public async Task<string> GetTeamLocation(User user, User targetUser)
        {
            try
            {
                object[] messageContents = { user, targetUser };
                var stringContent = new StringContent(JsonConvert.SerializeObject(messageContents), Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PostAsync("teamloc", stringContent);
                res.EnsureSuccessStatusCode();
                return await res.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                return "Failed";
            }
        }


        public async Task<string> NewRegistration(User user, Race race, UserRace userRace)
        {
            try
            {
                object[] messageContents = { user, race, userRace };
                var stringContent = new StringContent(JsonConvert.SerializeObject(messageContents), Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PostAsync("newregister", stringContent);
                res.EnsureSuccessStatusCode();
                return await res.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                return "Failed";
            }

        }
        public class PracticeLog
        {
            public DateTime time;
            public int meters;
        }
        public async Task<string> AddPracticeMeters(User user, int meters)
        {
            try
            {
                object[] messageContents = { user, new PracticeLog() { meters = meters, time = DateTime.Now } };
                var stringContent = new StringContent(JsonConvert.SerializeObject(messageContents), Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PostAsync("addpracticemeters", stringContent);
                res.EnsureSuccessStatusCode();
                return await res.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                return "Failed";
            }

        }
        public async Task<bool> Initialize()
        {
            IsInitialized = true;
            client.Timeout = TimeSpan.FromSeconds(3);
            client.BaseAddress = new Uri("https://KH21SEServer.daveeddigs.repl.co/");
            return true;
        }
    }
}

