using LivingBunker;

static void Check(bool result, string message)
{
    if (!result) throw new Exception(message);
}

var game = new PrologueOpening();
Check(game.Apply("early-confirm", "confirm").Contains("Сначала"), "Step must not advance before action");
game.Apply("hero", "hero");
game.Apply("confirm-1", "confirm");
game.Apply("stats", "stats");
game.Apply("confirm-2", "confirm");
game.Apply("conflict", "conflict");
Check(!game.ConditionMet, "A schedule conflict must not complete the step");
game.Apply("schedule", "schedule");
game.Tick(2);
game.Apply("confirm-3", "confirm");

game.Apply("food-work", "food");
Check(game.RawFood == 0.8, "Food input must be reserved once");
game.Apply("food-work", "food");
Check(game.RawFood == 0.8, "Repeated command must not reserve again");
game.Tick(30);
game = PrologueOpening.FromJson(game.ToJson());
game.Apply("food-work", "food");
Check(game.RawFood == 0.8, "Saved command history must prevent duplicate reservation after load");
game.Tick(30);
Check(game.Food == 4.2 && game.Work == "", "Save and load must preserve in-progress production");
game.Apply("eat", "eat");
Check(game.Food == 3.2 && game.Hunger == 15, "Eating must spend food and reduce hunger");
game.Apply("confirm-4", "confirm");

game.Apply("water-work", "water");
game.Tick(60);
Check(game.Water == 2.04 && game.RawWater == 0.96, "Water production must follow skill formula");
game.Apply("drink", "drink");
game.Apply("confirm-5", "confirm");

game.Apply("sleep", "sleep");
game.Apply("pause", "pause");
game.Tick(60);
Check(game.WorkSecondsLeft == 60, "Pause must stop work");
game.Apply("resume", "pause");
game.Tick(60);
Check(!game.Bruised && game.Fatigue == 20, "Sleep must heal bruising and fatigue");
game.Apply("confirm-6", "confirm");
Check(game.Step == 7 && game.ConfirmedSteps.Count == 6, "First six steps must complete once");

Check(game.Apply("clear-too-soon", "clear").Contains("Сначала"), "Clearance requires Sveta's dialogue");
game.Apply("sveta", "sveta");
game.Apply("clear", "clear");
game.Tick(30);
game = PrologueOpening.FromJson(game.ToJson());
game.Tick(30);
Check(game.CorridorCleared && game.ConstructionParts == 12, "Clearing opens the corridor without spending parts");
game.Apply("confirm-7", "confirm");

Check(game.Apply("build-too-soon", "build_room").Contains("Нужны"), "Room placement requires preview");
game.Apply("preview", "preview_room");
game.Apply("build", "build_room");
Check(game.ConstructionParts == 6 && game.WorkshopId == "", "Building reserves cost but room is not ready yet");
game.Tick(60);
game = PrologueOpening.FromJson(game.ToJson());
game.Apply("build", "build_room");
Check(game.ConstructionParts == 6, "Repeated build command after load cannot double-spend");
game.Tick(60);
Check(game.WorkshopId == "workshop-01" && game.ConditionMet, "Finished project creates stable room ID");
Check(PrologueOpening.WorkshopWidthCells == 4 && PrologueOpening.WorkshopDoorStartCell == 49 &&
      PrologueOpening.WorkshopCapacity == 8, "Small room follows grid and capacity contract");
game.Apply("confirm-8", "confirm");

game.Apply("install", "install_bench");
game.Tick(60);
Check(game.WorkbenchInstalled && !game.ConditionMet && game.ConstructionParts == 4,
      "Installed bench must still be acknowledged and upgraded");
Check(game.Apply("upgrade-too-soon", "upgrade_bench").Contains("подтвердите"),
      "Upgrade requires installation acknowledgment");
