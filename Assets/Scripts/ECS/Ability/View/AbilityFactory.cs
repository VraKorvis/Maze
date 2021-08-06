using System;
using System.IO;
using JetBrains.Annotations;
using Maze.Data.Storages;
using Unity.Entities;
using UnityEngine;

public class AbilityFactory {
    private AbilityFactory() { }

    public static void
        CreateAndAddAbilityToEntity(Entity entity, AbilityType type, ShamanAbilitiesData abil, int index) {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        switch (type) {
            case AbilityType.Fear:
                var fearData = (FearData) abil;
                em.AddComponentData(entity,
                    new Fear() {
                        duration = fearData.duration[index],
                    });
                em.AddComponentData(entity,
                    new AttackAbility() {
                        radius = fearData.radius[index],
                        throughObstacle = fearData.throughObstacle[index] == 1,
                    });
                break;
            case AbilityType.FrostNova:
                var frostData = (FrostNovaData) abil;
                em.AddComponentData(entity,
                    new FrostNova() {
                        duration = frostData.duration[index],
                    });
                em.AddComponentData(entity,
                    new AttackAbility() {
                        radius = frostData.radius[index],
                        throughObstacle = frostData.throughObstacle[index] == 1,
                    });
                break;
            case AbilityType.Slowdown:
                var slowdownData = (SlowdownData) abil;
                em.AddComponentData(entity,
                    new Slowdown() {
                        duration = slowdownData.duration[index],
                        influence = slowdownData.influence[index]
                    });
                em.AddComponentData(entity,
                    new AttackAbility() {
                        radius = slowdownData.radius[index],
                        throughObstacle = slowdownData.throughObstacle[index] == 1,
                    });
                break;
            case AbilityType.Jump:
                var jumpData = (JumpData) abil;
                em.AddComponentData(entity, new Jump());
                em.AddComponentData(entity, new AuxiliaryAbility());
                break;
            case AbilityType.BlinkAuto:
                var blinkData = (BlinkAutoData) abil;
                em.AddComponentData(entity,
                    new BlinkAuto() {
                        duration = blinkData.duration[index],
                    });
                em.AddComponentData(entity, new AuxiliaryAbility());
                break;
            case AbilityType.Invulnerability:
                var invulnerabilityData = (InvulnerabilityData) abil;

                em.AddComponentData(entity,
                    new Invulnerability() {
                        duration = invulnerabilityData.duration[index],
                    });
                em.AddComponentData(entity, new AuxiliaryAbility());
                break;
            case AbilityType.HeroLights:
                var heroLights = (HeroLightsData) abil;
                em.AddComponentData(entity, new HeroLights());
                break;
            case AbilityType.Sonar:
                var sonar = (SonarData) abil;
                em.AddComponentData(entity,
                    new InformingAbility() {
                        type = type,
                        duration = sonar.duration[index],
                        radius = sonar.radius[index],
                    });
                break;
            case AbilityType.ShowRag:
                var showRag = (ShowRagData) abil;
                em.AddComponentData(entity,
                    new InformingAbility() {
                        type = type,
                        duration = showRag.duration[index],
                        radius = showRag.radius[index],
                    });
                break;
            case AbilityType.ShowHome:
                var showHome = (ShowHomeData) abil;

                em.AddComponentData(entity,
                    new InformingAbility() {
                        type = type,
                        duration = showHome.duration[index],
                        radius = showHome.radius[index],
                    });
                break;
            default: return;
        }
    }


    public static ShamanAbilitiesData CreateAbilityDataFromJson(AbilityType type, string json) {
        switch (type) {
            case AbilityType.Fear: return JsonUtility.FromJson<FearData>(json);
            case AbilityType.FrostNova: return JsonUtility.FromJson<FrostNovaData>(json);
            case AbilityType.Slowdown: return JsonUtility.FromJson<SlowdownData>(json);
            case AbilityType.Jump: return JsonUtility.FromJson<JumpData>(json);
            case AbilityType.BlinkAuto: return JsonUtility.FromJson<BlinkAutoData>(json);
            case AbilityType.Invulnerability: return JsonUtility.FromJson<InvulnerabilityData>(json);
            case AbilityType.HeroLights: return JsonUtility.FromJson<HeroLightsData>(json);
            case AbilityType.Sonar: return JsonUtility.FromJson<SonarData>(json);
            case AbilityType.ShowRag: return JsonUtility.FromJson<ShowRagData>(json);
            case AbilityType.ShowHome: return JsonUtility.FromJson<ShowHomeData>(json);
            default: throw new Exception($"Check exist AbilityType {type}.");
        }
    }


    public static string GetPath(AbilityType type) {
        switch (type) {
            case AbilityType.Fear:
                return Path.Combine(Application.streamingAssetsPath, AbilityNameConstant.ABILITY_FEAR);
            case AbilityType.FrostNova:
                return Path.Combine(Application.streamingAssetsPath, AbilityNameConstant.ABILITY_FROSTNOVA);
            case AbilityType.Slowdown:
                return Path.Combine(Application.streamingAssetsPath, AbilityNameConstant.ABILITY_SLOWDOWN);
            case AbilityType.Jump:
                return Path.Combine(Application.streamingAssetsPath, AbilityNameConstant.ABILITY_JUMP);
            case AbilityType.BlinkAuto:
                return Path.Combine(Application.streamingAssetsPath, AbilityNameConstant.ABILITY_BLINK_AUTO);
            case AbilityType.Invulnerability:
                return Path.Combine(Application.streamingAssetsPath, AbilityNameConstant.ABILITY_INVULNERABILITY);
            case AbilityType.HeroLights:
                return Path.Combine(Application.streamingAssetsPath, AbilityNameConstant.ABILITY_HERO_LIGHTS);
            case AbilityType.Sonar:
                return Path.Combine(Application.streamingAssetsPath, AbilityNameConstant.ABILITY_SONAR);
            case AbilityType.ShowRag:
                return Path.Combine(Application.streamingAssetsPath, AbilityNameConstant.ABILITY_SHOW_RAG);
            case AbilityType.ShowHome:
                return Path.Combine(Application.streamingAssetsPath, AbilityNameConstant.ABILITY_SHOW_HOME);
            default: throw new Exception($"Check exist AbilityType {type}.");
        }
    }

    public static void CreateSingleton<T>(T data) where T : struct, IComponentData {
        var query = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(T));
        query.SetSingleton(data);
    }
}