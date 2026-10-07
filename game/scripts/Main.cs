using Godot;
using System;
using System.IO;

namespace LivingBunker;

public partial class Main : Node2D
{
    private Camera2D _camera = null!;
    private FirstScreenWorld _world = null!;
    private Label _info = null!;
    private Label _tutorialGoal = null!;
    private Label _tutorialState = null!;
    private Label _tutorialMessage = null!;
    private VBoxContainer _tutorialActions = null!;
    private Button _confirmButton = null!;
    private PanelContainer _combatPanel = null!;
    private Label _combatTitle = null!;
    private Label _ownForces = null!;
    private Label _enemyForces = null!;
    private ProgressBar _ownHealth = null!;
    private ProgressBar _enemyHealth = null!;
    private Button _reactionButton = null!;
    private PrologueOpening _opening = new();
    private double _uiRefresh;
    private double _autoSave;
    private int _renderedStep = -1;
    private bool _cargoAnnounced;
    private string _observedBattleEvent = "";
    private string _observedStoryEvent = "";
    private string _renderedBattlePhase = "";
    private string _lastMessage = "Света: Ты вернулся. Вижу, ты ранен и голоден. Приведи себя в порядок.";
    private static string SavePath => ProjectSettings.GlobalizePath("user://prologue-opening.json");

    private readonly record struct Site(Rect2 Area, string Description);

    private static readonly Site[] Sites =
    [
        new(new Rect2(1260, 360, 115, 260), "1 · Входной флаг — ориентир внешнего мира"),
        new(new Rect2(1240, 555, 80, 70), "2 · Противотанковый ёж — оборона у КПП"),
        new(new Rect2(1130, 475, 115, 145), "3 · КПП — отправка в пустошь и внешние события"),
        new(new Rect2(600, 410, 400, 215), "4 · Руины дома — после ремонта место для средней комнаты"),
        new(new Rect2(395, 460, 155, 165), "5 · Вход в бункер — начало пролога и взлом"),
        new(new Rect2(35, 445, 355, 180), "6 · Входная группа — первый оборонительный узел"),
        new(new Rect2(235, 455, 135, 450), "7 · Лифт — шахта требует ремонта"),
        new(new Rect2(55, 455, 150, 450), "8 · Лестница — путь на первый подземный этаж"),
        new(new Rect2(400, 715, 620, 195), "9 · Коридор — дверь маленькой комнаты и завал справа"),
    ];

    public override void _Ready()
    {
        _camera = GetNode<Camera2D>("Camera2D");
        _world = GetNode<FirstScreenWorld>("World");
        _info = GetNode<Label>("HUD/BottomBar/Info");
        LoadOpening();
        BuildTutorialPanel();
        BuildCombatPanel();
        RefreshTutorial();
        GD.Print("Living Bunker: first screen ready.");
    }

