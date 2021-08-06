using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


[UpdateInGroup(typeof(AbilitySystemGroup))]
[UpdateAfter(typeof(CastBlinkAutoSystem))]
public class BlinkAutoSystem : SystemBase {
    private EntityQuery m_abilityQuery;

    private QuadrantCollisionSystem QuadrantSystem;
    private EndSimulationEntityCommandBufferSystem m_EndSimulationEntityCommandBuffer;

    protected override void OnCreate() {
        m_abilityQuery = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadOnly<BlinkAuto>(),
                ComponentType.ReadOnly<PlayerData>(),
            },
        });
        QuadrantSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<QuadrantCollisionSystem>();
        m_EndSimulationEntityCommandBuffer = World.DefaultGameObjectInjectionWorld
            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        RequireForUpdate(m_abilityQuery);
    }

    // job is obsolete
//    [BurstCompile]
//    [RequireComponentTag(typeof(PlayerData))]
//    private struct ProcessBlinkJob : IJobForEachWithEntity<BlinkAuto, Translation, Heading, CellIndex> {
//        public float dt;
//        public int2 bound;
//        public int dimX;
//
//        [ReadOnly] public DynamicBuffer<GridBuffer> grid;
//
//        [NativeDisableParallelForRestriction] [ReadOnly]
//        public NativeMultiHashMap<int, QuadrantData> quadrant;
//
//        public void Execute(Entity entity, int index, ref BlinkAuto blink, ref Translation position,
//            ref Heading direction, ref CellIndex cellIndex) {
//            blink.duration -= dt;
//
//            var pos2d = new float2(position.Value.x, position.Value.y);
//            float3 dir3d = new float3(direction.VectorDirection.x, direction.VectorDirection.y, 0);
//            var supposedPoint = position.Value + dir3d * 1.1f;
//
//            var supposedTargetQuadrantKey = GridUtils.GetPositionHashMapKey(supposedPoint);
//
//            if (quadrant.TryGetFirstValue(supposedTargetQuadrantKey, out var targetData, out var iter)) {
//                do {
//                    var dirToTarget = new float2(targetData.position.x, targetData.position.y) - pos2d;
//                    var dirToTargetNormalize = math.normalizesafe(dirToTarget);
//                    var dot = math.dot(direction.VectorDirection, dirToTargetNormalize);
//
//                    if (dot > 0.71f) {
//                        var distToTarget = math.length(targetData.position - position.Value);
//                        if (distToTarget > 1.1f) {
//                            break;
//                        }
//
//                        float3 nextCellPos = math.abs(dir3d.x) > 0
//                            ? new float3(targetData.position.x, position.Value.y, 0)
//                            : new float3(position.Value.x, targetData.position.y, 0);
//                        float3 blinkPos = nextCellPos + dir3d * 1.1f;
//                        var coord = GridUtils.WorldToCellCoord(blinkPos, grid[0].WorldPos);
//                        var ind = GridUtils.CoordToIndex(coord, dimX);
//
//                        if (!OutBound(coord) && grid[ind].Type == CellType.Ground) {
//                            position.Value = blinkPos;
////                            commandBuffer.AddComponent(index, entity, new InvulnerabilityBuff() {
////                                timer = 0.3f
////                            });
//                            break;
//                        }
//                    }
//                } while (quadrant.TryGetNextValue(out targetData, ref iter));
//            }
//        }
//
//        private bool OutBound(int2 coord) {
//            return (coord.x > bound.x || coord.y > bound.y) && (coord.x < 0 || coord.y < 0);
//        }
//    }

    // is obsolete
