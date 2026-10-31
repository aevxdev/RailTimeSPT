using System;
using EFT;
using EFT.MovingPlatforms;
using HarmonyLib;

namespace Blackwell.RailTime;

internal enum TrainStage
{
    Unavailable,
    Approaching,
    AtStation,
    Departed
}

internal readonly struct TrainStatus
{
    public readonly TrainStage Stage;
    public readonly TimeSpan Remaining;

    public TrainStatus(TrainStage stage, TimeSpan remaining)
    {
        Stage = stage;
        Remaining = remaining;
    }
}

internal static class TrainSchedule
{
    private static readonly AccessTools.FieldRef<MovingPlatform, DateTime> DepartureField =
        AccessTools.FieldRefAccess<MovingPlatform, DateTime>("_depart");

    private static bool IsRussian => Plugin.Language.Value.Equals("Russian", StringComparison.OrdinalIgnoreCase);

    public static TrainStatus Read(Locomotive locomotive)
    {

        var departure = DepartureField(locomotive);
        var offsets = locomotive._travelStateTiming;
        int arrivedIdx = (int)Locomotive.ETravelState.Arrived;
        int departingIdx = (int)Locomotive.ETravelState.Departing;

        if (departure == default || offsets == null || offsets.Length <= departingIdx || offsets.Length <= arrivedIdx)
            return new TrainStatus(TrainStage.Unavailable, TimeSpan.Zero);

        var now = DateTimeExtensions.UtcNow;
        var arrival = departure.AddSeconds(offsets[arrivedIdx]);
        var depart = departure.AddSeconds(offsets[departingIdx]);

        if (now < arrival)
            return new TrainStatus(TrainStage.Approaching, arrival - now);

        if (now < depart)
            return new TrainStatus(TrainStage.AtStation, depart - now);

        return new TrainStatus(TrainStage.Departed, TimeSpan.Zero);
    }

    public static string FormatCountdown(TimeSpan value)
    {
        if (value < TimeSpan.Zero)
            value = TimeSpan.Zero;

        return value.TotalHours >= 1
            ? $"{(int)value.TotalHours:00}:{value.Minutes:00}:{value.Seconds:00}"
            : $"{value.Minutes:00}:{value.Seconds:00}";
    }

    public static string StageLabel(TrainStage stage) => stage switch
    {
        TrainStage.Approaching => IsRussian ? "прибудет через" : "arrives in",
        TrainStage.AtStation => IsRussian ? "отправится через" : "leaves in",
        TrainStage.Departed => IsRussian ? "уже уехал" : "has departed",
        _ => string.Empty
    };

    public static string OverlayText(TrainStatus status)
    {
        if (status.Stage == TrainStage.Unavailable)
            return string.Empty;

        string trainName = IsRussian ? "Бронепоезд" : "Armored train";
        string label = StageLabel(status.Stage);

        return status.Stage == TrainStage.Departed
            ? $"{trainName} {label}"
            : $"{trainName} {label} {FormatCountdown(status.Remaining)}";
    }
}