using System;
using System.Collections.Generic;
using System.Linq;

namespace LivingBunker;

public sealed class BattleIcon
{
    public int Id { get; set; }
    public string Kind { get; set; } = "";
    public double SecondsLeft { get; set; }
}

/// <summary>Deterministic gray-box defense. Individual HP prevents healing a defeated fighter.</summary>
public sealed class PrologueBattle
{
    private static readonly string[] IconOrder =
    [
        "shot", "skull", "heal", "jam", "shield", "broken_shield",
        "dodge", "enemy_aim", "volley", "panic"
    ];

    public string Phase { get; set; } = "none";
    public List<int> DefenderHp { get; set; } = [];
    public List<int> RaiderHp { get; set; } = [];
    public int DefenderStartCount { get; set; }
    public int RaiderStartCount { get; set; }
    public double PhaseSeconds { get; set; }
    public double NextIconSeconds { get; set; } = 1;
    public int IconSerial { get; set; }
    public BattleIcon? ActiveIcon { get; set; }
    public double DefenderDamageFraction { get; set; }
    public double RaiderDamageFraction { get; set; }
    public double ShieldSeconds { get; set; }
    public double DodgeSeconds { get; set; }
    public double VolleySeconds { get; set; }
    public double JamSeconds { get; set; }
    public double BrokenShieldSeconds { get; set; }
    public double EnemyAimSeconds { get; set; }
    public double PanicSeconds { get; set; }
    public string LastEvent { get; set; } = "";

    public int DefendersAlive => DefenderHp.Count(hp => hp > 0);
    public int RaidersAlive => RaiderHp.Count(hp => hp > 0);
    public int DefenderTotalHp => DefenderHp.Sum();
    public int RaiderTotalHp => RaiderHp.Sum();
    public int DefenderPower => (int)Math.Round(DefendersAlive * 10 * (VolleySeconds > 0 ? 1.5 : 1) *
        (JamSeconds > 0 ? 0.5 : 1) * (PanicSeconds > 0 ? 0.5 : 1));
    public int RaiderPower => RaidersAlive * 10;

    public void Start()
    {
        if (Phase != "none") return;
        Phase = "surface";
        DefenderHp = Enumerable.Repeat(100, 15).ToList();
        RaiderHp = Enumerable.Repeat(100, 50).ToList();
        DefenderStartCount = 15;
        RaiderStartCount = 50;
        LastEvent = "15 защитников удерживают поверхность против 50 рейдеров. Оба отряда стреляют сами.";
    }

    public void Tick(double seconds)
    {
        if (Phase is not ("surface" or "corridor") || seconds <= 0) return;
        PhaseSeconds += seconds;
        ShieldSeconds = Math.Max(0, ShieldSeconds - seconds);
        DodgeSeconds = Math.Max(0, DodgeSeconds - seconds);
        VolleySeconds = Math.Max(0, VolleySeconds - seconds);
        JamSeconds = Math.Max(0, JamSeconds - seconds);
        BrokenShieldSeconds = Math.Max(0, BrokenShieldSeconds - seconds);
        EnemyAimSeconds = Math.Max(0, EnemyAimSeconds - seconds);
        PanicSeconds = Math.Max(0, PanicSeconds - seconds);

        if (ActiveIcon is not null)
        {
            ActiveIcon.SecondsLeft -= seconds;
            if (ActiveIcon.SecondsLeft <= 0) ActiveIcon = null; // Skipping is neutral.
        }
        NextIconSeconds -= seconds;
        if (NextIconSeconds <= 0 && ActiveIcon is null)
        {
            IconSerial++;
            ActiveIcon = new BattleIcon { Id = IconSerial, Kind = IconOrder[(IconSerial - 1) % IconOrder.Length], SecondsLeft = 2 };
            NextIconSeconds = 3;
        }

        double attack = 2.3 * DefendersAlive * (VolleySeconds > 0 ? 1.5 : 1) *
            (JamSeconds > 0 ? 0.5 : 1) * (PanicSeconds > 0 ? 0.5 : 1);
        double incoming = 0.9 * RaidersAlive * (ShieldSeconds > 0 ? 0.5 : 1) *
            (BrokenShieldSeconds > 0 ? 1.5 : 1) * (EnemyAimSeconds > 0 ? 1.5 : 1);
        if (DodgeSeconds > 0) incoming = 0;
        RaiderDamageFraction += attack * seconds;
        DefenderDamageFraction += incoming * seconds;
        int enemyDamage = (int)RaiderDamageFraction;
        int ownDamage = (int)DefenderDamageFraction;
        RaiderDamageFraction -= enemyDamage;
        DefenderDamageFraction -= ownDamage;
        Damage(RaiderHp, enemyDamage);
        Damage(DefenderHp, ownDamage);

        if (PhaseSeconds < 30 && DefendersAlive > 0) return;
        if (Phase == "surface")
        {
            int survivors = RaidersAlive;
            Phase = "corridor";
            PhaseSeconds = 0;
            DefenderHp = Enumerable.Repeat(100, 20).ToList();
            DefenderStartCount = 20;
            RaiderStartCount = survivors;
            ActiveIcon = null;
            ClearEffects();
            LastEvent = $"Наружный рубеж прорван. {survivors} из исходных 50 рейдеров спускаются к 20 другим защитникам.";
        }
        else
        {
            Phase = "aftermath";
            ActiveIcon = null;
            ClearEffects();
            LastEvent = "Коридор прорван. Жители уходят из комнат; рейдеры движутся к складу и реактору.";
        }
    }

