using Godot;

namespace LivingBunker;

/// <summary>A single cutaway projection: every visible depth edge uses the same vector.</summary>
public partial class FirstScreenWorld : Node2D
{
    private const int Cell = 20;
    private const int GroundY = 620;
    private const int FloorY = 900;
    private static readonly Vector2 Depth = new(64, -64);
    private const int RoofRise = 20;

    private static readonly Color Ink = new("#1b2930");
    private static readonly Color Edge = new("#a8b3ac");
    private static readonly Color Steel = new("#62747a");
    private static readonly Color Side = new("#35464e");
    private static readonly Color Floor = new("#7c786f");
    private static readonly Color Amber = new("#ffbf65");
    private static readonly Color Glass = new("#72c6c7");

    private Texture2D? _panorama;
    private bool _showGrid;
    private bool _corridorCleared;
    private bool _workshopPreviewed;
    private bool _workshopBuilt;
    private bool _workbenchInstalled;
    private int _workbenchLevel;
    private string _expeditionPhase = "none";
    private float _heroDepartureProgress;
    private bool _bunkerLooted;
    private bool _residentsCaptured;
    private bool _reactorDestroyed;
    private bool _heroReturned;

    public override void _Ready()
    {
        _panorama = GD.Load<Texture2D>("res://assets/world-layout-background-hill.png");
        QueueRedraw();
    }

    public void ToggleGrid()
    {
        _showGrid = !_showGrid;
        QueueRedraw();
    }

    public void SetConstructionState(bool cleared, bool previewed, bool built, bool benchInstalled, int benchLevel)
    {
        if (_corridorCleared == cleared && _workshopPreviewed == previewed &&
            _workshopBuilt == built && _workbenchInstalled == benchInstalled && _workbenchLevel == benchLevel)
            return;
        _corridorCleared = cleared;
        _workshopPreviewed = previewed;
        _workshopBuilt = built;
        _workbenchInstalled = benchInstalled;
        _workbenchLevel = benchLevel;
        QueueRedraw();
    }

    public void SetExpeditionState(string phase, double secondsLeft)
    {
        float progress = phase == "outbound" ? Mathf.Clamp(1f - (float)secondsLeft / 45f, 0f, 1f) : 0f;
        if (_expeditionPhase == phase && Mathf.Abs(_heroDepartureProgress - progress) < 0.01f) return;
        _expeditionPhase = phase;
        _heroDepartureProgress = progress;
        QueueRedraw();
    }

    public void SetAftermathState(bool looted, bool captured, bool destroyed, bool heroReturned)
    {
        if (_bunkerLooted == looted && _residentsCaptured == captured &&
            _reactorDestroyed == destroyed && _heroReturned == heroReturned) return;
        _bunkerLooted = looted;
        _residentsCaptured = captured;
        _reactorDestroyed = destroyed;
        _heroReturned = heroReturned;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_panorama is not null)
            DrawTextureRect(_panorama, new Rect2(0, 106, 2000, 1126), false);

