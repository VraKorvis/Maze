using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(InitializationGroup))]
//[DisableAutoCreation]
public class UpdateCellIndexSystem : SystemBase {
    private EntityQuery m_cellGroup;
    private EntityQuery m_mazeGroup;

    private EntityManager em;
    protected override void OnCreate() {
        // Enabled = SceneManager.GetActiveScene().name.StartsWith("maze");
        m_cellGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<CellIndex>(),
            },
            Options = EntityQueryOptions.FilterWriteGroup
        });
        m_cellGroup.SetChangedVersionFilter(typeof(Translation));
        m_mazeGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadOnly<GridBuffer>()
            },
            Options = EntityQueryOptions.FilterWriteGroup
        });
        RequireSingletonForUpdate<GridBuffer>();
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

//    [BurstCompile] // is obsolete
//    private struct UpdateCellIndexForEachCharacterInMazeJob : IJobForEach<CellIndex, Translation> {
//
//        [ReadOnly]
//        public DynamicBuffer<GridBuffer> mazeBuffer;
//
//        public int dimX;
//
//        public void Execute(ref CellIndex cellIndex, [ChangedFilter] [ReadOnly] ref Translation position) {
//            var cellCoord = GridUtils.WorldToCellCoord(mazeBuffer[0].WorldPos, position.Value);
//            var index     = GridUtils.CoordToIndex(cellCoord, dimX);
//            if (index != cellIndex.Index) {
//                cellIndex.Index = mazeBuffer[index].Index;
//                cellIndex.coord = cellCoord;
//            }
//        }
//    }

    [BurstCompile]
    private struct UpdateCellIndexForEachCharacterInMazeJob : IJobChunk {
        //IJobForEach<CellIndex, Translation>

        [ReadOnly]
        public DynamicBuffer<GridBuffer> mazeBuffer;

        public int dimX;

        public ArchetypeChunkComponentType<CellIndex> CellIndexType;

        [ReadOnly]
        public ArchetypeChunkComponentType<Translation> TranslationType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var chunkCellIndex = chunk.GetNativeArray(CellIndexType);
            var chunkTranslation = chunk.GetNativeArray(TranslationType);
            for (int i = 0; i < chunk.Count; i++) {
                var cellIndex = chunkCellIndex[i];
                var cellCoord = GridUtils.WorldToCellCoord(mazeBuffer[0].WorldPos, chunkTranslation[i].Value);
                var index = GridUtils.CoordToIndex(cellCoord, dimX);
                if (index != cellIndex.Index) {
                    cellIndex.Index = mazeBuffer[index].Index;
                    cellIndex.coord = cellCoord;
                }

                chunkCellIndex[i] = cellIndex;
            }
        }
    }

    protected override void OnUpdate() {
        var entityGrid = m_mazeGroup.GetSingletonEntity();
        var gridBuffer = em.GetBuffer<GridBuffer>(entityGrid);
        var dimX = MazeBootstrap.Instance.dimX;

        // TODO fix NullReferenceException: Object reference not set to an instance of an object when ScheduleParallel
        // var updateCellCoordJob = 
        //     new UpdateCellIndexForEachCharacterInMazeJob {
        //     CellIndexType = GetArchetypeChunkComponentType<CellIndex>(),
        //     TranslationType = GetArchetypeChunkComponentType<Translation>(true),
        //     mazeBuffer = gridBuffer,
        //     dimX = dimX
        // }.Schedule(m_cellGroup);

        var updateCellCoordJobHandle =
            Entities.WithName("UpdateCellIndexForEachCharacterInMazeJob")
                .WithBurst()
                .WithReadOnly(gridBuffer)
                .ForEach((ref CellIndex cellIndex, in Translation translation) => {
                    var cellCoord = GridUtils.WorldToCellCoord(gridBuffer[0].WorldPos, translation.Value);
                    var index = GridUtils.CoordToIndex(cellCoord, dimX);
                    if (index != cellIndex.Index) {
                        cellIndex.Index = gridBuffer[index].Index;
                        cellIndex.coord = cellCoord;
                    }
                }).ScheduleParallel(Dependency);
        Dependency = updateCellCoordJobHandle;
        updateCellCoordJobHandle.Complete();

//        Entities.WithName("Debug_UpdateCellIndex")
//            .WithoutBurst()
//            .ForEach((in CellIndex cellIndex) => {
//                Debug.Log("cellIndex.coord: " + cellIndex.coord + " cellIndex.Index: " + cellIndex.Index );
//            }).Run();
    }
}