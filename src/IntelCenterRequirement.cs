using System;
using System.Collections;
using System.Reflection;
using Comfort.Common;
using EFT;

namespace Blackwell.RailTime;

internal static class IntelCenterRequirement
{
    public static bool ShouldDisplayTimer()
    {
        int requiredLevel = Plugin.RequiredIntelCenterLevel.Value;

        // Требование отключено.
        if (requiredLevel <= 0)
            return true;

        var world = Singleton<GameWorld>.Instantiated
            ? Singleton<GameWorld>.Instance
            : null;

        var player = world?.MainPlayer;

        // Не блокируем таймер, если игрок ещё не загружен.
        if (player == null)
            return true;

        var profile = ReadMember(player, "Profile");

        if (profile == null)
            return true;

        var side = ReadMember(profile, "Side")?.ToString();

        // Дикий всегда видит таймер.
        if (string.Equals(
            side,
            "Savage",
            StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        int currentLevel = GetIntelCenterLevel(profile);

        return currentLevel >= requiredLevel;
    }

    private static int GetIntelCenterLevel(object profile)
    {
        var hideout = ReadMember(profile, "Hideout");
        var areas = ReadMember(hideout, "Areas") as IEnumerable;

        if (areas == null)
            return 0;

        foreach (var area in areas)
        {
            if (area == null)
                continue;

            var type = ReadMember(area, "Type")?.ToString();

            if (!string.Equals(
                type,
                "IntelligenceCenter",
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var level = ReadMember(area, "Level");

            if (level == null)
                continue;

            if (int.TryParse(level.ToString(), out int result))
                return result;

            // На случай, если Level представлен вложенным объектом.
            var nestedLevel = ReadMember(level, "Level");

            if (nestedLevel != null &&
                int.TryParse(nestedLevel.ToString(), out result))
            {
                return result;
            }
        }

        return 0;
    }

    private static object? ReadMember(object? instance, string name)
    {
        if (instance == null)
            return null;

        const BindingFlags flags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        var type = instance.GetType();

        try
        {
            var property = type.GetProperty(name, flags);

            if (property != null)
                return property.GetValue(instance);

            var field = type.GetField(name, flags);

            return field?.GetValue(instance);
        }
        catch
        {
            return null;
        }
    }
}