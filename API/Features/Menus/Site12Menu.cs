namespace Site12.API.Features.Menus;

using System.Collections.Generic;
using System.Linq;
using Department;
using Exiled.API.Features;
using Extensions;
using MEC;
using PlayerRoles;
using UnityEngine;
using UserSettings.ServerSpecific;
using UserSettings.ServerSpecific.Entries;
using Random = System.Random;

/// <summary>
/// This example shows an ability to organize longer lists of entries by introducing a page selector.
/// <para /> This example uses auto-generated IDs, since it doesn't provide additional functionality, and reliability of saving isn't important here.
/// </summary>
public class Site12Menu
{
    private static Dictionary<int, Role> RolesToId = [];

    private class Role(string department, string roleName)
    {
        public string Department = department;
        public string RoleName = roleName;
    }

    private SSDropdownSetting _pageSelectorDropdown;

    private ServerSpecificSettingBase[] _pinnedSection;
    private SettingsPage[] _pages;
    private Dictionary<ReferenceHub, int> _lastSentPages;

    public void Activate()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived += ServerOnSettingValueReceived;
        ReferenceHub.OnPlayerRemoved += OnPlayerDisconnected;

        _lastSentPages = new Dictionary<ReferenceHub, int>();

        List<ServerSpecificSettingBase> lobbyMenuPage = [];
        var index = 0;
        foreach (var department in Department.DepartmentsData)
        {
            lobbyMenuPage.Add(new SSGroupHeader(department.Key));
            foreach (var role in department.Value.Roles.Where(role => department.Key != "Other" || role.RoleName == "ClassD"))
            {
                lobbyMenuPage.Add(new SSButton(index, role.RoleName, "Select Role...", 0.5f, role.Role.Description));
                if(!RolesToId.ContainsKey(index))
                    RolesToId.Add(index, new Role(department.Key, role.RoleName));
                index++;
            }
        }

        _pages = [new SettingsPage("Lobby", lobbyMenuPage.ToArray())];


        string[] dropdownPageOptions = new string[_pages.Length];

        for (int i = 0; i < dropdownPageOptions.Length; i++)
            dropdownPageOptions[i] = $"{_pages[i].Name} ({i + 1} out of {_pages.Length})";

        _pinnedSection =
        [
            _pageSelectorDropdown = new SSDropdownSetting(null, "Page", dropdownPageOptions, entryType: SSDropdownSetting.DropdownEntryType.HybridLoop),
        ];

        _pages.ForEach(page => page.GenerateCombinedEntries(_pinnedSection));

        // All settings must be included in DefinedSettings, even if we're only sending a small part at the time.
        List<ServerSpecificSettingBase> allSettings = new(_pinnedSection);
        _pages.ForEach(page => allSettings.AddRange(page.OwnEntries));
        ServerSpecificSettingsSync.DefinedSettings = allSettings.ToArray();

