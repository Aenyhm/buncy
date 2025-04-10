namespace Module2;

public enum LogType : byte {
    Info,
    Success,
    Error,
}

public class Notification {
    public readonly LogType LogType;
    public readonly string Text;
    public float Elapsed { get; set; }
    
    public Notification(LogType logType, string text) {
        LogType = logType;
        Text = text;
    }
}

public static class NotificationManager {
    private const float _duration = 2f;
    private static readonly Dictionary<LogType, Color> _colorsByLogType = new() {
        { LogType.Info, Color.RayWhite },
        { LogType.Success, Color.Green },
        { LogType.Error, Color.Red },
    };

    private static readonly List<Notification> _notifs = [];
    
    public static void Add(Notification item) {
        _notifs.Add(item);
    }

    public static void Update(float dt) {
        for (var i = 0; i < _notifs.Count; i++) {
            var notif = _notifs[i];
            notif.Elapsed += dt;
            
            if (notif.Elapsed >= _duration) {
                _notifs.RemoveAt(i);
            }
        }
    }

    public static void Draw() {
        foreach (var notif in _notifs) {
            var timeRatio = notif.Elapsed/_duration;
            var fontSize = 64*(1 - timeRatio/10);
            var font = FontService.Instance.Get("default");
            var textSize = MeasureTextEx(font, notif.Text, fontSize, 0);
            var pos = new Vec2F32((Window.Size.X - textSize.X)/2, textSize.Y);
            pos.Y -= timeRatio*fontSize;
            var color = _colorsByLogType[notif.LogType];
            color.A = (byte)((1 - timeRatio)*255);
            
            var shadowColor = Color.Black;
            shadowColor.A = color.A;
            DrawTextPro(font, notif.Text, pos + Vec2F32.One, Vec2F32.Zero, 0, fontSize, 0, shadowColor);
            DrawTextPro(font, notif.Text, pos, Vec2F32.Zero, 0, fontSize, 0, color);
        }
    }
}
