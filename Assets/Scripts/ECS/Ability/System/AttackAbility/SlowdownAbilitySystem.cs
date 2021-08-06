using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(AbilitySystemGroup))]
[UpdateAfter(typeof(CastSlowdownNovaSystem))]
public class SlowdownAbilitySystem : SystemBase {
    private EntityQuery markedGroup;

    private EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    protected override void OnCreate() {
        markedGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadOnly<SlowdownDebuff>(),
                ComponentType.ReadOnly<Spirit>(),
                ComponentType.ReadOnly<MoveSettings>(),
            },
            None = new[] {
                ComponentType.ReadOnly<SpellImmuneTag>(),
            },
            Options = EntityQueryOptions.FilterWriteGroup
        });

        m_EndSimulationEcbSystem = World.DefaultGameObjectInjectionWorld
            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        RequireForUpdate(markedGroup);
    }

    // job is obsolete
//    [RequireComponentTag(typeof(Spirit))]
//    [ExcludeComponent(typeof(SpellImmuneTag))]
//    private struct SlowdownDebuffTimerJob : IJobForEachWithEntity<MoveSettings, SlowdownDebuff> {
//        [ReadOnly] public float dt;
//
//        public EntityCommandBuffer.Concurrent commandBuffer;
//
//        public void Execute(Entity entity, int index, ref MoveSettings moveData, ref SlowdownDebuff frostDebuff) {
//            frostDebuff.timer -= dt;
//            if (frostDebuff.timer < 0) {
//                if (moveData.speed > 0) {
//                    moveData.speed = moveData.defaultSpeed;
//                }
//                commandBuffer.RemoveComponent<SlowdownDebuff>(index, entity);
//            }
//        }
//    }

    protected override void OnUpdate() {
        var dt = Time.DeltaTime;
        var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();
        var slowdownNovaDebuffTimerJobHandle = Entities.WithName("SlowdownNovaDebuffTimerJob")
            .WithAll<Spirit>()
            .WithNone<SpellImmuneTag>()
            .ForEach((Entity e, int entityInQueryIndex, ref MoveSettings moveData, ref SlowdownDebuff frostDebuff) => {
                frostDebuff.timer -= dt;
                if (frostDebuff.timer < 0) {
                    if (moveData.speed > 0) {
                        moveData.speed = moveData.defaultSpeed;
                    }

                    ecb.RemoveComponent<SlowdownDebuff>(entityInQueryIndex, e);
                }
            }).Schedule(Dependency);
        m_EndSimulationEcbSystem.AddJobHandleForProducer(slowdownNovaDebuffTimerJobHandle);
        Dependency = slowdownNovaDebuffTimerJobHandle;
//        var slowdownNovaDebuffTimerJob = new SlowdownDebuffTimerJob() {
//            commandBuffer = m_EndSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent(),
//            dt = Time.DeltaTime,
//        }.Schedule(markedGroup);
//        m_EndSimulationEntityCommandBuffer.AddJobHandleForProducer(slowdownNovaDebuffTimerJob);
    }
}

[UpdateInGroup(typeof(AbilitySystemGroup))]
public class CastSlowdownNovaSystem : SystemBase {
    private EntityQuery m_castAbilityGroup;
    private EntityQuery m_playerQuery;
    private EntityQuery m_spiritGroup;

    private EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    protected override void OnCreate() {
        m_castAbilityGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadOnly<Slowdown>(),
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
        m_EndSimulationEcbSystem = World.DefaultGameObjectInjectionWorld
            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        RequireForUpdate(m_spiritGroup);
        m_playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerData>());
        RequireForUpdate(m_castAbilityGroup);
        RequireForUpdate(m_playerQuery);
    }

    protected override void OnUpdate() {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var player = m_playerQuery.GetSingletonEntity();

        var slowdownEntity = m_castAbilityGroup.GetSingletonEntity();
        var slowdown = EntityManager.GetComponentData<Slowdown>(slowdownEntity);
        var playerPos = em.GetComponentData<Translation>(player);
        var playerCell = em.GetComponentData<CellIndex>(player);

        var attackAbility = em.GetComponentData<AttackAbility>(slowdownEntity);

        var founds = new NativeList<Entity>(Allocator.TempJob);
        // next job is obsolete
//        var filterEnemiesNearestInSightJob = new FilterFoundNearestEnemiesJob() {
//            radiusAbility = attackAbility.radius,
//            playerPos = playerPos,
//            founds = founds
//        }.Schedule(this);
//        filterEnemiesNearestInSightJob.Complete();
        var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();
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

        EntityManager.AddComponent(filterEnemies, typeof(SlowdownTag));
        founds.Dispose();
        filterEnemies.Dispose();
        em.RemoveComponent<ActivateAbilityTag>(slowdownEntity);

        Entities
            .WithName("SlowdownTimerJob")
            .WithAll<Spirit, SlowdownTag>()
            .WithNone<SpellImmuneTag, SlowdownDebuff>()
            .WithStructuralChanges()
            .WithoutBurst()
            .ForEach((Entity e, ref MoveSettings moveData) => {
                em.RemoveComponent<SlowdownTag>(e);
                if (moveData.speed < 0.1f) {
                    return;
                }

                moveData.speed = moveData.speed * (1 - slowdown.influence);
                em.AddComponentData(e, new SlowdownDebuff() {timer = slowdown.duration});
            }).Run();
    }
}