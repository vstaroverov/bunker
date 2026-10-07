using System;
using System.Collections.Generic;
using System.Text.Json;

namespace LivingBunker;

/// <summary>Tutorial goals through the new workshop. No Godot types: time and commands belong to the simulation.</summary>
public sealed class PrologueOpening
{
    public const int WorkshopStartCell = 48;
    public const int WorkshopWidthCells = 4;
    public const int WorkshopDoorStartCell = 49;
    public const int WorkshopCapacity = 8;
    public const int WorkbenchPlaces = 2; // Prototype value; content balance can change it.
    public int Step { get; set; } = 1;
    public bool ConditionMet { get; set; }
    public bool Paused { get; set; }
    public double GameMinutes { get; set; } = 7 * 60;
    public bool ScheduleSet { get; set; }
    public double ScheduleObservationSeconds { get; set; }
    public string Work { get; set; } = "";
    public double WorkSecondsLeft { get; set; }
    public double Food { get; set; }
    public double RawFood { get; set; } = 5;
    public double Water { get; set; }
    public double RawWater { get; set; } = 3;
    public int Hunger { get; set; } = 65;
    public int Thirst { get; set; } = 55;
    public int Fatigue { get; set; } = 70;
    public bool Bruised { get; set; } = true;
    public int ConstructionParts { get; set; } = 12;
    public bool SpokeToSveta { get; set; }
    public bool CorridorCleared { get; set; }
    public bool WorkshopPreviewed { get; set; }
    public string WorkshopId { get; set; } = "";
    public bool WorkbenchInstalled { get; set; }
    public bool WorkbenchInstallConfirmed { get; set; }
    public int WorkbenchLevel { get; set; }
    public bool SpokeToSvetaBeforeExpedition { get; set; }
    public int SpareParts { get; set; } = 6;
    public string WeaponId { get; set; } = "hero-rifle-01";
    public int WeaponBasePower { get; set; } = 10;
    public int WeaponWearPercent { get; set; } = 40;
    public bool WeaponCardViewed { get; set; }
    public bool WeaponRepairConfirmed { get; set; }
    public bool EquipmentCardViewed { get; set; }
    public bool JacketInStorage { get; set; } = true;
    public bool JacketEquipped { get; set; }
    public bool RouteViewed { get; set; }
    public string ExpeditionId { get; set; } = "";
    public string ExpeditionPhase { get; set; } = "none";
    public double ExpeditionSecondsLeft { get; set; }
    public bool CargoFound { get; set; }
    public int HeroCargoParts { get; set; }
    public int HeroCargoRations { get; set; }
    public int HeroCargoWater { get; set; }
    public int HeroCargoMedicine { get; set; }
    public bool RaidAlert { get; set; }
    public PrologueBattle Battle { get; set; } = new();
    public double AftermathSeconds { get; set; }
    public bool BunkerLooted { get; set; }
    public bool ResidentsCaptured { get; set; }
    public bool SvetaMissing { get; set; }
    public bool ReactorDestroyed { get; set; }
    public bool WorkshopDamaged { get; set; }
    public bool HeroReturned { get; set; }
    public bool DemoCompleted { get; set; }
    public string LastStoryEvent { get; set; } = "";
    public List<int> ConfirmedSteps { get; set; } = [];
    public Dictionary<string, string> CommandResults { get; set; } = [];

    public static PrologueOpening FromJson(string json) =>
        JsonSerializer.Deserialize<PrologueOpening>(json) ?? new PrologueOpening();

    public string ToJson() => JsonSerializer.Serialize(this);

    public string Goal => Step switch
    {
        1 => "У1 · Света встречает героя. Откройте его карточку.",
        2 => "У2 · Изучите характеристики, потребности и травму.",
        3 => "У3 · Задайте суточный график и дождитесь перехода к работе.",
        4 => "У4 · Произведите еду и накормите героя.",
        5 => "У5 · Очистите воду и дайте герою попить.",
        6 => "У6 · Дайте герою поспать и вылечить ушиб.",
        7 => "У7 · Поговорите со Светой и расчистите завал в коридоре.",
        8 => "У8 · Выберите границы и постройте маленькую мастерскую.",
        9 => "У9 · Установите верстак, подтвердите результат и улучшите его.",
        10 => "У10 · Поговорите со Светой о последней экспедиции.",
        11 => "У11 · Отдельно отремонтируйте и улучшите оружие героя.",
        12 => "У12 · Наденьте на героя куртку из кладовки.",
        13 => "У13 · Изучите маршрут и отправьте героя к ремонтному складу.",
        14 => "У14 · Оборона поверхности: 15 защитников против 50 рейдеров. Бой идёт сам.",
        15 => "У15 · Оборона коридора: 20 других защитников против выживших рейдеров.",
        16 => "У16 · Посмотрите грабёж, пленение, взрыв и возвращение героя.",
        _ => "Пролог завершён. Герой вернулся к руинам со своим оружием, курткой и найденным грузом."
    };

