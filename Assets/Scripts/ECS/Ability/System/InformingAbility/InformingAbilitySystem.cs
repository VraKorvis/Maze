using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(AbilitySystemGroup))]
public class InformingAbilitySystem : SystemBase {
    private EntityQuery informingGroup;

    // private EntityQuery m_markedSpiritGroup;
    //private GameObject cacheGoIndicator;
    //private Entity cacheEntityIndicator;

    EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    protected override void OnCreate() {
        informingGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadOnly<InformingAbility>(),
                ComponentType.ReadOnly<ActivateAbilityTag>(),
            },
            Options = EntityQueryOptions.FilterWriteGroup,
        });
//        m_markedSpiritGroup = GetEntityQuery(new EntityQueryDesc() {
//            All = new[] {
//                ComponentType.ReadOnly<Spirit>(),
//                ComponentType.ReadOnly<MarkedEntityToAddIndicatorTag>(),
//            },
////            Any = new[] {
////                ComponentType.ReadOnly<Spirit>(),
////                ComponentType.ReadOnly<HomeTag>(),
////                ComponentType.ReadOnly<RagTag>(),
////                
////            },
//            Options = EntityQueryOptions.FilterWriteGroup,
//        });
        m_EndSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate() {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (informingGroup.CalculateEntityCount() != 1) return;
        var informingEntity = informingGroup.GetSingletonEntity();
        var informingAbility = EntityManager.GetComponentData<InformingAbility>(informingEntity);
        // var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer();
        string color = String.Empty;

        // JobHandle addIndicatorsJobHandle = new JobHandle();
        switch (informingAbility.type) {
            case AbilityType.Sonar:
                // addIndicatorsJobHandle = 
                Entities.WithName("AddSonarIndicatorToShowEnemyJob")
                    .WithoutBurst()
                    .WithStructuralChanges()
                    .WithAll<Spirit>()
                    .ForEach((Entity e) => {
//                        Debug.Log("Sonar case");
                        em.AddComponent<MarkedEntityToAddIndicatorTag>(e);
                    }).Run();
                color = StringConstant.MATERIAL_RED;
                break;
            case AbilityType.ShowHome:
                // addIndicatorsJobHandle = 
                Entities.WithName("AddIndicatorToShowHomeJob")
                    .WithoutBurst()
                    .WithStructuralChanges()
                    .WithAll<HomeTag>()
                    .ForEach((Entity e) => { em.AddComponent<MarkedEntityToAddIndicatorTag>(e); }).Run();
                color = StringConstant.MATERIAL_BLUE;
                break;
            case AbilityType.ShowRag:
                //addIndicatorsJobHandle = 
                Entities.WithName("AddIndicatorToShowRagJob")
                    .WithoutBurst()
                    .WithStructuralChanges()
                    .WithAll<RagTag>()
                    .ForEach((Entity e) => { em.AddComponent<MarkedEntityToAddIndicatorTag>(e); }).Run();
                color = StringConstant.MATERIAL_BLUE;
                break;
            case AbilityType.HeroLights:
                break;
        }

        em.RemoveComponent<ActivateAbilityTag>(informingEntity);
        CreateIndicatorIcon(StringConstant.INDICATOR_POINTER_ICON, color, informingAbility);
    }

    private void CreateIndicatorIcon(string path, string materialName, InformingAbility informingAbility) {
//        if (cacheGoIndicator == null) {
//            cacheGoIndicator = Resources.Load(path) as GameObject;
////            Debug.Log("cacheIndicator==null: " + cacheGoIndicator);
//        }

        GameObject cacheGoIndicator = Resources.Load(path) as GameObject;
        var mat = Resources.Load(materialName) as Material;
        cacheGoIndicator.GetComponent<MeshRenderer>().material = mat;

//        if (cacheEntityIndicator != Entity.Null) {
////            Debug.Log("cacheEntityIndicator!=Entity.Null");
//            em.DestroyEntity(cacheEntityIndicator);
//        }

        var blobAssetStore = new BlobAssetStore();
        var settings =
            GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, blobAssetStore);
        Entity cacheEntityIndicator =
            GameObjectConversionUtility.ConvertGameObjectHierarchy(cacheGoIndicator, settings);
        var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();
        var addIndicatorPointerJobHandle =
            Entities.WithName("AddIndicatorPointerJob")
                .WithAll<MarkedEntityToAddIndicatorTag>()
                .WithAny<Spirit, HomeTag, RagTag>()
//                .WithoutBurst()
//                .WithStructuralChanges()
                .ForEach((Entity e, int entityInQueryIndex) => {
                    ecb.RemoveComponent<MarkedEntityToAddIndicatorTag>(entityInQueryIndex, e);
                    var icon = ecb.Instantiate(entityInQueryIndex, cacheEntityIndicator);
                    ecb.AddComponent(entityInQueryIndex, icon, new IndicatorPointer() {
                        targetEntity = e,
                        radius = informingAbility.radius,
                        duration = informingAbility.duration
                    });
                    ecb.AddComponent(entityInQueryIndex, icon, new Scale());
                }).ScheduleParallel(Dependency);
        m_EndSimulationEcbSystem.AddJobHandleForProducer(addIndicatorPointerJobHandle);
        Dependency = addIndicatorPointerJobHandle;
        blobAssetStore.Dispose();
    }
}