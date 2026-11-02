using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace Blackwell.RailTime;

[BepInPlugin("com.blackwell.railtime", "blackwell-RailTime", "1.0.0")]
public sealed class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log = null!;

    internal static ConfigEntry<bool> IsEnabled = null!;
    internal static ConfigEntry<string> Language = null!;
    internal static ConfigEntry<bool> ShowInExitList = null!;
    internal static ConfigEntry<bool> ShowOverlay = null!;
    internal static ConfigEntry<int> RequiredIntelCenterLevel = null!;
    internal static ConfigEntry<string> OverlayPosition = null!;
    internal static ConfigEntry<int> OverlayFontSize = null!;
    internal static ConfigEntry<KeyboardShortcut> OverlayToggle = null!;

    private Harmony? _harmony;

    private void Awake()
    {
        Log = Logger;

        IsEnabled = Config.Bind("General", "Enabled", true, "Enable mod");
        Language = Config.Bind("General", "Language", "Russian", new ConfigDescription("Language", new AcceptableValueList<string>("Russian", "English")));
        ShowInExitList = Config.Bind("ExitList", "ShowTimer", true, "Show train timer in extraction list");
        ShowOverlay = Config.Bind("Overlay", "Enabled", false, "Show on-screen overlay");
        RequiredIntelCenterLevel = Config.Bind("Requirements", "RequiredIntelCenterLevel", 0, new ConfigDescription("Minimum Intel Center level required for PMC (0 = disabled).", new AcceptableValueRange<int>(0, 3)));
        OverlayPosition = Config.Bind("Overlay", "Position", "TopLeft", new ConfigDescription("Overlay position", new AcceptableValueList<string>("TopLeft", "TopRight", "BottomLeft", "BottomRight")));
        OverlayFontSize = Config.Bind("Overlay", "FontSize", 18, new ConfigDescription("Font size", new AcceptableValueRange<int>(10, 40)));
        OverlayToggle = Config.Bind("Overlay", "ToggleKey", new KeyboardShortcut(KeyCode.None), "Toggle overlay visibility");

        _harmony = new Harmony("blackwell.railtime");
        try
        {
            ExitListIntegration.Apply(_harmony);
        }
        catch (Exception ex)
        {
            Log.LogError($"Failed to patch ExitTimerPanel: {ex}");
        }

        gameObject.AddComponent<TrainOverlay>();
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }
}