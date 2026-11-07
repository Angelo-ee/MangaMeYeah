using System.Windows;

namespace MangaMeYeah.Services;

public enum GestureAction
{
    None = 0,
    FullScreenToggle,
    AlignHeightTwoPage,
    FitWidth,
    ThumbnailCustom,
    JumpForwardFixed,
    JumpBackwardFixed,
    SpecifiedScale,
    QuickSave1,
    QuickSave2,
    QuickSave3,
    QuickSave4
}

public sealed class MouseGestureEngine
{
    private const double UnitLength = 30.0;
    private const double SplitPauseMs = 190.0;

    private readonly List<Point> _trailPoints = [];
    private readonly List<string> _directions = [];
    private Point _anchorPoint;
    private DateTime _lastMoveTimeUtc;
    private bool _pauseRecordedForRepeat;

    public IReadOnlyList<Point> TrailPoints => _trailPoints;
    public string CurrentGesturePattern => string.Concat(_directions);
    public bool HasMovedBeyondThreshold => _directions.Count > 0;

    public void Start(Point startPoint)
    {
        _trailPoints.Clear();
        _directions.Clear();
        _trailPoints.Add(startPoint);
        _anchorPoint = startPoint;
        _lastMoveTimeUtc = DateTime.UtcNow;
        _pauseRecordedForRepeat = false;
    }

    public bool AddPoint(Point pt)
    {
        var now = DateTime.UtcNow;
        if ((now - _lastMoveTimeUtc).TotalMilliseconds >= SplitPauseMs)
        {
            _pauseRecordedForRepeat = true;
            _anchorPoint = _trailPoints.Count > 0 ? _trailPoints[^1] : pt;
        }
        _lastMoveTimeUtc = now;
        _trailPoints.Add(pt);

        double dx = pt.X - _anchorPoint.X;
        double dy = pt.Y - _anchorPoint.Y;
        double dist = Math.Sqrt(dx * dx + dy * dy);

        if (dist < UnitLength)
        {
            return false;
        }

        string dir = Math.Abs(dx) >= Math.Abs(dy)
            ? (dx > 0 ? "→" : "←")
            : (dy > 0 ? "↓" : "↑");

        _anchorPoint = pt;

        if (_directions.Count == 0)
        {
            _directions.Add(dir);
            _pauseRecordedForRepeat = false;
            return true;
        }

        string lastDir = _directions[^1];
        if (lastDir != dir)
        {
            if (_directions.Count < 4)
            {
                _directions.Add(dir);
            }
            _pauseRecordedForRepeat = false;
            return true;
        }
        else if (_pauseRecordedForRepeat && _directions.Count == 1)
        {
            // Support →→ and ←← after a brief pause (MangaMeeya GestureSplitTime=200ms)
            _directions.Add(dir);
            _pauseRecordedForRepeat = false;
            return true;
        }

        return false;
    }

    public static (GestureAction Action, string Description) ResolveGesture(string pattern)
    {
        return pattern switch
        {
            "↑" => (GestureAction.FullScreenToggle, "切换全屏"),
            "↓" => (GestureAction.AlignHeightTwoPage, "统一高度 (用于双页显示)"),
            "←" => (GestureAction.FitWidth, "自动适应宽"),
            "→" => (GestureAction.ThumbnailCustom, "缩略图显示 (自定义)"),
            "→→" => (GestureAction.JumpForwardFixed, "前进固定页数"),
            "←←" => (GestureAction.JumpBackwardFixed, "后退固定页数"),
            "↑↓" or "↓↑" => (GestureAction.SpecifiedScale, "指定倍率显示 (170%)"),
            "↑←" or "←↑" => (GestureAction.QuickSave1, "快速保存 1"),
            "↑→" or "→↑" => (GestureAction.QuickSave2, "快速保存 2"),
            "↓←" or "←↓" => (GestureAction.QuickSave3, "快速保存 3"),
            "↓→" or "→↓" => (GestureAction.QuickSave4, "快速保存 4"),
            _ => (GestureAction.None, string.IsNullOrEmpty(pattern) ? "" : "未分配")
        };
    }
}
