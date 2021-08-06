using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(PreSimulationSystemGroup))]
public class GridMovementSystem : SystemBase {
    private EntityQuery cellHeadingMovementGroupV2;
    private EntityQuery mazeGroup;

    protected override void OnCreate() {
        mazeGroup = GetEntityQuery(new EntityQueryDesc {
            All = new[] {
                ComponentType.ReadOnly<GridBuffer>()
            },
            Options = EntityQueryOptions.FilterWriteGroup
        });

        RequireSingletonForUpdate<GridBuffer>();

        cellHeadingMovementGroupV2 = GetEntityQuery(new EntityQueryDesc {
            All = new[] {
                ComponentType.ReadWrite<Heading>(),
                ComponentType.ReadWrite<CellIndex>(),
                ComponentType.ReadOnly<CellMovementTag>(),
                ComponentType.ReadWrite<MoveSettings>(),
                ComponentType.ReadWrite<IndicesForwardCellTemp>(),
            },
            None = new[] {
                ComponentType.ReadOnly<PathMovementTag>(),
            },
            //Options = EntityQueryOptions.FilterWriteGroup
        });
    }

    [BurstCompile]
    [RequireComponentTag(typeof(CellMovementTag))]
    [ExcludeComponent(typeof(PathMovementTag))]
    private struct TowardsCellIndexJobChunk : IJobChunk {
        public ArchetypeChunkComponentType<IndicesForwardCellTemp> IndicesForwardCellTempArchetype;
        public ArchetypeChunkComponentType<Heading> HeadingArchetype;
        [ReadOnly] public ArchetypeChunkComponentType<CellIndex> CellIndexArchetype;

        [ReadOnly] public int dimX;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var headingChunk = chunk.GetNativeArray(HeadingArchetype);
            var cellIndexChunk = chunk.GetNativeArray(CellIndexArchetype);
            var indicesForwardCellTempChunk = chunk.GetNativeArray(IndicesForwardCellTempArchetype);

