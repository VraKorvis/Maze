using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(AbilitySystemGroup))]
[UpdateAfter(typeof(CastFrostNovaSystem))]
public class FrostNovaAbilitySystem : SystemBase {
    private EntityQuery markedGroup;

    private EndSimulationEntityCommandBufferSystem m_EndSimulationEntityCommandBuffer;

    protected override void OnCreate() {
        markedGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadWrite<FrostNovaDebuff>(),
                ComponentType.ReadOnly<Spirit>(),
                ComponentType.ReadOnly<MoveSettings>(),
            },
            None = new[] {
                ComponentType.ReadOnly<SpellImmuneTag>(),
            },
            Options = EntityQueryOptions.FilterWriteGroup
        });
        m_EndSimulationEntityCommandBuffer = World.DefaultGameObjectInjectionWorld
            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        RequireForUpdate(markedGroup);
    }

    // is obsolete
//    [RequireComponentTag(typeof(Spirit))]
//    [ExcludeComponent(typeof(SpellImmuneTag))]
//    private struct FrostNovaDebuffTimerJob : IJobForEachWithEntity<MoveSettings, FrostNovaDebuff> {
//        public float dt;
//
//        public EntityCommandBuffer.Concurrent commandBuffer;
//
//        public void Execute(Entity entity, int index, ref MoveSettings moveData, ref FrostNovaDebuff frostDebuff) {
//            frostDebuff.timer -= dt;
//            if (frostDebuff.timer < 0) {
//                moveData.speed = moveData.defaultSpeed;
//                commandBuffer.RemoveComponent<FrostNovaDebuff>(index, entity);
//            }
//        }
//    }

    protected override void OnUpdate() {
        //var commandBuffer = m_EndSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent();
        // var ecb = new EntityCommandBuffer(Allocator.TempJob);
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entities.WithName("FrostNovaDebuffTimer")
            .WithAll<Spirit>()
            .WithNone<SpellImmuneTag>()
            .WithoutBurst()
            .WithStructuralChanges()
            .ForEach((Entity entity, ref MoveSettings moveData, ref FrostNovaDebuff frostDebuff) => {
                frostDebuff.timer -= Time.DeltaTime;
                if (frostDebuff.timer < 0) {
                    moveData.speed = moveData.defaultSpeed;
                    em.RemoveComponent<FrostNovaDebuff>(entity);
                }
            }).Run();
        // job is obsolete
//        var frostNovaDebuffTimerJob = new FrostNovaDebuffTimerJob() {
//            dt = Time.DeltaTime,
//            commandBuffer = m_EndSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent(),
//        }.Schedule(markedGroup);
//        m_EndSimulationEntityCommandBuffer.AddJobHandleForProducer(frostNovaDebuffTimerJob);
    }
}

[UpdateInGroup(typeof(AbilitySystemGroup))]
public class CastFrostNovaSystem : SystemBase {
    private EntityQuery m_castAbilityGroup;
    private EntityQuery m_playerQuery;
    private EntityQuery m_spiritGroup;

    protected override void OnCreate() {
        m_castAbilityGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadOnly<FrostNova>(),
                ComponentType.ReadOnly<AttackAbility>(),
                ComponentType.ReadOnly<ActivateAbilityTag>(),
            },
            Options = EntityQueryOptions.FilterWriteGroup
        });
        m_spiritGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadOnly<Spirit>(),
            },
            None = new[] {
                ComponentType.ReadOnly<SpellImmuneTag>(),
                ComponentType.ReadOnly<HauntingTag>(),
            },
            Options = EntityQueryOptions.FilterWriteGroup
        });
        RequireForUpdate(m_spiritGroup);
        m_playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerData>());
        RequireForUpdate(m_castAbilityGroup);
        RequireForUpdate(m_playerQuery);
    }

    protected override void OnUpdate() {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var player = m_playerQuery.GetSingletonEntity();
        var frostEntity = m_castAbilityGroup.GetSingletonEntity();
        var frost = em.GetComponentData<FrostNova>(frostEntity);
        var attackAbility = em.GetComponentData<AttackAbility>(frostEntity);

        var playerPos = em.GetComponentData<Translation>(player);
        var playerCell = em.GetComponentData<CellIndex>(player);

        var founds = new NativeList<Entity>(Allocator.TempJob);
        // next job is obsolete
//        var filterEnemiesNearestInSightJob = new FilterFoundNearestEnemiesJob() {
//            radiusAbility = attackAbility.radius,
//            playerPos = playerPos,
//            founds = founds
//        }.Schedule(this);
//        filterEnemiesNearestInSightJob.Complete();
        var entities = GetArchetypeChunkEntityType();
        var TranslationArchetype = GetArchetypeChunkComponentType<Translation>();
        var filterEnemiesNearestInSightJobChunk = new FilterFoundNearestEnemiesJobChunk() {
            EntitiesArch = entities,
            TranslationArchetype = TranslationArchetype,
            radiusAbility = attackAbility.radius,
            playerPos = playerPos,
            founds = founds
        }.Schedule(m_spiritGroup);
        filterEnemiesNearestInSightJobChunk.Complete();

        var filterEnemies = AttackFoundEnemies.FilterEnemies(founds, playerPos, playerCell, attackAbility);

        EntityManager.AddComponent(filterEnemies, typeof(FrostTag));
        founds.Dispose();
        filterEnemies.Dispose();
        EntityManager.RemoveComponent<ActivateAbilityTag>(frostEntity);

        Entities
            .WithName("FrostDebuffTimerJob")
            .WithAll<Spirit, FrostTag>()
            .WithNone<SpellImmuneTag, FrostNovaDebuff>()
            .WithoutBurst()
            .WithStructuralChanges()
            .ForEach((Entity e, ref MoveSettings moveData) => {
                moveData.speed = 0;
                em.AddComponentData(e, new FrostNovaDebuff() {timer = frost.duration});
                em.RemoveComponent<FrostTag>(e);
            }).Run();
    }
}