//    [RequireComponentTag(typeof(PlayerData))]
//    private struct CooldownBlinkJob : IJobForEachWithEntity<BlinkAuto> {
//        public EntityCommandBuffer.Concurrent commandBuffer;
//
//        public void Execute(Entity entity, int index, ref BlinkAuto blink) {
//            if (blink.duration < 0) {
//                commandBuffer.RemoveComponent<BlinkAuto>(index, entity);
//            }
//        }
//    }

    private static bool OutBound(int2 coord, int2 bound) {
        return (coord.x > bound.x || coord.y > bound.y) && (coord.x < 0 || coord.y < 0);
    }

    protected override void OnUpdate() {
//        var blinkJob = new ProcessBlinkJob() {
//            dt = Time.DeltaTime,
//            bound = MazeBootstrap.Instance.bound,
//            dimX = MazeBootstrap.Instance.dimX,
//            grid = EntityManager.GetBuffer<GridBuffer>(GetSingletonEntity<GridTag>()),
//            quadrant = QuadrantSystem.QuadrantMultiMap,
//        }.Schedule();

        var dt = Time.DeltaTime;
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        NativeMultiHashMap<int, QuadrantData> quadrant = QuadrantSystem.QuadrantMultiMap;
        var bound = MazeBootstrap.Instance.bound;
        var dimX = MazeBootstrap.Instance.dimX;
        var grid = em.GetBuffer<GridBuffer>(GetSingletonEntity<GridTag>());
        var processBlinkJobHandle = Entities.WithName("ProcessBlinkJob")
            .WithAll<PlayerData>()
            .WithBurst()
            .WithReadOnly(quadrant)
            .WithReadOnly(grid)
            .ForEach((Entity entity, int entityInQueryIndex, ref BlinkAuto blink, ref Translation position,
                ref Heading direction, ref CellIndex cellIndex) => {
                blink.duration -= dt;

                var pos2d = new float2(position.Value.x, position.Value.y);
                float3 dir3d = new float3(direction.VectorDirection.x, direction.VectorDirection.y, 0);
                var supposedPoint = position.Value + dir3d * 1.1f;

                var supposedTargetQuadrantKey = GridUtils.GetPositionHashMapKey(supposedPoint);

                if (quadrant.TryGetFirstValue(supposedTargetQuadrantKey, out var targetData, out var iter)) {
                    do {
                        var dirToTarget = new float2(targetData.position.x, targetData.position.y) - pos2d;
                        var dirToTargetNormalize = math.normalizesafe(dirToTarget);
                        var dot = math.dot(direction.VectorDirection, dirToTargetNormalize);

                        if (dot > 0.71f) {
                            var distToTarget = math.length(targetData.position - position.Value);
                            if (distToTarget > 1.1f) {
                                break;
                            }

                            float3 nextCellPos = math.abs(dir3d.x) > 0
                                ? new float3(targetData.position.x, position.Value.y, 0)
                                : new float3(position.Value.x, targetData.position.y, 0);
                            float3 blinkPos = nextCellPos + dir3d * 1.1f;
                            var coord = GridUtils.WorldToCellCoord(blinkPos, grid[0].WorldPos);
                            var ind = GridUtils.CoordToIndex(coord, dimX);

                            if (!OutBound(coord, bound) && grid[ind].Type == CellType.Ground) {
                                position.Value = blinkPos;
//                            commandBuffer.AddComponent(index, entity, new InvulnerabilityBuff() {
//                                timer = 0.3f
//                            });
                                break;
                            }
                        }
                    } while (quadrant.TryGetNextValue(out targetData, ref iter));
                }
            }).Schedule(Dependency);
        m_EndSimulationEntityCommandBuffer.AddJobHandleForProducer(processBlinkJobHandle);
        Dependency = processBlinkJobHandle;


//        var cooldownBlinkJob = new CooldownBlinkJob() {
//            commandBuffer = m_EndSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent(),
//        }.Schedule(this, blinkJob);
//        cooldownBlinkJob.Complete();
//        m_EndSimulationEntityCommandBuffer.AddJobHandleForProducer(cooldownBlinkJob);

        var ecb = m_EndSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent();
        var cooldownBlinkJobHandle =
            Entities.WithName("CooldownBlinkJob")
                .WithAll<PlayerData>()
                .WithBurst()
                .ForEach((Entity e, int entityInQueryIndex, ref BlinkAuto blink) => {
                    if (blink.duration < 0) {
                        ecb.RemoveComponent<BlinkAuto>(entityInQueryIndex, e);
                    }
                }).Schedule(Dependency);
        m_EndSimulationEntityCommandBuffer.AddJobHandleForProducer(cooldownBlinkJobHandle);
        Dependency = cooldownBlinkJobHandle;
    }
}

[UpdateInGroup(typeof(AbilitySystemGroup))]
public class CastBlinkAutoSystem : ComponentSystem {
    private EntityQuery castBlinkGroup;
    private EntityQuery playerQuery;

    protected override void OnCreate() {
        castBlinkGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadOnly<BlinkAuto>(),
                ComponentType.ReadOnly<AuxiliaryAbility>(),
                ComponentType.ReadOnly<ActivateAbilityTag>(),
            },
            Options = EntityQueryOptions.FilterWriteGroup
        });
        playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerData>());
        RequireForUpdate(castBlinkGroup);
        RequireForUpdate(playerQuery);
    }

    protected override void OnUpdate() {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var blinkEntity = castBlinkGroup.GetSingletonEntity();
        var blink = EntityManager.GetComponentData<BlinkAuto>(blinkEntity);
        var player = playerQuery.GetSingletonEntity();
        em.AddComponentData(player, blink);
        em.RemoveComponent<ActivateAbilityTag>(blinkEntity);
    }
}