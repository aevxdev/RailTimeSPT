using System;
using Comfort.Common;
using EFT;
using EFT.MovingPlatforms;
using UnityEngine;

namespace Blackwell.RailTime;

internal sealed class TrainOverlay : MonoBehaviour
{
    private Locomotive? _train;
    private GameWorld? _currentWorld;
    private float _nextLookup;
    private bool _isVisible = true;
    private GUIStyle? _style;
    private string _text = string.Empty;

    private void Update()
    {
        if (Plugin.OverlayToggle.Value.IsDown())
            _isVisible = !_isVisible;

        var world = Singleton<GameWorld>.Instantiated ? Singleton<GameWorld>.Instance : null;
        if (world != _currentWorld)
        {
            _currentWorld = world;
            _train = null;
            _nextLookup = 0f;
        }

        if (world == null || !Plugin.IsEnabled.Value || !Plugin.ShowOverlay.Value)
        {
            _text = string.Empty;
            return;
        }

        if (_train == null && Time.time >= _nextLookup)
        {
            _nextLookup = Time.time + 4f;
            _train = FindObjectOfType<Locomotive>();
        }

        if (!IntelCenterRequirement.ShouldDisplayTimer())
        {
            _text = string.Empty;
            return;
        }

        _text = _train == null
            ? string.Empty
            : TrainSchedule.OverlayText(TrainSchedule.Read(_train));
    }

    private void OnGUI()
    {
        if (!_isVisible || string.IsNullOrEmpty(_text))
            return;

        _style ??= new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
        _style.fontSize = Plugin.OverlayFontSize.Value;

        var content = new GUIContent(_text);
        var size = _style.CalcSize(content);
        const float margin = 14f;

        string pos = Plugin.OverlayPosition.Value;
        float x = pos.EndsWith("Left", StringComparison.OrdinalIgnoreCase) ? margin : Screen.width - size.x - margin;
        float y = pos.StartsWith("Top", StringComparison.OrdinalIgnoreCase) ? margin : Screen.height - size.y - margin;
        var rect = new Rect(x, y, size.x, size.y);

        // Тень и основной текст
        _style.normal.textColor = new Color(0f, 0f, 0f, 0.85f);
        GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), content, _style);
        _style.normal.textColor = Color.white;
        GUI.Label(rect, content, _style);
    }
}