    public double WeaponEffectivePower => Math.Round(WeaponBasePower * (100 - WeaponWearPercent) / 100.0, 1);
    public int HeroProtection => JacketEquipped ? 5 : 0;
    public double HeroPower => WeaponEffectivePower + HeroProtection;

    public string Apply(string commandId, string action)
    {
        if (CommandResults.TryGetValue(commandId, out string? previous)) return previous;
        string result = ApplyOnce(action);
        CommandResults[commandId] = result;
        return result;
    }

    private string ApplyOnce(string action)
    {
        if (action == "pause")
        {
            Paused = !Paused;
            return Paused ? "Пауза включена." : "Время снова идёт.";
        }

        if (action == "confirm")
        {
            if (Step >= 14) return "Бой и финал переходят между сценами автоматически.";
            if (!ConditionMet || Step > 13) return "Сначала выполните текущую цель.";
            ConfirmedSteps.Add(Step);
            Step++;
            ConditionMet = false;
            return Step > 13 ? "Экспедиция началась; у бункера объявлена тревога." : "Задача подтверждена. Следующая цель открыта.";
        }

        if (action.StartsWith("react:", StringComparison.Ordinal) && int.TryParse(action.AsSpan(6), out int iconId))
            return Battle.React(iconId);

        if (Work != "") return "Герой занят. Дождитесь завершения работы.";

        switch (Step, action)
        {
            case (1, "hero"):
                ConditionMet = true;
                return "Герой вернулся. Света видит ушиб и голод. Карточка героя открыта.";
            case (2, "stats"):
                ConditionMet = true;
                return "Здоровье: 100%. Навыки: готовка 40, очистка воды 2. Ушиб снижает эффективность; сон вылечит его. Потребности видны справа.";
            case (3, "conflict"):
                return "Конфликт: кухня и очиститель назначены на 09:00–10:00 одновременно. Выберите разные часы.";
            case (3, "schedule"):
                ScheduleSet = true;
                ScheduleObservationSeconds = 2;
                return "График сохранён: кухня 09–10, вода 10–11, сон 22–06. Герой идёт на первую смену.";
            case (4, "food") when ScheduleSet && RawFood >= 4.2:
                Work = "food";
                WorkSecondsLeft = 60;
                RawFood = Math.Round(RawFood - 4.2, 2);
                return "Кухня: 1 игровой час. Зарезервировано 4,20 кг сырья; ожидаемый выпуск 4,20 кг.";
            case (4, "food"):
                return "Для смены на кухне не хватает 4,20 кг сырья или график не задан.";
            case (4, "eat") when Food >= 1:
                Food = Math.Round(Food - 1, 2);
                Hunger = Math.Max(0, Hunger - 50);
                ConditionMet = true;
                return "Герой съел 1 кг еды. Голод уменьшился.";
            case (4, "eat"):
                return "Сначала дождитесь выпуска еды.";
            case (5, "water") when RawWater >= 2.04:
                Work = "water";
                WorkSecondsLeft = 60;
                RawWater = Math.Round(RawWater - 2.04, 2);
                return "Очиститель: 1 игровой час. Зарезервировано 2,04 л сырой воды.";
            case (5, "water"):
                return "Для очистки не хватает сырой воды.";
            case (5, "drink") when Water >= 1:
                Water = Math.Round(Water - 1, 2);
                Thirst = Math.Max(0, Thirst - 50);
                ConditionMet = true;
                return "Герой выпил 1 л воды. Жажда уменьшилась.";
            case (5, "drink"):
                return "Сначала дождитесь очистки воды.";
            case (6, "sleep"):
                Work = "sleep";
                WorkSecondsLeft = 60;
                return "Герой спит в бараке. Через 1 игровой час усталость и ушиб уменьшатся.";
            case (7, "sveta"):
                SpokeToSveta = true;
                return "Света: Коридор завален. Расчисти проход — там хватит места для небольшой мастерской.";
            case (7, "clear") when SpokeToSveta && !CorridorCleared:
                Work = "clear";
                WorkSecondsLeft = 60;
                return "Расчистка началась: 1 игровой час, без расхода деталей. После неё участок станет проходимым.";
            case (7, "clear"):
                return SpokeToSveta ? "Этот участок уже расчищен." : "Сначала поговорите со Светой.";
            case (8, "preview_room") when CorridorCleared:
                WorkshopPreviewed = true;
                return $"Проект: стена x={WorkshopStartCell}–{WorkshopStartCell + WorkshopWidthCells - 1}, дверь x={WorkshopDoorStartCell}–{WorkshopDoorStartCell + 1}, 8 мест. Стоимость 6 деталей, работа 2 часа. Проход остаётся свободным.";
            case (8, "build_room") when CorridorCleared && WorkshopPreviewed && WorkshopId == "" && ConstructionParts >= 6:
                ConstructionParts -= 6;
                Work = "build_room";
                WorkSecondsLeft = 120;
                return "Проект мастерской принят. Зарезервировано 6 деталей; герой строит дверь, стены и готовит помещение.";
            case (8, "build_room"):
                return "Нужны расчищенный проход, просмотренный проект, свободные клетки и 6 деталей.";
            case (9, "install_bench") when WorkshopId != "" && !WorkbenchInstalled && ConstructionParts >= 2:
                ConstructionParts -= 2;
                Work = "install_bench";
                WorkSecondsLeft = 60;
                return "Верстак заказан в карточке мастерской: 2 детали, 2 из 8 мест, 1 игровой час.";
            case (9, "install_bench"):
                return "Для установки нужен готовый зал, свободные места и 2 детали.";
            case (9, "confirm_install") when WorkbenchInstalled:
                WorkbenchInstallConfirmed = true;
                return "Установка подтверждена. Верстак работает; доступно 6 из 8 мест.";
            case (9, "confirm_install"):
                return "Дождитесь окончания установки верстака.";
            case (9, "upgrade_bench") when WorkbenchInstalled && WorkbenchInstallConfirmed && WorkbenchLevel == 0 && ConstructionParts >= 1:
                ConstructionParts--;
                Work = "upgrade_bench";
                WorkSecondsLeft = 30;
                return "Улучшение верстака началось: 1 деталь, ½ игрового часа. Проверочное качество станет 110%.";
            case (9, "upgrade_bench"):
                return "Сначала подтвердите установку; для улучшения нужна 1 деталь.";
            case (10, "sveta_expedition") when WorkshopId != "" && WorkbenchLevel >= 1:
                SpokeToSvetaBeforeExpedition = true;
                ConditionMet = true;
                return "Света: На заброшенном ремонтном складе могут быть детали и лекарства. Сначала приведи оружие в порядок и надень куртку.";
            case (11, "weapon_card"):
                WeaponCardViewed = true;
                return $"Оружие {WeaponId}: базовая мощь {WeaponBasePower}, износ {WeaponWearPercent}%, текущая мощь {WeaponEffectivePower}. Ремонт и улучшение — разные заказы.";
            case (11, "repair_weapon") when WeaponCardViewed && WeaponWearPercent > 0 && SpareParts >= 2 && WorkbenchInstalled:
                SpareParts -= 2;
                Work = "repair_weapon";
                WorkSecondsLeft = 30;
                return "Ремонт оружия: 2 запчасти, ½ игрового часа. Износ уменьшится до 0%, базовая мощь не изменится.";
            case (11, "repair_weapon"):
                return "Откройте карточку оружия, проверьте верстак и запас 2 запчастей.";
            case (11, "confirm_repair") when WeaponWearPercent == 0:
                WeaponRepairConfirmed = true;
                return $"Ремонт подтверждён. Износ 0%; текущая мощь {WeaponEffectivePower}.";
            case (11, "confirm_repair"):
                return "Дождитесь окончания ремонта.";
            case (11, "upgrade_weapon") when WeaponRepairConfirmed && WeaponBasePower == 10 && SpareParts >= 3 && WorkbenchLevel >= 1:
                SpareParts -= 3;
                Work = "upgrade_weapon";
                WorkSecondsLeft = 60;
                return "Улучшение оружия: 3 запчасти, 1 игровой час. Базовая мощь вырастет с 10 до 15 без изменения износа.";
            case (11, "upgrade_weapon"):
                return "Сначала подтвердите ремонт; для улучшения нужны 3 запчасти.";
            case (12, "equipment_card"):
                EquipmentCardViewed = true;
                return "Слоты героя: голова, тело, штаны, ботинки, перчатки, левая и правая рука, двуручное оружие, 2 гаджета. Куртка находится в кладовке.";
            case (12, "equip_jacket") when EquipmentCardViewed && JacketInStorage && !JacketEquipped:
                JacketInStorage = false;
                JacketEquipped = true;
                ConditionMet = true;
                return $"Куртка перенесена из кладовки в слот тела. Защита +5; проверочная мощь героя {HeroPower}.";
            case (12, "equip_jacket"):
                return "Сначала откройте экипировку и проверьте куртку в кладовке.";
            case (13, "route_card"):
                RouteViewed = true;
                return $"Заброшенный ремонтный склад · искать припасы. Путь туда около 45 с; известен риск встречи с рейдерами. Мощь героя {HeroPower}; груз пока пуст.";
            case (13, "start_expedition") when RouteViewed && ExpeditionId == "" && JacketEquipped && WeaponBasePower == 15 && WeaponWearPercent == 0:
                ExpeditionId = "expedition-01";
                ExpeditionPhase = "outbound";
                ExpeditionSecondsLeft = 45;
                return "Герой отправился вправо к ремонтному складу. Его 24-часовой график в бункере временно не исполняется.";
            case (13, "start_expedition"):
                return "Для отправки нужен просмотренный маршрут, исправное улучшенное оружие и надетая куртка.";
            default:
                return "Это действие сейчас недоступно.";
        }
    }