    public override void _Process(double delta)
    {
        _opening.Tick(delta);
        _uiRefresh += delta;
        _autoSave += delta;
        if (_uiRefresh >= 0.2)
        {
            _uiRefresh = 0;
            RefreshTutorial();
        }
        if (_autoSave >= 5)
        {
            _autoSave = 0;
            SaveOpening();
        }

        Vector2 direction = Vector2.Zero;
        if (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left)) direction.X -= 1;
        if (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right)) direction.X += 1;
        if (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up)) direction.Y -= 1;
        if (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down)) direction.Y += 1;

        if (direction != Vector2.Zero)
        {
            _camera.Position += direction.Normalized() * 650f * (float)delta / _camera.Zoom.X;
            _camera.Position = new Vector2(
                Mathf.Clamp(_camera.Position.X, 200, 1800),
                Mathf.Clamp(_camera.Position.Y, 300, 950));
        }
    }

    public override void _ExitTree() => SaveOpening();

    private void BuildTutorialPanel()
    {
        var panel = new PanelContainer
        {
            AnchorLeft = 1, AnchorRight = 1, AnchorTop = 0, AnchorBottom = 1,
            OffsetLeft = -370, OffsetRight = -12, OffsetTop = 76, OffsetBottom = -94
        };
        GetNode<CanvasLayer>("HUD").AddChild(panel);

        var padding = new MarginContainer();
        padding.AddThemeConstantOverride("margin_left", 16);
        padding.AddThemeConstantOverride("margin_right", 16);
        padding.AddThemeConstantOverride("margin_top", 14);
        padding.AddThemeConstantOverride("margin_bottom", 14);
        panel.AddChild(padding);

        var scroll = new ScrollContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        padding.AddChild(scroll);
        var column = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        column.AddThemeConstantOverride("separation", 10);
        scroll.AddChild(column);

        column.AddChild(new Label { Text = "ПРОЛОГ · ДЕЙСТВУЮЩИЙ БУНКЕР" });
        _tutorialGoal = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        column.AddChild(_tutorialGoal);
        _tutorialState = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        column.AddChild(_tutorialState);
        column.AddChild(new HSeparator());

        _tutorialActions = new VBoxContainer();
        _tutorialActions.AddThemeConstantOverride("separation", 6);
        column.AddChild(_tutorialActions);

        _confirmButton = new Button { Text = "Подтвердить выполненную задачу" };
        _confirmButton.Pressed += () => RunAction("confirm");
        column.AddChild(_confirmButton);

        _tutorialMessage = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        column.AddChild(_tutorialMessage);

        var save = new Button { Text = "Сохранить" };
        save.Pressed += () => { SaveOpening(); _lastMessage = "Прогресс сохранён."; RefreshTutorial(); };
        column.AddChild(save);
        var load = new Button { Text = "Загрузить" };
        load.Pressed += () => { LoadOpening(); _renderedStep = -1; RefreshTutorial(); };
        column.AddChild(load);
        var pause = new Button { Text = "Пауза / продолжить" };
        pause.Pressed += () => RunAction("pause");
        column.AddChild(pause);
    }

    private void BuildCombatPanel()
    {
        _combatPanel = new PanelContainer { OffsetLeft = 78, OffsetTop = 162, OffsetRight = 868, OffsetBottom = 520, Visible = false };
        GetNode<CanvasLayer>("HUD").AddChild(_combatPanel);
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 18);
        margin.AddThemeConstantOverride("margin_right", 18);
        margin.AddThemeConstantOverride("margin_top", 14);
        margin.AddThemeConstantOverride("margin_bottom", 14);
        _combatPanel.AddChild(margin);
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 12);
        margin.AddChild(column);
        _combatTitle = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        column.AddChild(_combatTitle);
        var sides = new HBoxContainer();
        sides.AddThemeConstantOverride("separation", 24);
        column.AddChild(sides);
        var own = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        sides.AddChild(own);
        _ownForces = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        own.AddChild(_ownForces);
        _ownHealth = new ProgressBar { CustomMinimumSize = new Vector2(0, 26), ShowPercentage = false };
        own.AddChild(_ownHealth);
        sides.AddChild(new Label { Text = "⚔", HorizontalAlignment = HorizontalAlignment.Center });
        var enemy = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        sides.AddChild(enemy);
        _enemyForces = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        enemy.AddChild(_enemyForces);
        _enemyHealth = new ProgressBar { CustomMinimumSize = new Vector2(0, 26), ShowPercentage = false };
        enemy.AddChild(_enemyHealth);
        column.AddChild(new Label { Text = "Отряды перестреливаются автоматически. Можно нажать реакцию или просто наблюдать." , AutowrapMode = TextServer.AutowrapMode.WordSmart });
        _reactionButton = new Button { CustomMinimumSize = new Vector2(0, 58) };
        _reactionButton.Pressed += () =>
        {
            if (_opening.Battle.ActiveIcon is { } icon) RunAction($"react:{icon.Id}");
        };
        column.AddChild(_reactionButton);
        column.AddChild(new Label { Text = "Полезные иконки помогают. Опасные срабатывают только при нажатии; пропуск нейтрален.", AutowrapMode = TextServer.AutowrapMode.WordSmart });
    }

    private void RefreshTutorial()
    {
        if (_opening.CargoFound && !_cargoAnnounced)
        {
            _cargoAnnounced = true;
            _lastMessage = "Герой нашёл 2 детали, паёк, воду и препарат. Груз у него, а не в общем складе. Тревога: к бункеру идут рейдеры!";
        }
        if (_opening.Battle.LastEvent != "" && _opening.Battle.LastEvent != _observedBattleEvent)
        {
            _observedBattleEvent = _opening.Battle.LastEvent;
            _lastMessage = _observedBattleEvent;
        }
        if (_opening.LastStoryEvent != "" && _opening.LastStoryEvent != _observedStoryEvent)
        {
            _observedStoryEvent = _opening.LastStoryEvent;
            _lastMessage = _observedStoryEvent;
        }
        _tutorialGoal.Text = _opening.Goal;
        _confirmButton.Visible = _opening.Step <= 13;
        int hour = (int)(_opening.GameMinutes / 60) % 24;
        int minute = (int)_opening.GameMinutes % 60;
        string work = _opening.Work == "" ? "нет" : $"{_opening.Work}, ещё {Math.Ceiling(_opening.WorkSecondsLeft)} с";
        _tutorialState.Text =
            $"День 1 · {hour:00}:{minute:00}{(_opening.Paused ? " · ПАУЗА" : "")}\n" +
            $"Еда {_opening.Food:0.00} кг (сырьё {_opening.RawFood:0.00}) · вода {_opening.Water:0.00} л (сырая {_opening.RawWater:0.00})\n" +
            $"Голод {_opening.Hunger}% · жажда {_opening.Thirst}% · усталость {_opening.Fatigue}% · ушиб {(_opening.Bruised ? "есть" : "вылечен")}\n" +
            $"Работа: {work} · условие: {(_opening.ConditionMet ? "выполнено" : "ожидается")}" +
            (_opening.Step < 7 ? "" : $"\nДетали: {_opening.ConstructionParts} · мастерская: {(_opening.WorkshopId == "" ? "не построена" : "8 мест")} · верстак: {(_opening.WorkbenchInstalled ? $"ур. {_opening.WorkbenchLevel}, качество {100 + _opening.WorkbenchLevel * 10}%" : "нет")}") +
            (_opening.Step < 10 ? "" : $"\nЗапчасти: {_opening.SpareParts} · оружие: {_opening.WeaponBasePower} база / {_opening.WeaponEffectivePower} сейчас, износ {_opening.WeaponWearPercent}% · защита {_opening.HeroProtection} · мощь {_opening.HeroPower} · куртка {(_opening.JacketEquipped ? "на герое" : "на складе")}") +
            (_opening.Step < 13 ? "" : $"\nПоход: {_opening.ExpeditionPhase}{(_opening.ExpeditionPhase == "outbound" || _opening.ExpeditionPhase == "returning" ? $", ещё {Math.Ceiling(_opening.ExpeditionSecondsLeft)} с" : "")} · груз героя: {_opening.HeroCargoParts} детали, {_opening.HeroCargoRations} паёк, {_opening.HeroCargoWater} л воды, {_opening.HeroCargoMedicine} препарат");
        _tutorialMessage.Text = _lastMessage;
        _world.SetConstructionState(_opening.CorridorCleared, _opening.WorkshopPreviewed,
            _opening.WorkshopId != "", _opening.WorkbenchInstalled, _opening.WorkbenchLevel);
        _world.SetExpeditionState(_opening.ExpeditionPhase, _opening.ExpeditionSecondsLeft);
        _world.SetAftermathState(_opening.BunkerLooted, _opening.ResidentsCaptured,
            _opening.ReactorDestroyed, _opening.HeroReturned);
        RefreshCombatPanel();

        if (_renderedStep == _opening.Step) return;
        _renderedStep = _opening.Step;
        foreach (Node child in _tutorialActions.GetChildren()) child.QueueFree();
        switch (_opening.Step)
        {
            case 1: AddAction("Открыть карточку героя", "hero"); break;
            case 2: AddAction("Посмотреть характеристики", "stats"); break;
            case 3:
                AddAction("Проверить пересечение смен", "conflict");
                AddAction("Сохранить график на 24 часа", "schedule");
                break;
            case 4:
                AddAction("Назначить на кухню · 1 час", "food");
                AddAction("Поесть в столовой · 1 кг", "eat");
                break;
            case 5:
                AddAction("Назначить на очиститель · 1 час", "water");
                AddAction("Попить · 1 л", "drink");
                break;
            case 6: AddAction("Отправить спать · 1 час", "sleep"); break;
            case 7:
                AddAction("Поговорить со Светой", "sveta");
                AddAction("Расчистить завал · 1 час", "clear");
                break;
            case 8:
                AddAction("Посмотреть проект двери и границы", "preview_room");
                AddAction("Построить мастерскую · 6 деталей · 2 часа", "build_room");
                break;
            case 9:
                AddAction("Установить верстак · 2 детали · 1 час", "install_bench");
                AddAction("Подтвердить установку верстака", "confirm_install");
                AddAction("Улучшить верстак · 1 деталь · ½ часа", "upgrade_bench");
                break;
            case 10: AddAction("Поговорить со Светой", "sveta_expedition"); break;
            case 11:
                AddAction("Осмотреть оружие", "weapon_card");
                AddAction("Ремонт · 2 запчасти · ½ часа", "repair_weapon");
                AddAction("Подтвердить ремонт", "confirm_repair");
                AddAction("Улучшить · 3 запчасти · 1 час", "upgrade_weapon");
                break;
            case 12:
                AddAction("Открыть экипировку героя", "equipment_card");
                AddAction("Надеть куртку из кладовки", "equip_jacket");
                break;
            case 13:
                AddAction("Посмотреть маршрут ремонтного склада", "route_card");
                AddAction("Отправить героя в экспедицию", "start_expedition");
                break;
        }
    }

    private void RefreshCombatPanel()
    {
        PrologueBattle battle = _opening.Battle;
        bool fighting = battle.Phase is "surface" or "corridor";
        _combatPanel.Visible = fighting;
        if (battle.Phase != _renderedBattlePhase)
        {
            _renderedBattlePhase = battle.Phase;
            if (battle.Phase == "surface")
            {
                _camera.Position = new Vector2(800, 555);
                _camera.Zoom = new Vector2(1.15f, 1.15f);
            }
            else if (battle.Phase == "corridor")
            {
                _camera.Position = new Vector2(760, 800);
                _camera.Zoom = new Vector2(1.15f, 1.15f);
            }
            else if (battle.Phase == "aftermath")
            {
                _camera.Position = new Vector2(780, 610);
                _camera.Zoom = new Vector2(0.8f, 0.8f);
            }
        }
        if (!fighting) return;
        _combatTitle.Text = battle.Phase == "surface" ? "БОЙ 1 · ПОВЕРХНОСТЬ" : "БОЙ 2 · ПОДЗЕМНЫЙ КОРИДОР";
        _ownForces.Text = $"ЗАЩИТНИКИ СЛЕВА\n{battle.DefendersAlive}/{battle.DefenderStartCount} бойцов · мощь {battle.DefenderPower}\nЗдоровье {battle.DefenderTotalHp}/{battle.DefenderStartCount * 100}";
        _enemyForces.Text = $"РЕЙДЕРЫ СПРАВА\n{battle.RaidersAlive}/{battle.RaiderStartCount} бойцов · мощь {battle.RaiderPower}\nЗдоровье {battle.RaiderTotalHp}/{battle.RaiderStartCount * 100}";
        _ownHealth.MaxValue = battle.DefenderStartCount * 100;
        _ownHealth.Value = battle.DefenderTotalHp;
        _enemyHealth.MaxValue = battle.RaiderStartCount * 100;
        _enemyHealth.Value = battle.RaiderTotalHp;
        if (battle.ActiveIcon is { } icon)
        {
            bool helpful = PrologueBattle.IsHelpful(icon.Kind);
            _reactionButton.Disabled = false;
            _reactionButton.Text = $"{(helpful ? "+ ПОЛЕЗНО" : "− ОПАСНО")} · {PrologueBattle.IconLabel(icon.Kind)} · {icon.SecondsLeft:0.0} с";
            _reactionButton.Modulate = helpful ? new Color("#91dbce") : new Color("#f0a28d");
        }
        else
        {
            _reactionButton.Disabled = true;
            _reactionButton.Text = "Реакция скоро появится";
            _reactionButton.Modulate = Colors.White;
        }
    }

    private void AddAction(string label, string action)
    {
        var button = new Button { Text = label };
        button.Pressed += () => RunAction(action);
        _tutorialActions.AddChild(button);
    }

    private void RunAction(string action)
    {
        _lastMessage = _opening.Apply(Guid.NewGuid().ToString("N"), action);
        SaveOpening();
        RefreshTutorial();
    }

    private void SaveOpening()
    {
        try { File.WriteAllText(SavePath, _opening.ToJson()); }
        catch (Exception error) { GD.PushError($"Could not save prologue: {error.Message}"); }
    }

    private void LoadOpening()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                _opening = PrologueOpening.FromJson(File.ReadAllText(SavePath));
                _cargoAnnounced = false;
                _observedBattleEvent = _observedStoryEvent = "";
                _renderedBattlePhase = "";
                _lastMessage = "Прогресс загружен.";
            }
        }
        catch (Exception error) { GD.PushError($"Could not load prologue: {error.Message}"); }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo)
        {
            if (key.Keycode == Key.G) _world.ToggleGrid();
            if (key.Keycode == Key.Home)
            {
                _camera.Position = new Vector2(780, 610);
                _camera.Zoom = new Vector2(0.8f, 0.8f);
            }
        }

        if (@event is not InputEventMouseButton mouse || !mouse.Pressed) return;
        if (mouse.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
        {
            float factor = mouse.ButtonIndex == MouseButton.WheelUp ? 1.12f : 1f / 1.12f;
            float value = Mathf.Clamp(_camera.Zoom.X * factor, 0.55f, 1.4f);
            _camera.Zoom = new Vector2(value, value);
            return;
        }

        if (mouse.ButtonIndex != MouseButton.Left) return;
        Vector2 point = GetGlobalMousePosition();
        foreach (Site site in Sites)
        {
            if (!site.Area.HasPoint(point)) continue;
            _info.Text = site.Description;
            return;
        }
        _info.Text = "Поверхность и первый подземный этаж · выбери объект";
    }
}
