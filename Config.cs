namespace Site12;

using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;
using UnityEngine;

public class Config : IConfig
{
    [Description("Set to true to enable the plugin once you setup all the configurations")]
    public bool ConfigurationComplete { get; set; } = false;
    [Description("Whether or not the plugin is enabled.")]
    public bool IsEnabled { get; set; } = true;

    [Description("Should debug logs be enabled?")]
    public bool Debug { get; set; } = false;

    [Description("Player Lobby Platform Spawn Location *Not the Schematic Spawning*")]
    public Vector3 LobbySpawnLocation { get; set; } = new(0,0,0);

    [Description("Lobby Schematic Name")]
    public string LobbySchematic { get; set; } = "exampleSchematic";

    [Description("Location while spawning whenever the user picks their role while lobby is enabled, other than Class-D")]
    public Vector3 PlayerSpawnLocation { get; set; } = new(0,0,0);

    [Description("List of Departments that exist")]
    public List<string> Departments { get; set; } = [
    "Security",
    "Research"
    ];

    [Description("Scom Word Blacklist")] 
    public List<string> BlackList { get; set; } = [];

    [Description("Whether or not the WeightSystem should be enabled/disabled or not.")] 
    public bool WeightSystem { get; set; } = false;

    [Description("Discord Webhook link for Department Logs")]
    public string URL { get; set; } = "Example URL";
}