    public void Tick(double realSeconds)
    {
        if (Paused || realSeconds <= 0) return;
        GameMinutes += realSeconds; // 60 real seconds = 60 game minutes.

        if (Step == 3 && ScheduleSet && !ConditionMet)
        {
            ScheduleObservationSeconds -= realSeconds;
            if (ScheduleObservationSeconds <= 0) ConditionMet = true;
        }

        TickExpedition(realSeconds);
        TickBattleAndAftermath(realSeconds);
        if (Work == "") return;
        WorkSecondsLeft -= realSeconds;
        if (WorkSecondsLeft > 0) return;

        switch (Work)
        {
            case "food": Food = Math.Round(Food + 4.2, 2); break;
            case "water": Water = Math.Round(Water + 2.04, 2); break;
            case "sleep":
                Fatigue = Math.Max(0, Fatigue - 50);
                Bruised = false;
                ConditionMet = true;
                break;
            case "clear":
                CorridorCleared = true;
                ConditionMet = true;
                break;
            case "build_room":
                WorkshopId = "workshop-01";
                ConditionMet = true;
                break;
            case "install_bench": WorkbenchInstalled = true; break;
            case "upgrade_bench":
                WorkbenchLevel = 1;
                ConditionMet = true;
                break;
            case "repair_weapon": WeaponWearPercent = 0; break;
            case "upgrade_weapon":
                WeaponBasePower = 15;
                ConditionMet = true;
                break;
        }
        Work = "";
        WorkSecondsLeft = 0;
    }

