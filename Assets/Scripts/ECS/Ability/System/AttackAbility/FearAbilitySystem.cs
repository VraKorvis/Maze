using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


[UpdateInGroup(typeof(AbilitySystemGroup))]
[UpdateAfter(typeof(CastFearSystem))]
public class FearAbilitySystem : SystemBase {
    // private EntityQuery fearDebuffGroup;
    private EntityQuery queryForUpdate;

    // private EndSimulationEntityCommandBufferSystem m_EndSimulationEntityCommandBuffer;

    // private static readonly NativeArray<int2> Directions = new NativeArray<int2>(4, Allocator.Persistent) {
    //     [0] = new int2(0, 1),
    //     [1] = new int2(1, 0),
    //     [2] = new int2(0, -1),
    //     [3] = new int2(-1, 0),
    // };

    protected override void OnCreate() {
        // fearDebuffGroup = GetEntityQuery(new EntityQueryDesc() {
        //     All = new[] {
        //         ComponentType.ReadWrite<FearDebuff>(),
        //         ComponentType.ReadWrite<Heading>(),
        //         ComponentType.ReadOnly<Translation>(),
        //         ComponentType.ReadOnly<Spirit>(),
        //         ComponentType.ReadOnly<MoveSettings>(),
        //         ComponentType.ReadOnly<CellIndex>(),
        //     },
        //     None = new[] {
        //         ComponentType.ReadOnly<SpellImmuneTag>()
        //     },
        //     Options = EntityQueryOptions.FilterWriteGroup
        // });
        // RequireForUpdate(fearDebuffGroup);
        // m_EndSimulationEntityCommandBuffer = World.DefaultGameObjectInjectionWorld
        //     .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        queryForUpdate = GetEntityQuery(new EntityQueryDesc() {
            Any = new[] {
                ComponentType.ReadWrite<FearDebuff>(),
                ComponentType.ReadWrite<AfterFearDebuffPathMoveTag>(),
                ComponentType.ReadWrite<FearDebuffTimeOverTag>(),
            },
        });
        RequireForUpdate(queryForUpdate);

        // directions = new NativeArray<int2>(4, Allocator.Persistent) {
        //     [0] = new int2(0, 1),
        //     [1] = new int2(1, 0),
        //     [2] = new int2(0, -1),
        //     [3] = new int2(-1, 0),
        // };
    }

    protected override void OnDestroy() {
        // GridUtils.Directions.Dispose();
    }

//    protected override JobHandle OnUpdate(JobHandle inputDeps) {
//        var player = GetSingletonEntity<PlayerData>();
//        var playerPos = EntityManager.GetComponentData<Translation>(player);
//
//        var fearDebuffJob = new FearLineDebuffJob() {
//            playerPos = playerPos,
//        }.Schedule(fearDebuffGroup, inputDeps);
//
//        var fearJob = new FearDebuffTimerJob() {
//            dt = Time.DeltaTime,
//            commandBuffer = m_EndSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent(),
//        }.Schedule(this, fearDebuffJob);
//
//        m_EndSimulationEntityCommandBuffer.AddJobHandleForProducer(fearJob);
//
//        var afterFearDebuffPathMoveJob = new AfterFearDebuffPathMovementJob() {
//            commandBuffer = m_EndSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent(),
//        }.Schedule(this, fearJob);
//
//        m_EndSimulationEntityCommandBuffer.AddJobHandleForProducer(afterFearDebuffPathMoveJob);
//
//        return afterFearDebuffPathMoveJob;
//    }

