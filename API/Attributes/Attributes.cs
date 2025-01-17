namespace Site12.API.Attributes;

using System;

/// <summary>
/// Attribute that specifies a method to call when the plugin is first enabled.
/// </summary>
/// <remarks>
/// The method must be static and with no parameters to be called.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class OnPluginEnabledAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class RegionAttribute : Attribute
{
    public string RegionName { get; private set; }

    public RegionAttribute(string regionName)
    {
        RegionName = regionName;
    }
}