        // We're technically sending ALL settings here, but clients will immediately send back the response which will allow us to re-send only the portion they're interested in.
        // You can optimize this process by only sending the page selector, but I didn't want to complicate this example more than it needs to.
        ServerSpecificSettingsSync.SendToAll();
    }

    public void Deactivate()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived -= ServerOnSettingValueReceived;
        ReferenceHub.OnPlayerRemoved -= OnPlayerDisconnected;
    }

    private void ServerOnSettingValueReceived(ReferenceHub hub, ServerSpecificSettingBase setting)
    {
        switch (setting)
        {
            case SSDropdownSetting dropdown when dropdown.SettingId == _pageSelectorDropdown.SettingId:
                ServerSendSettingsPage(hub, dropdown.SyncSelectionIndexValidated);
                break;
            case SSButton potentialRole when RolesToId.TryGetValue(potentialRole.SettingId, out var role):
            {
                var player = ExPlayer.Get(hub);
                var setRole = Department.GetRole(role.RoleName, role.Department);

                if (!Lobby.IsRoleplay && !player.IsBypassModeEnabled)
                    break;
                if (!Lobby.IsLobby && !player.IsBypassModeEnabled)
                    break;

                if (setRole == player.ScomPlayer().CurrentRole.RoleEntry)
                    break;

                var setRank = setRole.Role.Ranks.Values.FirstOrDefault(r => r.Default);

                if (setRank == null) break;

                if (setRank.RequiresRankInRoster && !player.IsBypassModeEnabled)
                {
                    var data = DataPlayer.GetPlayer(player.UserId);
                    if (!DataPlayer.IsPlayerInDepartment(player.UserId, role.Department, out _))
                    {
                        player.ShowHint("You are required to join the department and have this rank or above.");
                        break;
                    }

                    if (!data.DepartmentData[role.Department].Roles.TryGetValue(setRole.RoleName, out var playerData))
                        break;

                    if (playerData < setRank.RankWeight && setRank.RequiresRankInRoster)
                    {
                        player.ShowHint("You are required to join the department and have this rank or above.");
                        break;
                    }
                }

                if (role.RoleName == "ClassD")
                {
                    if (!player.SetRole(role.RoleName, role.Department, RoleSpawnFlags.All)) break;
                }
                else
                {
                    if (!player.SetRole(role.RoleName, role.Department, RoleSpawnFlags.AssignInventory))
                        break;

                    Timing.CallDelayed(0.25f, () => player.Position = Plugin.Singleton.Config.PlayerSpawnLocation);
                }

                player.ShowHint("<size=16>" + setRole.Role.Description, 10f);
                break;
            }
        }
    }

    private void ServerSendSettingsPage(ReferenceHub hub, int settingIndex)
    {
        // Client automatically re-sends values of all the field after reception of the settings collection.
        // This can result in triggering this event, so we want to save the previously sent value to avoid going into infinite loops.
        if (_lastSentPages.TryGetValue(hub, out int prevSent) && prevSent == settingIndex)
            return;

        _lastSentPages[hub] = settingIndex;
        if (_pages[settingIndex].Name == "Lobby")
        {
            List<ServerSpecificSettingBase> lobbyMenuPage = [];
            var index = 0;
            foreach (var department in Department.DepartmentsData)
            {
                lobbyMenuPage.Add(new SSGroupHeader(department.Key));
                foreach (var role in department.Value.Roles.Where(role => department.Key != "Other" || role.RoleName == "ClassD"))
                {
                    lobbyMenuPage.Add(new SSButton(index, role.RoleName, "Select Role...", 0.5f, role.Role.Description));
                    if(!RolesToId.ContainsKey(index))
                        RolesToId.Add(index, new Role(department.Key, role.RoleName));
                    index++;
                }
            }

            _pages[settingIndex].OwnEntries = lobbyMenuPage.ToArray();
            _pages[settingIndex].GenerateCombinedEntries(_pinnedSection);
        }

        ServerSpecificSettingsSync.SendToPlayer(hub, _pages[settingIndex].CombinedEntries);
    }

    private void OnPlayerDisconnected(ReferenceHub hub)
    {
        _lastSentPages?.Remove(hub);
    }

    public class SettingsPage
    {
        public readonly string Name;
        
        public ServerSpecificSettingBase[] OwnEntries;

        public ServerSpecificSettingBase[] CombinedEntries { get; private set; }

        public SettingsPage(string name, ServerSpecificSettingBase[] entries)
        {
            Name = name;
            OwnEntries = entries;
        }

        public void GenerateCombinedEntries(ServerSpecificSettingBase[] pageSelectorSection)
        {
            int combinedLength = pageSelectorSection.Length + OwnEntries.Length + 1; // +1 to accomodate for auto-generated name header.
            CombinedEntries = new ServerSpecificSettingBase[combinedLength];

            int nextIndex = 0;

            // Include page selector section.
            foreach (ServerSpecificSettingBase entry in pageSelectorSection)
                CombinedEntries[nextIndex++] = entry;

            // Add auto-generated name header.
            CombinedEntries[nextIndex++] = new SSGroupHeader(Name);

            // Include own entries.
            foreach (ServerSpecificSettingBase entry in OwnEntries)
                CombinedEntries[nextIndex++] = entry;
        }
    }
}