using Unity.Entities;

public enum AbilityType {
    //DefaultGameObjectInjectionWorld 0 < x < 100
    Fear = 1,
    FrostNova = 2,
    Slowdown = 3,

    //Auxiliary 100 < x < 1000
    Jump = 101,
    BlinkAuto = 103,
    Invulnerability = 104,

    //Informing >1000
    HeroLights = 1001,
    Sonar = 1002,
    ShowRag = 1003,
    ShowHome = 1004,
}

public struct CooldownAbilityButton : IComponentData {
    public float timer;
}

public struct ActivateAbilityTag : IComponentData {
}

public struct AuxiliaryAbility : IComponentData {
}

public struct InformingAbility : IComponentData {
    public AbilityType type;
    public float radius;
    public float duration;
}

//AttackAbility
public struct AttackAbility : IComponentData {
    public float radius;
    public bool throughObstacle;
}

public struct Fear : IComponentData {
    public float duration;
}

public struct FearTag : IComponentData {
}

public struct FearDebuff : IComponentData {
    public float timer;
}

public struct FearDebuffTimeOverTag : IComponentData {
}


public struct AfterFearDebuffPathMoveTag : IComponentData {
}


public struct FrostNova : IComponentData {
    public float duration;
}

public struct FrostTag : IComponentData {
}

public struct FrostNovaDebuff : IComponentData {
    public float timer;
}

public struct Slowdown : IComponentData {
    public float duration;
    public float influence;
}

public struct SlowdownTag : IComponentData {
}

public struct SlowdownDebuff : IComponentData {
    public float timer;
}

//Auxiliary Ability
public struct Jump : IComponentData {
    public float duration;
}

public struct BlinkAuto : IComponentData {
    public float duration;
}

public struct Invulnerability : IComponentData {
    public float duration;
}

public struct HeroLights : IComponentData {
}

//InformingAbility

public struct EmptyAbility : IComponentData {
}

//Enemy debaff
public struct Silence : IComponentData {
    public float radius;
    public float duration;
    public int point;
    public float cooldown;
}

public struct ReducedVisibility : IComponentData {
    public float distance;
    public float duration;
    public int point;
    public float cooldown;
}