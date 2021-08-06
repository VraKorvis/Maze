using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//TODO TO OPTIMIZE
[UpdateInGroup(typeof(PreSimulationSystemGroup))]
//[DisableAutoCreation]
public class HauntingSpiritUpdatePathSystem : SystemBase {
    private EntityQuery m_playerGroup;
    // private EntityQuery m_bossQuery;
    // private EntityQuery m_HauntingGroup;

    // EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    protected override void OnCreate() {
        // m_HauntingGroup = GetEntityQuery(new EntityQueryDesc() {
        //     All = new[] {
        //         ComponentType.ReadOnly<HauntingTag>(),
        //         ComponentType.ReadOnly<Spirit>(),
        //         ComponentType.ReadWrite<PathRequestAgent>(),
        //         // ComponentType.ReadWrite<PathAgentStatus>(),
        //         ComponentType.ReadOnly<CellIndex>(),
        //         // ComponentType.ReadWrite<TargetForHaunting>(),
        //     },
        //     Options = EntityQueryOptions.FilterWriteGroup,
        // });
        m_playerGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadOnly<PlayerData>(),
                ComponentType.ReadOnly<CellIndex>(),
            },
            Options = EntityQueryOptions.FilterWriteGroup,
        });
        // m_playerGroup.SetChangedVersionFilter(typeof(CellIndex));
        RequireForUpdate(m_playerGroup);
        // m_EndSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