    private void TickExpedition(double realSeconds)
    {
        if (ExpeditionPhase is not ("outbound" or "returning")) return;
        ExpeditionSecondsLeft -= realSeconds;
        if (ExpeditionSecondsLeft > 0) return;
        if (ExpeditionPhase == "outbound")
        {
            CargoFound = true;
            HeroCargoParts = 2;
            HeroCargoRations = 1;
            HeroCargoWater = 1;
            HeroCargoMedicine = 1;
            ExpeditionPhase = "returning";
            ExpeditionSecondsLeft = 75;
            RaidAlert = true;
            if (Step == 13) ConditionMet = true;
        }
        else
        {
            // The hero waits outside until the defense sequence resolves.
            ExpeditionPhase = "waiting_for_raid";
            ExpeditionSecondsLeft = 0;
        }
    }

    private void TickBattleAndAftermath(double realSeconds)
    {
        if (Step == 14 && RaidAlert && Battle.Phase == "none") Battle.Start();
        if (Battle.Phase is "surface" or "corridor")
        {
            Battle.Tick(realSeconds);
            if (Step == 14 && Battle.Phase == "corridor")
            {
                ConfirmedSteps.Add(14);
                Step = 15;
            }
            if (Step == 15 && Battle.Phase == "aftermath")
            {
                ConfirmedSteps.Add(15);
                Step = 16;
            }
        }
        if (Step != 16 || Battle.Phase != "aftermath") return;
        AftermathSeconds += realSeconds;
        if (AftermathSeconds >= 5 && !BunkerLooted)
        {
            BunkerLooted = true;
            ConstructionParts = SpareParts = 0;
            Food = RawFood = Water = RawWater = 0;
            LastStoryEvent = "Рейдеры выносят склад и имущество мастерской. Общие запасы утрачены.";
        }
        if (AftermathSeconds >= 10 && !ResidentsCaptured)
        {
            ResidentsCaptured = true;
            SvetaMissing = true;
            LastStoryEvent = "Жителей уводят в плен. Света исчезает в суматохе; её судьба неизвестна.";
        }
        if (AftermathSeconds >= 15 && !ReactorDestroyed)
        {
            ReactorDestroyed = true;
            WorkshopDamaged = true;
            LastStoryEvent = "Рейдеры перегружают реактор. Вход, лифт и часть коридора разрушены.";
        }
        if (AftermathSeconds >= 20 && ExpeditionPhase == "waiting_for_raid" && !HeroReturned)
        {
            HeroReturned = true;
            ExpeditionPhase = "arrived";
            DemoCompleted = true;
            ConfirmedSteps.Add(16);
            Step = 17;
            LastStoryEvent = "Герой возвращается к руинам. Улучшенное оружие, куртка и найденные припасы остались при нём.";
        }
    }
}
