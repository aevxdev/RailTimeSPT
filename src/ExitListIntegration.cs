#pragma warning disable CS0618

using System;
using System.Linq;
using EFT;
using EFT.Interactive;
using EFT.MovingPlatforms;
using EFT.UI;
using EFT.UI.BattleTimer;
using HarmonyLib;

namespace Blackwell.RailTime;

internal static class ExitListIntegration
{
    private static AccessTools.FieldRef<ExitTimerPanel, ExfiltrationPoint> _extractionPoint = null!;
    private static AccessTools.FieldRef<ExitTimerPanel, CustomTextMeshProUGUI> _pointName = null!;

    public static void Apply(Harmony harmony)
    {
        _extractionPoint = AccessTools.FieldRefAccess<ExitTimerPanel, ExfiltrationPoint>("_point");
        _pointName = AccessTools.FieldRefAccess<ExitTimerPanel, CustomTextMeshProUGUI>("_pointName");

        var timerMethod = AccessTools.Method(typeof(ExitTimerPanel), nameof(ExitTimerPanel.SetTimerText), [typeof(TimeSpan)]);
        var activeMethod = AccessTools.Method(typeof(ExitTimerPanel), nameof(ExitTimerPanel.SetTimerTextActive));
        var visitedMethod = AccessTools.Method(typeof(ExitTimerPanel), nameof(ExitTimerPanel.UpdateVisitedStatus));

        harmony.Patch(timerMethod, postfix: new HarmonyMethod(typeof(ExitListIntegration), nameof(AfterTimerUpdate)));
        harmony.Patch(activeMethod, postfix: new HarmonyMethod(typeof(ExitListIntegration), nameof(AfterTimerVisibilityUpdate)));
        harmony.Patch(visitedMethod, postfix: new HarmonyMethod(typeof(ExitListIntegration), nameof(AfterVisitedUpdate)));
    }

    private static Locomotive? FindTrain(ExitTimerPanel panel)
    {
        if (!Plugin.IsEnabled.Value || !Plugin.ShowInExitList.Value)
            return null;

        var point = _extractionPoint(panel);
        if (point == null || !point.HasRequirements)
            return null;

        return point.Requirements.OfType<TrainRequirement>().FirstOrDefault()?.Locomotive;
    }

    private static void AfterTimerVisibilityUpdate(ExitTimerPanel __instance)
    {
        if (FindTrain(__instance) == null)
            return;

        if (!IntelCenterRequirement.ShouldDisplayTimer())
        {
            __instance.TimerText.gameObject.SetActive(false);
            return;
        }

        __instance.TimerText.gameObject.SetActive(true);
    }

    private static void AfterVisitedUpdate(ExitTimerPanel __instance)
    {
        if (FindTrain(__instance) == null)
            return;

        if (!IntelCenterRequirement.ShouldDisplayTimer())
            return;

        __instance._itemsObject?.SetActive(false);
        __instance._discountStats?.SetActive(false);
    }

    private static void AfterTimerUpdate(ExitTimerPanel __instance)
    {
        try
        {
            var train = FindTrain(__instance);
            if (train == null)
                return;
            if (!IntelCenterRequirement.ShouldDisplayTimer())
            {
                __instance.TimerText.gameObject.SetActive(false);
                return;
            }

            var status = TrainSchedule.Read(train);
            if (status.Stage == TrainStage.Unavailable)
                return;

            __instance.TimerText.gameObject.SetActive(true);
            __instance.TimerText.text = status.Stage == TrainStage.Departed
                ? "--:--"
                : TrainSchedule.FormatCountdown(status.Remaining);

            var name = _pointName(__instance);
            var point = _extractionPoint(__instance);
            if (name != null && point != null)
                name.text = $"{point.Settings.Name.Localized()} ({TrainSchedule.StageLabel(status.Stage)})";
        }
        catch (Exception ex)
        {
            Plugin.Log.LogWarning($"Failed to update train exit timer: {ex.Message}");
        }
    }
}