//    job is obsolete
//    [RequireComponentTag(typeof(Spirit), typeof(FearDebuff))]
//    [ExcludeComponent(typeof(SpellImmuneTag))]
//    private struct FearLineDebuffJob : IJobForEach<Heading, Translation, MoveSettings, CellIndex> {
//        [ReadOnly] public Translation playerPos;
//
//        [ReadOnly] public CellIndex playerCell;
//
//        public void Execute(ref Heading heading, [ReadOnly] ref Translation position,
//            [ReadOnly] ref MoveSettings moveSettings, ref CellIndex cell) {
//            var vectorB = playerPos.Value - position.Value;
//
//            if (math.length(vectorB) < 3.0f) {
//                var vectorA = new float3(heading.VectorDirection.x, heading.VectorDirection.y, 0);
//                var dotAB = math.dot(vectorA, vectorB);
//                var newDirection = vectorA;
//                if (moveSettings.targetCellBlocked) {
//                    newDirection = math.mul(quaternion.Euler(0, 0, math.radians(90)), newDirection);
//                    heading.VectorDirection =
//                        new int2((int) math.round(newDirection.x), (int) math.round(newDirection.y));
//                }
//
//                if (dotAB > 0) {
//                    newDirection = math.mul(quaternion.Euler(0, 0, math.radians(90)), newDirection);
//                    heading.VectorDirection =
//                        new int2((int) math.round(newDirection.x), (int) math.round(newDirection.y));
//                }
//            }
//
////            var cellDir = (math.normalizesafe(playerCell.coord - cell.coord)); 
////            float2 dir2d = math.floor(cellDir);
////            
////            float3 cellDirection = new float3(dir2d.x, dir2d.y, 0);
////            
////            if (moveSettings.targetCellBlocked) {
////               var newDirection = math.round(math.mul(quaternion.Euler(0, 0, math.radians(90)), cellDirection));
////                
////                var dotAB = math.dot(newDirection, cellDirection);
////                if (dotAB > 0) {
////                    newDirection = math.round(math.mul(quaternion.Euler(0, 0, math.radians(90)), cellDirection));
////                }
////                heading.VectorDirection = new int2((int)math.floor(newDirection.x), (int)math.floor(newDirection.y));
////            }
//        }
//    }

    [RequireComponentTag(typeof(Spirit), typeof(FearDebuff))]
    [ExcludeComponent(typeof(SpellImmuneTag))]
    [BurstCompile]
    private struct FearLineDebuffJobChunk : IJobChunk {
        [ReadOnly] public Translation playerPos;
        [ReadOnly] public CellIndex playerCell;

        public ArchetypeChunkComponentType<Heading> HeadingArchetype;
        public ArchetypeChunkComponentType<Translation> TranslationArchetype;
        public ArchetypeChunkComponentType<MoveSettings> MoveSettingsArchetype;
        public ArchetypeChunkComponentType<CellIndex> CellIndexArchetype;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var headingChunk = chunk.GetNativeArray(HeadingArchetype);
            var translationChunk = chunk.GetNativeArray(TranslationArchetype);
            var moveSettingsChunk = chunk.GetNativeArray(MoveSettingsArchetype);
            var cellIndexChunk = chunk.GetNativeArray(CellIndexArchetype);

            for (int i = 0; i < chunk.Count; i++) {
                var heading = headingChunk[i];

                var position = translationChunk[i];
                var moveSettings = moveSettingsChunk[i];
                var cellIndex = cellIndexChunk[i];
                var vectorB = playerPos.Value - position.Value;

                if (math.length(vectorB) < 3.0f) {
//                    Debug.Log("EnemySpirit under FearDebuff closer then 3f");
                    var vectorA = new float3(heading.VectorDirection.x, heading.VectorDirection.y, 0);
                    var dotAB = math.dot(vectorA, vectorB);
                    var newDirection = vectorA;
                    if (moveSettings.targetCellBlocked) {
                        newDirection = math.mul(quaternion.Euler(0, 0, math.radians(90)), newDirection);
                        heading.VectorDirection = new int2((int) math.round(newDirection.x),
                            (int) math.round(newDirection.y));
                        headingChunk[i] = heading;
                    }

                    if (dotAB > 0) {
                        newDirection = math.mul(quaternion.Euler(0, 0, math.radians(90)), newDirection);

                        heading.VectorDirection =
                            new int2((int) math.round(newDirection.x), (int) math.round(newDirection.y));
                        headingChunk[i] = heading;
                    }
                }
            }
        }
    }

    protected override void OnUpdate() {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var player = GetSingletonEntity<PlayerData>();
        var playerPos = em.GetComponentData<Translation>(player);
        var playerHeading = em.GetComponentData<Heading>(player);

        //var addCellMovementTagToCircleTypeCharactersJobHandle =
        Entities
            .WithName("AddCellMovementTagToCircleTypeCharactersJob")
            .WithAll<Spirit, FearDebuff>()
            .WithNone<CellMovementTag, IndicesForwardCellTemp>()
            .WithAny<Circle, FlatCurveRose>()
            .WithoutBurst()
            .WithStructuralChanges()
            .ForEach((Entity entity) => {
                em.AddComponent<CellMovementTag>(entity);
                em.AddComponent<IndicesForwardCellTemp>(entity);
                em.AddComponent<PathAgentStatusProcessTag>(entity);
            }).Run();

        var fearLineDebuffJobHandle = Entities.WithName("FearLineDebuffJob")
            .WithAll<Spirit, FearDebuff>()
            .WithNone<SpellImmuneTag>()
            .WithoutBurst()
            .ForEach((Entity entity, ref Heading heading, ref Translation position, ref MoveSettings moveSettings) => {
                var distanceToPlayerPosValue = playerPos.Value - position.Value;

                var enemyHeading = new float3(heading.VectorDirection.x, heading.VectorDirection.y, 0);

                if (math.length(distanceToPlayerPosValue) < 3.0f) {

                    var dotAB = math.dot(enemyHeading, distanceToPlayerPosValue);
                    if (dotAB > 0 || moveSettings.targetCellBlocked) {
                        var newDirection = math.mul(quaternion.Euler(0, 0, math.radians(90)), enemyHeading);
                        heading.VectorDirection =
                            new int2((int) math.round(newDirection.x), (int) math.round(newDirection.y));
                    }

                    // if (moveSettings.targetCellBlocked) {
                    //     newDirection = math.mul(quaternion.Euler(0, 0, math.radians(90)), newDirection);
                    //     heading.VectorDirection = new int2((int) math.round(newDirection.x), (int) math.round(newDirection.y));
                    // }
                }
                else {
                    // Debug.Log("****************BEGIN*****************");
                    // var eh = new float3(1, 0, 0);
                    // var newDirection1 = math.mul(quaternion.Euler(0, 0, math.radians(90)), new float3(1, 0, 0));
                    // Debug.Log("new angle: " + newDirection1);
                    // int2 nv = new int2((int) Math.Round((double) newDirection1.x), (int) Math.Round((double) newDirection1.y));
                    // Debug.Log("new heading: " + nv);
                    // Debug.Log("****************END*****************");
                }
            }).Schedule(Dependency);
        Dependency = fearLineDebuffJobHandle;

        Entities.WithName("FearDebuffTimerJob")
            .WithAll<Spirit>()
            .WithNone<SpellImmuneTag>()
            .WithoutBurst()
            .WithStructuralChanges()
            .ForEach((Entity entity, ref CellIndex cellIndex, ref FearDebuff fear, ref PathAgentStatus status,
                ref PathRequestAgent requestAgent) => {
                fear.timer -= Time.DeltaTime;
                if (fear.timer < 0) {
                    em.RemoveComponent<FearDebuff>(entity);
                    // em.AddComponent<AfterFearDebuffPathMoveTag>(entity);
                    em.AddComponent<FearDebuffTimeOverTag>(entity);
                    em.AddComponent<PathAgentStatusProcessTag>(entity);
                }
            }).Run();

        Entities.WithName("FearDebuffTimeOverJob")
            .WithAll<FearDebuffTimeOverTag>()
            .WithNone<SpellImmuneTag>()
            .WithoutBurst()
            .WithStructuralChanges()
            .ForEach((Entity entity, ref CellIndex cellIndex, ref PathAgentStatus status,
                ref PathRequestAgent requestAgent) => {
                //var reqAgentTemp = requestAgent;
                requestAgent.startCoord = cellIndex.coord;
                //requestAgent = reqAgentTemp;
                // var statusTemp = status;
                status.Value = AgentStatus.Find;
                //status = statusTemp;
                em.AddComponent<AfterFearDebuffPathMoveTag>(entity);
                em.RemoveComponent<FearDebuffTimeOverTag>(entity);
                //em.AddComponent<PathMovementTag>(entity);
            }).Run();

        Entities.WithName("AfterFearDebuffPathMoveJob")
            .WithAll<Spirit, PathRequestAgent, AfterFearDebuffPathMoveTag>()
            .WithoutBurst()
            .WithStructuralChanges()
            .ForEach((Entity entity, ref PathAgentStatus status, ref MoveSettings moveSettings,
                ref Translation position, ref Heading heading) => {
                // if (status.Value == AgentStatus.None) {
                    em.RemoveComponent<AfterFearDebuffPathMoveTag>(entity);
                    em.RemoveComponent<PathAgentStatusProcessTag>(entity);
                    // var dir = math.normalizesafe(moveSettings.targetCellPos - position.Value);
                    // heading.VectorDirection = new int2((int) math.round(dir.x), (int) math.round(dir.y));
                    // }

                    //em.RemoveComponent<AfterFearDebuffPathMoveTag>(entity);
            }).Run();

        //var removeCellMovementTagToCircleTypeCharactersJobHandle = 
        Entities
            .WithName("RemoveCellMovementTagToCircleTypeCharactersJob")
            .WithAll<CellMovementTag, IndicesForwardCellTemp>()
            .WithAny<Circle, FlatCurveRose>()
            .WithNone<FearDebuffTimeOverTag, FearDebuff>()
            .WithoutBurst()
            .WithStructuralChanges()
            .ForEach((Entity entity, ref PathAgentStatus status) => {
                if (status.Value == AgentStatus.None) {
                    em.RemoveComponent<CellMovementTag>(entity);
                    em.RemoveComponent<IndicesForwardCellTemp>(entity);
                    em.RemoveComponent<PathMovementTag>(entity);
                    em.RemoveComponent<PathAgentStatusProcessTag>(entity);
                }
            }).Run();
    }
}