            for (int i = 0; i < chunk.Count; i++) {
//                Debug.Log("cellIndex: " +  cellIndexChunk[i].Index);
                var indicesForwardCellTemp = indicesForwardCellTempChunk[i];
                indicesForwardCellTemp.indexForwardCell = GridUtils.ForwardCellIndex(cellIndexChunk[i].Index,
                    headingChunk[i].VectorDirection, dimX);
//                Debug.Log("indexForwardCell: "+IndicesForwardCellTempChunk[i].indexForwardCell);
                indicesForwardCellTempChunk[i] = indicesForwardCellTemp;
            }
        }
    }

    [BurstCompile]
    [RequireComponentTag(typeof(CheckWallTag), typeof(CellMovementTag))]
    [ExcludeComponent(typeof(PathMovementTag))]
    private struct CheckWallJobChunk : IJobChunk {
        [ReadOnly] 
        public DynamicBuffer<GridBuffer> mazeBuffer;

        public ArchetypeChunkComponentType<IndicesForwardCellTemp> IndicesForwardCellTempArchetype;
        public ArchetypeChunkComponentType<MoveSettings> MoveSettingsArchetype;

// is obsolete
//        [ReadOnly]
//        public NativeArray<int> forwardCellIndices;

//        public void Execute(Entity entity, int index, ref MoveSettings moveData,
//            [ChangedFilter] [ReadOnly] ref CellIndex cell) {
//            var forwardCellInd = forwardCellIndices[index];
//            if (mazeBuffer[forwardCellInd].Type == CellType.Wall) {
//                moveData.targetCellBlocked = true;
//                moveData.isMoving = false;
//            } else {
//                moveData.targetCellBlocked = false;
//                moveData.isMoving = true;
//            } 
//        }

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var indicesForwardCellTempChunk = chunk.GetNativeArray(IndicesForwardCellTempArchetype);
            var moveDataChunk = chunk.GetNativeArray(MoveSettingsArchetype);
            for (int i = 0; i < chunk.Count; i++) {
                var forwardCellInd = indicesForwardCellTempChunk[i].indexForwardCell;
                var moveData = moveDataChunk[i];
                if (forwardCellInd < 0 || mazeBuffer[forwardCellInd].Type == CellType.Wall) {
                    moveData.targetCellBlocked = true;
                    moveData.isMoving = false;
                }
                else {
                    moveData.targetCellBlocked = false;
                    moveData.isMoving = true;
                }

                moveDataChunk[i] = moveData;
            }
        }
    }

    //TODO job which checks walls in all directions


    [BurstCompile]
    [RequireComponentTag(typeof(CellMovementTag))]
    [ExcludeComponent(typeof(PathMovementTag))]
    private struct CellHeadingJobChunk : IJobChunk {
        public ArchetypeChunkComponentType<IndicesForwardCellTemp> IndicesForwardCellTempArchetype;

        [ReadOnly] 
        public ArchetypeChunkComponentType<CellIndex> CellIndexArchetype;

        public ArchetypeChunkComponentType<MoveSettings> MoveSettingsArchetype;
        [ReadOnly] 
        public DynamicBuffer<GridBuffer> mazeBuffer;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var indicesForwardCellTempChunk = chunk.GetNativeArray(IndicesForwardCellTempArchetype);
            var moveDataChunk = chunk.GetNativeArray(MoveSettingsArchetype);
            var cellIndexChunk = chunk.GetNativeArray(CellIndexArchetype);
            for (int i = 0; i < chunk.Count; i++) {
                var moveData = moveDataChunk[i];
                var cellIndex = cellIndexChunk[i];
                var indicesForwardCellTemp = indicesForwardCellTempChunk[i];
                if (moveData.targetCellBlocked) {
                    moveData.targetCellPos = mazeBuffer[cellIndex.Index].WorldPos;
                }
                else {
                    var forwardCellIndex = indicesForwardCellTemp.indexForwardCell; //forwardCellIndices[index];
                    moveData.targetCellPos = mazeBuffer[forwardCellIndex].WorldPos;
                }

                moveDataChunk[i] = moveData;
            }
        }
    }

    protected override void OnUpdate() {
        var entityGrid = mazeGroup.GetSingletonEntity();
        var gridBuffer = World.DefaultGameObjectInjectionWorld.EntityManager.GetBuffer<GridBuffer>(entityGrid);
        var dimX = MazeBootstrap.Instance.dimX;

        // var countMovement = cellHeadingMovementGroup.CalculateEntityCount();
        // var forwardCellIndices = new NativeArray<int>(countMovement, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var cellIndexArchetype = GetArchetypeChunkComponentType<CellIndex>(true);

//        Entities.WithName("Debug_UpdateCellIndex")
//            .WithoutBurst()
//            .ForEach((in CellIndex cellIndex) => {
//                Debug.Log("cellIndex.coord: " + cellIndex.coord + " cellIndex.Index: " + cellIndex.Index);
//            }).Run();

        var moveSettingsArchetype = GetArchetypeChunkComponentType<MoveSettings>();
        var headingArchetype = GetArchetypeChunkComponentType<Heading>();
        var indicesForwardCellTempArchetype = GetArchetypeChunkComponentType<IndicesForwardCellTemp>();

        var towardsCellIndexJobHandle = new TowardsCellIndexJobChunk {
            // indicesForwardCells = forwardCellIndices,
            IndicesForwardCellTempArchetype = indicesForwardCellTempArchetype,
            dimX = dimX,
            HeadingArchetype = headingArchetype,
            CellIndexArchetype = cellIndexArchetype
        }.Schedule(cellHeadingMovementGroupV2, Dependency);
        // Dependency = towardsCellIndexJobHandle;

        var checkWallJobHandle = new CheckWallJobChunk {
            mazeBuffer = gridBuffer,
            IndicesForwardCellTempArchetype = indicesForwardCellTempArchetype,
            MoveSettingsArchetype = moveSettingsArchetype,
            //forwardCellIndices = forwardCellIndices
        }.Schedule(cellHeadingMovementGroupV2, towardsCellIndexJobHandle);
        // Dependency = checkWallJobHandle;

        var cellHeadJobHandle = new CellHeadingJobChunk {
            //forwardCellIndices = forwardCellIndices,
            IndicesForwardCellTempArchetype = indicesForwardCellTempArchetype,
            MoveSettingsArchetype = moveSettingsArchetype,
            CellIndexArchetype = cellIndexArchetype,
            mazeBuffer = gridBuffer
        }.Schedule(cellHeadingMovementGroupV2, checkWallJobHandle);
        Dependency = cellHeadJobHandle;
        // cellHeadJob.Complete();
    }
}