    public string React(int iconId)
    {
        if (Phase is not ("surface" or "corridor") || ActiveIcon is null ||
            ActiveIcon.Id != iconId || ActiveIcon.SecondsLeft <= 0)
            return "Иконка уже исчезла или была использована.";
        string kind = ActiveIcon.Kind;
        ActiveIcon = null;
        switch (kind)
        {
            case "shot": Damage(RaiderHp, 120); break;
            case "heal": Heal(DefenderHp, 140); break;
            case "shield": ShieldSeconds = Math.Min(6, Math.Max(ShieldSeconds, 4)); break;
            case "dodge": DodgeSeconds = Math.Min(4, Math.Max(DodgeSeconds, 2)); break;
            case "volley": VolleySeconds = Math.Min(6, Math.Max(VolleySeconds, 4)); break;
            case "skull": Damage(DefenderHp, 100); break;
            case "jam": JamSeconds = Math.Min(6, Math.Max(JamSeconds, 4)); break;
            case "broken_shield": BrokenShieldSeconds = Math.Min(6, Math.Max(BrokenShieldSeconds, 4)); break;
            case "enemy_aim": EnemyAimSeconds = Math.Min(6, Math.Max(EnemyAimSeconds, 4)); break;
            case "panic": PanicSeconds = Math.Min(5, Math.Max(PanicSeconds, 3)); break;
        }
        LastEvent = $"Реакция «{IconLabel(kind)}» применена. Свои: {DefendersAlive}, враги: {RaidersAlive}.";
        return LastEvent;
    }

    public static bool IsHelpful(string kind) => kind is "shot" or "heal" or "shield" or "dodge" or "volley";

    public static string IconLabel(string kind) => kind switch
    {
        "shot" => "Выстрел · дополнительный урон",
        "heal" => "Лечение · восстановить живых",
        "shield" => "Защита · меньше входящего урона",
        "dodge" => "Уворот · краткая неуязвимость",
        "volley" => "Залп · усилить стрельбу",
        "skull" => "Череп · ранит своих",
        "jam" => "Осечка · снижает стрельбу",
        "broken_shield" => "Треснувший щит · снижает защиту",
        "enemy_aim" => "Прицел врага · усиливает врага",
        "panic" => "Паника · ослабляет своих",
        _ => kind
    };

    private void ClearEffects()
    {
        ShieldSeconds = DodgeSeconds = VolleySeconds = JamSeconds = 0;
        BrokenShieldSeconds = EnemyAimSeconds = PanicSeconds = 0;
    }

    private static void Damage(List<int> fighters, int amount)
    {
        for (int i = 0; i < fighters.Count && amount > 0; i++)
        {
            if (fighters[i] <= 0) continue;
            int hit = Math.Min(amount, fighters[i]);
            fighters[i] -= hit;
            amount -= hit;
        }
    }

    private static void Heal(List<int> fighters, int amount)
    {
        for (int i = 0; i < fighters.Count && amount > 0; i++)
        {
            if (fighters[i] <= 0 || fighters[i] >= 100) continue;
            int healed = Math.Min(amount, 100 - fighters[i]);
            fighters[i] += healed;
            amount -= healed;
        }
    }
}