[UpdateInGroup(typeof(AbilitySystemGroup))]
public class CastFearSystem : SystemBase {
    private EntityQuery m_castAbilityGroup;
    private EntityQuery m_playerQuery;
    private EntityQuery m_spiritGroup;

    protected override void OnCreate() {
        m_castAbilityGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadOnly<Fear>(),
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
        var fearEntity = m_castAbilityGroup.GetSingletonEntity();
        var fearAb = em.GetComponentData<Fear>(fearEntity);
        var attackAbility = em.GetComponentData<AttackAbility>(fearEntity);

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
        var translationArchetype = GetArchetypeChunkComponentType<Translation>();
        var filterEnemiesNearestInSightJobChunk = new FilterFoundNearestEnemiesJobChunk() {
            EntitiesArch = entities,
            TranslationArchetype = translationArchetype,
            radiusAbility = attackAbility.radius,
            playerPos = playerPos,
            founds = founds
        }.Schedule(m_spiritGroup);
        filterEnemiesNearestInSightJobChunk.Complete();

        var filterEnemies = AttackFoundEnemies.FilterEnemies(founds, playerPos, playerCell, attackAbility);

        EntityManager.AddComponent(filterEnemies, typeof(FearTag));
        founds.Dispose();
        filterEnemies.Dispose();
        EntityManager.RemoveComponent<ActivateAbilityTag>(fearEntity);

        Entities
            .WithName("CastedFearDebuffJob")
            .WithAll<Spirit, FearTag>()
            .WithNone<SpellImmuneTag, FearDebuff>()
            .WithBurst()
            .WithStructuralChanges()
            .ForEach((Entity e, ref PathAgentStatus status, ref CellIndex cell, ref Translation position,
                ref PathRequestAgent agent) => {
                if (EntityManager.HasComponent<AfterFearDebuffPathMoveTag>(e)) {
                    em.RemoveComponent<AfterFearDebuffPathMoveTag>(e);
                }

                em.AddComponentData(e, new FearDebuff() {
                    timer = fearAb.duration,
                });
                status.Value = AgentStatus.Find;
                agent.destination = cell.coord;
                agent.owner = e;
                agent.focus = Entity.Null;
                em.RemoveComponent<FearTag>(e);
            }).Run();
    }
}