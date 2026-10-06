    }

    private static readonly Color DefaultBarColor = new Color(0.30f, 0.30f, 0.30f, 0.68f);
    private static readonly Color WindowFillColor = new Color(0f, 0f, 0f, 0.55f);
    private static readonly Color FireBarColor = new Color(0.62f, 0.18f, 0.18f, 0.68f);
    private static readonly Color IceBarColor = new Color(0.18f, 0.38f, 0.68f, 0.68f);
    private static readonly Color LightBarColor = new Color(0.68f, 0.60f, 0.16f, 0.68f);
    private static readonly Color DarkBarColor = new Color(0.40f, 0.18f, 0.52f, 0.68f);
    private static readonly Color AdScalingBarColor = new Color(0.55f, 0.36f, 0.18f, 0.68f);
    private static readonly Color ApScalingBarColor = new Color(0.18f, 0.50f, 0.55f, 0.68f);
    private static readonly Color HpScalingBarColor = new Color(0.36f, 0.55f, 0.22f, 0.68f);
    private static readonly Color HealingBarColor = new Color(0.22f, 0.62f, 0.30f, 0.68f);
    private static readonly Color SourceNameColor = new Color(0.97f, 0.97f, 0.97f, 1f);
    private const string DevelopmentVersion = "v4.90";

    private DpsData _data;
    private Vector2 _scroll;
    private DisplayMode _mode;

    private Rect _windowRect = new Rect(20f, 20f, 260f, 197f);
    private bool _dragging;
    private bool _resizing;
    private Vector2 _dragOffset;
    private Vector2 _resizeStartMouse;
    private Vector2 _resizeStartSize;
    private bool _resizeMoved;