        DrawGround();
        DrawEntranceGroup();
        DrawBunkerEntrance();
        DrawRuinedHouse();
        DrawCheckpoint();
        DrawHedgehog();
        DrawFlag();
        if (_expeditionPhase is "none" or "outbound" or "arrived") DrawHero();
        DrawUnderground();
        if (_bunkerLooted || _residentsCaptured || _reactorDestroyed) DrawAftermath();
        if (_showGrid) DrawGrid();
    }

    private void DrawAftermath()
    {
        if (_bunkerLooted)
        {
            for (int i = 0; i < 4; i++)
                Rect(990 + i * 48, 589 - i * 4, 33, 27, new Color("#735a43"), Ink, 2);
        }
        if (_residentsCaptured)
        {
            for (int i = 0; i < 5; i++)
            {
                float x = 1250 + i * 31;
                DrawCircle(new Vector2(x, 573), 7, new Color("#b7a293"));
                DrawLine(new Vector2(x, 581), new Vector2(x, 613), Ink, 6);
                DrawLine(new Vector2(x - 10, 595), new Vector2(x + 10, 595), Ink, 3);
            }
        }
        if (!_reactorDestroyed) return;
        DrawColoredPolygon([new Vector2(390, 620), new Vector2(430, 545), new Vector2(465, 579),
            new Vector2(490, 522), new Vector2(550, 620)], new Color("#353d40"));
        DrawColoredPolygon([new Vector2(820, 900), new Vector2(860, 820), new Vector2(900, 853),
            new Vector2(940, 803), new Vector2(1000, 900)], new Color("#303b40"));
        DrawCircle(new Vector2(920, 716), 49, new Color(0.22f, 0.27f, 0.29f, 0.65f));
        DrawCircle(new Vector2(955, 680), 38, new Color(0.22f, 0.27f, 0.29f, 0.47f));
        DrawLine(new Vector2(265, 534), new Vector2(327, 592), Ink, 8); // damaged lift
    }

    private void DrawGround()
    {
        Face([new(0, GroundY), new(2000, GroundY), new(2000, GroundY + 25), new(0, GroundY + 25)], new Color("#85694f"));
        Rect(0, GroundY + 25, 2000, 34, new Color("#3e4648"), Ink, 2);
        Face([new(1080, GroundY), new(2000, GroundY), new(2000, GroundY - 28), new(1100, GroundY - 28)], new Color("#9c785b"), 2);
        for (int x = 1130; x < 2000; x += 90)
            DrawLine(new Vector2(x, GroundY), new Vector2(x + 20, GroundY - 28), new Color("#705947"), 2);
        DrawLine(new Vector2(1100, 340), new Vector2(1100, GroundY), new Color(0.42f, 0.76f, 0.79f, 0.34f), 3);
    }

    private void DrawEntranceGroup()
    {
        DrawVolume(40, 480, 340, 140, new Color("#93836d"));
        DrawInset(64, 500, 132, 120, new Color("#34444a"));
        DrawInset(236, 500, 124, 120, new Color("#34444a"));
        DrawStairs(78, 602, 102, 92);
        DrawElevator(257, 506, 82, 114);
        DrawLine(new Vector2(218, 487), new Vector2(218, 620), Ink, 7);
        DrawLight(363, 530, 10, 60);
        DrawFrontLip(40, GroundY, 340);
    }

    private void DrawBunkerEntrance()
    {
        DrawVolume(400, 500, 150, 120, new Color("#637278"));
        DrawInset(416, 511, 118, 109, new Color("#26353c"));
        DrawDoor(434, 510, 76, 110, false);
        DrawLight(519, 535, 10, 54);
        Rect(445, 490, 55, 8, Amber, Ink, 2);
        DrawFrontLip(400, GroundY, 150);
    }

    private void DrawRuinedHouse()
    {
        // Two straight structural sections and a broken corner keep the ruin readable.
        DrawVolume(600, 506, 153, 114, new Color("#696d69"));
        DrawVolume(766, 486, 170, 134, new Color("#777a74"));
        DrawInset(634, 523, 62, 76, new Color("#26343a"));
        DrawInset(800, 511, 62, 79, new Color("#26343a"));
        DrawLine(new Vector2(726, 507), new Vector2(713, 608), Ink, 7);
        DrawLine(new Vector2(901, 488), new Vector2(889, 611), Ink, 7);
        Face([new(931, 620), new(944, 567), new(965, 584), new(980, 548), new(1005, 620)], Side);
        Face([new(947, 620), new(973, 592), new(989, 620)], Edge);
        DrawFrontLip(600, GroundY, 390);
    }

    private void DrawCheckpoint()
    {
        DrawVolume(1140, 514, 106, 106, new Color("#516168"));
        DrawInset(1157, 533, 71, 43, new Color("#2a454a"));
        Rect(1167, 538, 51, 29, Glass, Ink, 3);
        DrawDoor(1170, 580, 43, 40, false);
        DrawLight(1128, 549, 9, 49);
        DrawFrontLip(1140, GroundY, 106);
    }

    private void DrawHedgehog()
    {
        Vector2 centre = new(1286, 599);
        DrawLine(centre + new Vector2(-21, 16), centre + new Vector2(26, -28), Ink, 10);
        DrawLine(centre + new Vector2(-23, -27), centre + new Vector2(25, 17), Edge, 10);
        DrawLine(centre + new Vector2(0, -40), centre + new Vector2(0, 22), Steel, 9);
        DrawCircle(centre, 7, Ink);
    }

    private void DrawFlag()
    {
        DrawLine(new Vector2(1340, GroundY), new Vector2(1340, 408), Ink, 7);
        Face([new(1342, 418), new(1392, 434), new(1342, 455)], new Color("#be7858"));
        DrawCircle(new Vector2(1340, 403), 7, Amber);
    }

    private void DrawHero()
    {
        float x = 566 + _heroDepartureProgress * 800;
        DrawColoredPolygon([new(x - 15, 618), new(x + 18, 618), new(x + 29, 627), new(x - 4, 627)], new Color(0.05f, 0.08f, 0.09f, 0.45f));
        DrawLine(new Vector2(x - 4, 599), new Vector2(x - 5, 619), Ink, 6);
        DrawLine(new Vector2(x + 6, 599), new Vector2(x + 10, 619), Ink, 6);
        Face([new(x - 11, 576), new(x + 11, 576), new(x + 13, 603), new(x - 12, 603)], new Color("#b9774d"));
        Rect(x - 16, 579, 8, 24, Side, Ink, 2);
        DrawLine(new Vector2(x + 12, 583), new Vector2(x + 19, 599), Ink, 6);
        DrawCircle(new Vector2(x, 566), 10, new Color("#e6b58c"));
        Face([new(x - 12, 562), new(x - 9, 553), new(x + 10, 553), new(x + 13, 562)], Side);
    }

    private void DrawUnderground()
    {
        Rect(0, 653, 2000, 38, new Color("#343f44"), Ink, 3);
        DrawLine(new Vector2(0, 672), new Vector2(2000, 672), new Color("#526168"), 2);

        DrawVolume(40, 720, 340, FloorY - 720, new Color("#45545a"));
        DrawInset(64, 742, 132, 158, new Color("#314149"));
        DrawInset(236, 742, 124, 158, new Color("#314149"));
        DrawStairs(78, 882, 102, 125);
        DrawElevator(257, 760, 82, 140);
        DrawLine(new Vector2(218, 727), new Vector2(218, FloorY), Ink, 7);
        DrawFrontLip(40, FloorY, 340);

        DrawVolume(400, 720, 550, FloorY - 720, new Color("#455551"));
        DrawLine(new Vector2(420, 839), new Vector2(926, 839), new Color("#65736f"), 3);
        DrawLight(431, 760, 10, 52);
        DrawLight(905, 760, 10, 52);
        DrawLine(new Vector2(516, 844), new Vector2(596, 844), Amber, 3);
        DrawDoor(540, 840, 40, 60, false);
        DrawLight(589, 853, 8, 32);
        DrawFrontLip(400, FloorY, 550);
        if (_corridorCleared) DrawWorkshopCorridor();
        else DrawRubble();
    }

    private void DrawWorkshopCorridor()
    {
        DrawVolume(950, 720, 150, FloorY - 720, new Color("#455551"));
        DrawLine(new Vector2(950, 839), new Vector2(1100, 839), new Color("#65736f"), 3);
        DrawLight(1080, 760, 10, 52);

        // Four cells of wall (x=48..51); the two middle cells hold the door.
        if (_workshopBuilt)
        {
            DrawLine(new Vector2(960, 738), new Vector2(1040, 738), Amber, 4);
            DrawLine(new Vector2(960, 738), new Vector2(960, 840), Amber, 2);
            DrawLine(new Vector2(1040, 738), new Vector2(1040, 840), Amber, 2);
            DrawDoor(980, 840, 40, 60, false);
            if (_workbenchInstalled)
            {
                Color badge = _workbenchLevel > 0 ? Glass : Amber;
                Rect(993, 817, 15, 9, badge, Ink, 2);
            }
        }
        else if (_workshopPreviewed)
        {
            DrawRect(new Rect2(960, 738, 80, 162), Amber, false, 2);
            DrawRect(new Rect2(980, 840, 40, 60), Glass, false, 2);
        }
        DrawFrontLip(950, FloorY, 150);
    }

    private void DrawRubble()
    {
        Face([new(945, FloorY), new(951, 854), new(970, 839), new(984, FloorY)], Side);
        Face([new(962, FloorY), new(983, 866), new(1005, 848), new(1038, FloorY)], Steel);
        Face([new(992, FloorY), new(1016, 870), new(1044, 862), new(1055, FloorY)], new Color("#4b5555"));
        Face([new(952, FloorY), new(969, 884), new(989, FloorY)], Edge);
        DrawLine(new Vector2(943, 855), new Vector2(958, 828), Ink, 5);
    }

    private void DrawStairs(float x, float bottomY, float width, float rise)
    {
        const int steps = 6;
        for (int i = 0; i < steps; i++)
        {
            float sx = x + i * width / steps;
            float sy = bottomY - i * rise / steps;
            DrawLine(new Vector2(sx, sy), new Vector2(sx + width / steps, sy), Edge, 6);
            DrawLine(new Vector2(sx + width / steps, sy), new Vector2(sx + width / steps, sy - rise / steps), Ink, 4);
        }
        DrawLine(new Vector2(x - 2, bottomY - 5), new Vector2(x + width, bottomY - rise - 6), Ink, 3);
    }

    private void DrawElevator(float x, float y, float width, float height)
    {
        DrawDoor(x, y, width, height, true);
        Rect(x + width / 2 - 7, y + 13, 14, 6, Amber, Ink, 1);
        Rect(x + width + 8, y + 45, 8, 13, Amber, Ink, 2);
    }

    private void DrawDoor(float x, float y, float width, float height, bool elevator)
    {
        Face([new(x + width, y), new(x + width + 14, y - 14), new(x + width + 14, y + height - 14), new(x + width, y + height)], Side);
        Face([new(x, y), new(x + 14, y - 14), new(x + width + 14, y - 14), new(x + width, y)], Edge);
        Rect(x, y, width, height, Edge, Ink, 5);
        Rect(x + 8, y + 8, width - 16, height - 8, elevator ? new Color("#394b52") : new Color("#465a61"), Ink, 3);
        DrawLine(new Vector2(x + width / 2, y + 9), new Vector2(x + width / 2, y + height - 1), Ink, elevator ? 4 : 2);
        if (!elevator) DrawCircle(new Vector2(x + width - 13, y + height / 2), 3, Amber);
    }

    private void DrawInset(float x, float y, float width, float height, Color back)
    {
        const float inset = 16;
        Rect(x + inset, y - inset, width - inset, height, back, Ink, 2);
        Face([new(x, y), new(x + inset, y - inset), new(x + inset, y + height - inset), new(x, y + height)], Side);
        Face([new(x, y + height), new(x + width, y + height), new(x + width, y + height - inset), new(x + inset, y + height - inset)], Floor, 2);
        DrawLine(new Vector2(x, y), new Vector2(x + width, y), Edge, 5);
        DrawLine(new Vector2(x, y), new Vector2(x, y + height), Edge, 5);
    }

    private void DrawVolume(float x, float y, float width, float height, Color back)
    {
        float backTop = y - RoofRise;
        float backBottom = y + height + Depth.Y;
        Rect(x + Depth.X, backTop, width, backBottom - backTop, back, Ink, 3);
        Face([new(x + width, y), new(x + width, y + height), new(x + width + Depth.X, backBottom), new(x + width + Depth.X, backTop)], Side);
        Face([new(x, y), new(x + width, y), new(x + width + Depth.X, backTop), new(x + Depth.X, backTop)], Edge);
        DrawFloorPlane(x, y + height, width);
        DrawLine(new Vector2(x, y), new Vector2(x, y + height), Edge, 8);
        DrawLine(new Vector2(x + width, y), new Vector2(x + width, y + height), Edge, 8);
        DrawLine(new Vector2(x, y), new Vector2(x + width, y), Edge, 8);
    }

    private void DrawFloorPlane(float x, float frontY, float width)
    {
        Face([new(x, frontY), new(x + width, frontY), new(x + width + Depth.X, frontY + Depth.Y), new(x + Depth.X, frontY + Depth.Y)], Floor, 2);
        Color joints = new(0.16f, 0.23f, 0.25f, 0.45f);
        for (float dx = 48; dx < width; dx += 48)
            DrawLine(new Vector2(x + dx, frontY), new Vector2(x + dx + Depth.X, frontY + Depth.Y), joints, 2);
    }

    private void DrawFrontLip(float x, float y, float width)
    {
        Rect(x, y, width, 18, new Color("#344149"), Ink, 3);
        DrawLine(new Vector2(x + 4, y + 3), new Vector2(x + width - 4, y + 3), Edge, 2);
        Face([new(x + width, y), new(x + width + 12, y - 12), new(x + width + 12, y + 6), new(x + width, y + 18)], Side);
    }

    private void DrawLight(float x, float y, float width, float height)
    {
        Rect(x, y, width, height, Amber, Ink, 3);
        DrawColoredPolygon([new(x + width, y + height), new(x + width + 25, y + height + 7), new(x + 5, y + height + 7)], new Color(1f, 0.72f, 0.34f, 0.15f));
    }

    private void DrawGrid()
    {
        Color grid = new(0.82f, 0.94f, 0.92f, 0.22f);
        for (int x = 0; x <= 2000; x += Cell)
            DrawLine(new Vector2(x, 140), new Vector2(x, 1120), grid, x % (Cell * 5) == 0 ? 2 : 1);
        for (int y = 140; y <= 1120; y += Cell)
            DrawLine(new Vector2(0, y), new Vector2(2000, y), grid, y % (Cell * 5) == 0 ? 2 : 1);
    }

    private void Rect(float x, float y, float width, float height, Color fill, Color border, float borderWidth)
    {
        DrawRect(new Rect2(x, y, width, height), fill);
        DrawRect(new Rect2(x, y, width, height), border, false, borderWidth);
    }

    private void Face(Vector2[] points, Color fill, float outlineWidth = 3)
    {
        DrawColoredPolygon(points, fill);
        for (int i = 0; i < points.Length; i++)
            DrawLine(points[i], points[(i + 1) % points.Length], Ink, outlineWidth);
    }
}
