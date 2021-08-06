using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(AbilitySystemGroup))]
[UpdateAfter(typeof(CastJumpSystem))]
public class JumpSystem : SystemBase {
    private EntityQuery abilQuery;

    private EndSimulationEntityCommandBufferSystem m_EndSimulationEntityCommandBuffer;

    protected override void OnCreate() {
        abilQuery = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadOnly<Jump>(),
                ComponentType.ReadOnly<PlayerData>(),
            },
        });
        m_EndSimulationEntityCommandBuffer = World.DefaultGameObjectInjectionWorld
            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        RequireForUpdate(abilQuery);
    }

//    [RequireComponentTag(typeof(PlayerData), typeof(Jump))]
//    private struct JumpJob : IJobForEachWithEntity<Translation, MoveSettings, Heading, CellIndex> {
//        public int2 bound;
//        public int dimX;
//
//        [ReadOnly] public DynamicBuffer<GridBuffer> grid;
//
//        public EntityCommandBuffer.Concurrent commandBuffer;
//
//        public void Execute(Entity entity, int index, ref Translation position,
//            [ReadOnly] ref MoveSettings moveData,
//            [ReadOnly] ref Heading direction) {
//            if (moveData.targetCellBlocked) {
//                var pos2d = new float2(position.Value.x, position.Value.y);
//                var pos = pos2d + 2 * direction.VectorDirection;
//                var coordObstacle = GridUtils.WorldToCellCoord(pos, grid[0].WorldPos);
//
//                if (!OutBound(coordObstacle)) {
//                    var indexCell = GridUtils.CoordToIndex(coordObstacle, dimX);
//                    var newWorldPos = GridUtils.CoordToWorld(grid, indexCell);
//                    if (grid[indexCell].Type == CellType.Ground) {
//                        position.Value = newWorldPos;
//                    }
//                }
//            }
//            commandBuffer.RemoveComponent<Jump>(index, entity);
//        }
//        private bool OutBound(int2 coord) {
//            return coord.x >= bound.x || coord.y >= bound.y;
//        }
//    }

    private static bool OutBound(int2 coord, int2 bound) {
        return coord.x >= bound.x || coord.y >= bound.y;
    }

//    protected override JobHandle OnUpdate(JobHandle inputDeps) {
//        var jumpJob = new JumpJob() {
//            bound = MazeBootstrap.Instance.bound,
//            dimX = MazeBootstrap.Instance.dimX,
//            grid = EntityManager.GetBuffer<GridBuffer>(GetSingletonEntity<GridTag>()),
//            commandBuffer = m_EndSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent(),
//        }.Schedule(this, inputDeps);
//        jumpJob.Complete();
//        m_EndSimulationEntityCommandBuffer.AddJobHandleForProducer(jumpJob);
//
//        return inputDeps;
//    }

    protected override void OnUpdate() {
        var bound = MazeBootstrap.Instance.bound;
        var dimX = MazeBootstrap.Instance.dimX;
        var grid = EntityManager.GetBuffer<GridBuffer>(GetSingletonEntity<GridTag>());
        var ecb = m_EndSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent();

        var jumpJobHandle =
            Entities.WithName("jumpJob")
                .WithAll<PlayerData, Jump, CellIndex>()
                .WithReadOnly(grid)
                .ForEach((Entity e, int entityInQueryIndex, ref MoveSettings moveData, ref Translation position,
                    in Heading direction) => {
                    if (moveData.targetCellBlocked) {
                        var pos2d = new float2(position.Value.x, position.Value.y);
                        var pos = pos2d + 2 * direction.VectorDirection;
                        var coordObstacle = GridUtils.WorldToCellCoord(pos, grid[0].WorldPos);

                        if (!OutBound(coordObstacle, bound)) {
                            var indexCell = GridUtils.CoordToIndex(coordObstacle, dimX);
                            var newWorldPos = GridUtils.CoordToWorld(grid, indexCell);
                            if (grid[indexCell].Type == CellType.Ground) {
                                position.Value = newWorldPos;
                            }
                        }
                    }

                    ecb.RemoveComponent<Jump>(entityInQueryIndex, e);
                }).Schedule(Dependency);
        m_EndSimulationEntityCommandBuffer.AddJobHandleForProducer(jumpJobHandle);
        Dependency = jumpJobHandle;
    }

    [UpdateInGroup(typeof(AbilitySystemGroup))]
    public class CastJumpSystem : ComponentSystem {
        private EntityQuery castJumpGroup;
        private EntityQuery playerQuery;

        protected override void OnCreate() {
            castJumpGroup = GetEntityQuery(new EntityQueryDesc() {
                All = new[] {
                    ComponentType.ReadOnly<Jump>(),
                    ComponentType.ReadOnly<AuxiliaryAbility>(),
                    ComponentType.ReadOnly<ActivateAbilityTag>(),
                },
                Options = EntityQueryOptions.FilterWriteGroup
            });
            playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerData>());
            RequireForUpdate(castJumpGroup);
            RequireForUpdate(playerQuery);
        }

        protected override void OnUpdate() {
            var jumpEntity = castJumpGroup.GetSingletonEntity();
            var jump = EntityManager.GetComponentData<Jump>(jumpEntity);
            var player = playerQuery.GetSingletonEntity();
            EntityManager.AddComponentData(player, jump);
            EntityManager.RemoveComponent<ActivateAbilityTag>(jumpEntity);
        }
    }
}