namespace Site12;

using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;
using UnityEngine;

public class Config : IConfig
{
    [Description("Whether or not the plugin is enabled.")]
    public bool IsEnabled { get; set; } = true;

    [Description("Should debug logs be enabled?")]
    public bool Debug { get; set; } = false;

    [Description("Lobby Platform Spawn Location")]
    public Vector3 LobbySpawnLocation = new(0,0,0);
    
    [Description("Lobby Schematic Name")]
    public string LobbySchematic = "exampleSchematic";

    [Description("Location while spawning whenever the user picks their role while lobby is enabled, other than Class-D")]
    public Vector3 PlayerSpawnLocation = new(0, 0, 0);
    
    [Description("List of Departments that exist")]
    public List<string> Departments { get; set; } = [];

    [Description("Scom Word Blacklist")] 
    public List<string> BlackList { get; set; } = [];
}