game.Apply("confirm-install", "confirm_install");
game.Apply("upgrade", "upgrade_bench");
Check(game.ConstructionParts == 3, "Workshop, installation and upgrade must cost nine parts total");
game.Tick(15);
game = PrologueOpening.FromJson(game.ToJson());
game.Tick(15);
Check(game.WorkbenchLevel == 1 && game.ConditionMet, "Upgrade survives save in progress");
game.Apply("confirm-9", "confirm");
Check(game.Step == 10 && game.ConfirmedSteps.Count == 9, "All workshop steps must complete once");

game.Apply("sveta-expedition", "sveta_expedition");
game.Apply("confirm-10", "confirm");
Check(game.WeaponEffectivePower == 6 && game.SpareParts == 6, "Worn weapon starts below base power");
game.Apply("weapon-card", "weapon_card");
game.Apply("repair", "repair_weapon");
Check(game.SpareParts == 4 && game.WeaponWearPercent == 40, "Repair reserves parts before it completes");
game.Tick(15);
game = PrologueOpening.FromJson(game.ToJson());
game.Tick(15);
Check(game.WeaponWearPercent == 0 && game.WeaponBasePower == 10 && game.WeaponEffectivePower == 10,
      "Repair removes wear without increasing base power");
Check(game.Apply("upgrade-before-confirm", "upgrade_weapon").Contains("подтвердите"),
      "Upgrade requires repair acknowledgment");
game.Apply("repair-confirm", "confirm_repair");
game.Apply("upgrade-weapon", "upgrade_weapon");
game.Apply("upgrade-weapon", "upgrade_weapon");
Check(game.SpareParts == 1, "Repeated upgrade command cannot spend parts twice");
game.Tick(60);
Check(game.WeaponBasePower == 15 && game.WeaponWearPercent == 0 && game.WeaponEffectivePower == 15,
      "Upgrade changes base power without changing wear");
game.Apply("confirm-11", "confirm");

game.Apply("equipment-card", "equipment_card");
game.Apply("jacket", "equip_jacket");
Check(game.JacketEquipped && !game.JacketInStorage && game.HeroPower == 20,
      "Jacket moves from storage to hero and changes power");
game.Apply("confirm-12", "confirm");

Check(game.Apply("send-without-route", "start_expedition").Contains("просмотренный маршрут"),
      "Route must be viewed before sending the hero");
game.Apply("route", "route_card");
game.Apply("send", "start_expedition");
Check(game.ExpeditionId == "expedition-01" && game.ExpeditionPhase == "outbound", "Expedition starts once");
game.Apply("pause-expedition", "pause");
game.Tick(45);
Check(game.ExpeditionSecondsLeft == 45 && !game.CargoFound, "Pause stops expedition travel");
game.Apply("resume-expedition", "pause");
game.Tick(20);
game = PrologueOpening.FromJson(game.ToJson());
game.Apply("send", "start_expedition");
game.Tick(25);
Check(game.CargoFound && game.HeroCargoParts == 2 && game.HeroCargoRations == 1 &&
      game.HeroCargoWater == 1 && game.HeroCargoMedicine == 1 && game.RaidAlert,
      "Depot cargo belongs to hero and triggers the raid alert once");
game.Apply("confirm-13", "confirm");
Check(game.Step == 14 && game.ExpeditionPhase == "returning" && game.HeroCargoParts == 2,
      "Hero is returning during the raid, retaining cargo");

var spectator = PrologueOpening.FromJson(game.ToJson());
game.Apply("pause-defense", "pause");
game.Tick(5);
Check(game.Battle.Phase == "none", "Pause prevents automatic battle start");
game.Apply("resume-defense", "pause");
game.Tick(1);
Check(game.Battle.Phase == "surface" && game.Battle.DefenderStartCount == 15 &&
      game.Battle.RaiderStartCount == 50 && game.Battle.ActiveIcon?.Kind == "shot",
      "Surface battle starts automatically with 15 versus 50 and a useful icon");