//        m_bossQuery = GetEntityQuery(new EntityQueryDesc() {
//            All = new[] {
//                ComponentType.ReadOnly<HauntingTag>(),
//                ComponentType.ReadOnly<Spirit>(),
//            },
//            Options = EntityQueryOptions.FilterWriteGroup,
//        });
//        RequireForUpdate(m_bossQuery);

        // RequireForUpdate(m_HauntingGroup);
    }

    //TODO STUB, duct tape,USELESS Job
    [RequireComponentTag(typeof(Spirit), typeof(HauntingTag))]
    [ExcludeComponent(typeof(PathRequestAgent))]
    private struct ChangeAgentStatusToFindJob : IJobChunk {
        public ArchetypeChunkComponentType<PathAgentStatus> PathAgentStatusArchetype;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var pathAgentStatusChunk = chunk.GetNativeArray(PathAgentStatusArchetype);

            for (int i = 0; i < chunk.Count; i++) {
                var status = pathAgentStatusChunk[i];
                if (status.Value == AgentStatus.AddPathRequest || status.Value == AgentStatus.None) {
                    status.Value = AgentStatus.Find;
                    pathAgentStatusChunk[i] = status;
                }
            }
        }
    }

    //[BurstCompile]
    [RequireComponentTag(typeof(Spirit), typeof(HauntingTag))]
    private struct UpdatePathRequestEachCell : IJobChunk {
        // public int2 coordGoal;
        // public Entity focus;

        [ReadOnly]
        public ArchetypeChunkComponentType<CellIndex> CellIndexArchType;

        [ReadOnly]
        public ArchetypeChunkComponentType<TargetForHaunting> TargetForHauntingArchType;

        public ArchetypeChunkComponentType<PathAgentStatus> PathAgentStatusArchType;
        public ArchetypeChunkComponentType<PathRequestAgent> PathRequestAgentArchType;
        public ArchetypeChunkEntityType EntitiesArch;
        public uint LastSystemVersion;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var cellIndexChunk = chunk.GetNativeArray(CellIndexArchType);
            var pathAgentStatusChunk = chunk.GetNativeArray(PathAgentStatusArchType);
            var pathRequestAgentChunk = chunk.GetNativeArray(PathRequestAgentArchType);
            var targetForHauntingChunk = chunk.GetNativeArray(TargetForHauntingArchType);
            var entitiesCHunk = chunk.GetNativeArray(EntitiesArch);

            var cellIndexChangedChunk = chunk.DidChange(CellIndexArchType, LastSystemVersion);
            if (cellIndexChangedChunk) return;

            Debug.Log("InsideJob");
            for (int i = 0; i < chunk.Count; i++) {
                Debug.Log("chunk.Count " + chunk.Count);
                var cellIndex = cellIndexChunk[i];
                var status = pathAgentStatusChunk[i];
                var pathRequest = pathRequestAgentChunk[i];
                var targetForHaunting = targetForHauntingChunk[i];
                var entity = entitiesCHunk[i];
                // if (status.Value == AgentStatus.Process || status.Value == AgentStatus.Find ||
                //     status.Value == AgentStatus.Wait || status.Value == AgentStatus.None) {
                if (!targetForHaunting.cellIndex.coord.Equals(pathRequest.destination)) {
                    pathRequest.destination = targetForHaunting.cellIndex.coord;
                    pathRequest.focus = targetForHaunting.focus;
                    pathRequest.startCoord = cellIndex.coord;
                    pathRequest.owner = entity;
                    status.Value = AgentStatus.Find;

                    pathRequestAgentChunk[i] = pathRequest;
                    pathAgentStatusChunk[i] = status;
                }

                // }
            }
        }
    }

    protected override void OnUpdate() {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (m_playerGroup.CalculateEntityCount() == 0) return; // fix bug RequiredForUpdate(m_playerGroup)
        var playerEntity = m_playerGroup.GetSingletonEntity();
        if (playerEntity == Entity.Null) return; // RequiredForUpdate(m_playerGroup)
        var playerCellCoord = em.GetComponentData<CellIndex>(playerEntity);
        if (playerCellCoord.Index < 0) return;

        // var ecb = new EntityCommandBuffer(Allocator.TempJob);
        // var changeStatusToFind = new ChangeAgentStatusToFindJob() {
        //     PathAgentStatusArchetype = GetArchetypeChunkComponentType<PathAgentStatus>()
        // }.Schedule(m_HauntingGroup);
        // changeStatusToFind.Complete();

        // Entities.WithName("Add_PathRequest_Job")
        //     //.WithChangeFilter<PathAgentStatus>()
        //     .WithoutBurst()
        //     .WithStructuralChanges()
        //     .WithNone<PathRequestAgent>()
        //     .WithAll<Spirit, HauntingTag>()
        //     .ForEach((Entity entity) => {
        //         // if (status.Value == AgentStatus.AddPathRequest || status.Value == AgentStatus.None ||
        //         //     status.Value == AgentStatus.Find) {
        //         //     em.AddComponent<PathRequestAgent>(entity);
        //         // }
        //         em.AddComponent<PathRequestAgent>(entity);
        //         
        //     }).Run();

        // ecb.Playback(EntityManager);
        // ecb.Dispose();

        var targetCellCoord = playerCellCoord.coord;
        var focus = playerEntity;
        // var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();
        // var hauntingEntities = m_HauntingGroup.ToEntityArray(Allocator.TempJob);
        //
        // var addTargetForHauntingIfPlayerPositionChangedJobHandle =
        //     Entities.WithName("AddTargetFOrHauntingIfPlayerPositionChangedJob")
        //         .WithAll<PlayerData>()
        //         .WithChangeFilter<CellIndex>()
        //         .WithDeallocateOnJobCompletion(hauntingEntities)
        //         .WithReadOnly(hauntingEntities)
        //         .WithBurst()
        //         .ForEach((Entity entity, int entityInQueryIndex, ref CellIndex cell) => {
        //             for (int i = 0; i < hauntingEntities.Length; i++) {
        //                 ecb.SetComponent(entityInQueryIndex, hauntingEntities[i], new TargetForHaunting() {
        //                     focus = entity,
        //                     cellIndex = cell,
        //                 });
        //             }
        //         }).ScheduleParallel(Dependency);
        // Dependency = addTargetForHauntingIfPlayerPositionChangedJobHandle;


        // TODO NEED OPTIMIZE the System
        // var updateTargetForHauntingJobHandle =
        // Entities.WithName("UpdateTargetForHauntingJob")
        //     .WithBurst()
        //     //.WithChangeFilter<TargetForHaunting>()
        // .ForEach((ref TargetForHaunting target) => {
        //         target.focus = focus;
        //         target.cellIndex.coord = coordGoal;
        //     }).ScheduleParallel(Dependency);

        // var updatePathRequest = new UpdatePathRequestEachCell() {
        //     PathAgentStatusArchType = GetArchetypeChunkComponentType<PathAgentStatus>(),
        //     PathRequestAgentArchType = GetArchetypeChunkComponentType<PathRequestAgent>(),
        //     CellIndexArchType = GetArchetypeChunkComponentType<CellIndex>(true),
        //     TargetForHauntingArchType = GetArchetypeChunkComponentType<TargetForHaunting>(),
        //     EntitiesArch = GetArchetypeChunkEntityType(),
        //     LastSystemVersion = this.LastSystemVersion,
        // }.ScheduleParallel(m_HauntingGroup, Dependency);
        // Dependency = updatePathRequest;
        // updatePathRequest.Complete();

        // var updatePathReqForEachCell =
            Entities.WithName("UpdatePathReqForEachCell")
               // .WithBurst()
                // .WithChangeFilter<CellIndex>()
                .WithAll<Spirit, HauntingTag, PathAgentStatusProcessTag>() //TargetForHaunting
                //.WithAny<PathAgentStatusNoneTag, PathAgentStatusProcessTag>()
                .ForEach((Entity entity, ref PathRequestAgent pathRequest,
                    in CellIndex cellIndex) => {
                    // if (status.Value == AgentStatus.Process || status.Value == AgentStatus.Find ||
                    //     status.Value == AgentStatus.Wait || status.Value == AgentStatus.None) {
                    if (!targetCellCoord.Equals(pathRequest.startCoord))
                    {
                        pathRequest.destination = targetCellCoord;
                        pathRequest.focus = focus;
                        pathRequest.startCoord = cellIndex.coord;
                        pathRequest.owner = entity;
                        // ecb.AddComponent<PathAgentStatusFindTag>(entityInQueryIndex, entity);
                        // ecb.AddComponent<PathAgentStatusProcessTag>(entityInQueryIndex, entity);
                        // ecb.RemoveComponent<PathAgentStatusNoneTag>(entityInQueryIndex, entity);
                    // }
                    }

                    // if (!cellIndex.coord.Equals(pathRequest.destination)) {
                    //         pathRequest.destination = coordGoal;
                    //         pathRequest.focus = focus;
                    //         pathRequest.startCoord = cellIndex.coord;
                    //         pathRequest.owner = entity;
                    //         status.Value = AgentStatus.Find;
                    //     }
                }).ScheduleParallel();

        // Dependency = updatePathReqForEachCell;
        // m_EndSimulationEcbSystem.AddJobHandleForProducer(updatePathReqForEachCell);
        // updatePathReqForEachCell.Complete();
//        Entities.WithName("Debug_Check_toPathList")
//            .WithoutBurst()
//            .ForEach((in PathRequestAgent path) => {
//                Debug.Log(String.Format("path.destination: {0}, path.owner: {1}, path.focus: {2} path.startCoord: {3}"  ,path.destination, path.owner, path.focus, path.startCoord));
//            }).Run();
        // m_EndSimulationEcbSystem.AddJobHandleForProducer(addTargetForHauntingIfPlayerPositionChangedJobHandle);
    }
}