int shotId = game.Battle.ActiveIcon!.Id;
int raiderHpBeforeShot = game.Battle.RaiderTotalHp;
game.Apply("shot", $"react:{shotId}");
Check(game.Battle.RaiderTotalHp == raiderHpBeforeShot - 120, "Shot reaction damages raiders");
game = PrologueOpening.FromJson(game.ToJson());
game.Apply("shot-again", $"react:{shotId}");
Check(game.Battle.RaiderTotalHp == raiderHpBeforeShot - 120, "Used icon cannot fire again after load");
game.Tick(3);
Check(game.Battle.ActiveIcon?.Kind == "skull", "Harmful icon is identified before clicking");
int skullId = game.Battle.ActiveIcon!.Id;
int ownHpBeforeSkull = game.Battle.DefenderTotalHp;
game.Apply("skull", $"react:{skullId}");
Check(game.Battle.DefenderTotalHp == ownHpBeforeSkull - 100, "Harmful icon only applies when clicked");
game.Apply("skull-again", $"react:{skullId}");
Check(game.Battle.DefenderTotalHp == ownHpBeforeSkull - 100, "Harmful icon applies once");

int firstBattleSurvivors = -1;
for (int i = 0; i < 120 && !game.DemoCompleted; i++)
{
    game.Tick(1);
    if (game.Step == 15 && firstBattleSurvivors < 0)
    {
        firstBattleSurvivors = game.Battle.RaidersAlive;
        Check(game.Battle.DefenderStartCount == 20 && game.Battle.RaiderStartCount == firstBattleSurvivors,
              "Corridor gets 20 different defenders and the surviving raiders");
    }
}
Check(firstBattleSurvivors is > 0 and < 50, "Surface defenders inflict real losses");
Check(game.Step == 17 && game.DemoCompleted && game.ConfirmedSteps.Count == 16,
      "Two battles and aftermath finish automatically");
Check(game.ExpeditionPhase == "arrived", "Hero returns only after the blast");
Check(game.BunkerLooted && game.ResidentsCaptured && game.SvetaMissing && game.ReactorDestroyed &&
      game.WorkshopDamaged && game.HeroReturned, "Loot, captivity, mystery, blast and return occur in order");
Check(game.ConstructionParts == 0 && game.SpareParts == 0 && game.Food == 0 && game.Water == 0,
      "Raid destroys common stores");
Check(game.WeaponBasePower == 15 && game.JacketEquipped && game.HeroCargoParts == 2 &&
      game.HeroCargoMedicine == 1, "Hero retains personal gear and expedition cargo");
game = PrologueOpening.FromJson(game.ToJson());
game.Tick(30);
Check(game.HeroCargoParts == 2 && game.ConfirmedSteps.Count == 16,
      "Loading completed demo cannot duplicate loot or story events");

for (int i = 0; i < 120 && !spectator.DemoCompleted; i++) spectator.Tick(1);
Check(spectator.DemoCompleted && spectator.Battle.RaiderStartCount < 50,
      "Both battles and ending finish with no reaction clicks");

var icons = new PrologueBattle();
icons.Start();
var seenKinds = new HashSet<string>();
for (int i = 0; i < 29; i++)
{
    icons.Tick(1);
    if (icons.ActiveIcon is { } shown) seenKinds.Add(shown.Kind);
}
Check(seenKinds.Count == 10, "All five useful and five harmful icon types appear during battle");

var expired = new PrologueBattle();
expired.Start();
expired.Tick(1);
int expiredId = expired.ActiveIcon!.Id;
expired.Tick(2.1);
int hpAfterExpiry = expired.RaiderTotalHp;
Check(expired.React(expiredId).Contains("исчезла") && expired.RaiderTotalHp == hpAfterExpiry,
      "An expired icon has no delayed effect");

var healing = new PrologueBattle
{
    Phase = "surface", DefenderHp = [0, 50], RaiderHp = [100],
    ActiveIcon = new BattleIcon { Id = 99, Kind = "heal", SecondsLeft = 2 }
};
healing.React(99);
Check(healing.DefenderHp[0] == 0 && healing.DefenderHp[1] == 100,
      "Healing cannot revive a defeated fighter or exceed maximum HP");

Console.WriteLine("Complete gray prologue: all checks passed.");
