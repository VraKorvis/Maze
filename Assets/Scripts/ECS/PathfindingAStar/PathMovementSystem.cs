using System.Diagnostics.CodeAnalysis;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(PathFindingSystem))]
public class PathMovementSystem : SystemBase {
    // private EntityQuery pathMoveGroup;
    // private EntityQuery mazeGroup;

    private EntityManager em;
    private EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    protected override void OnCreate() {
        // pathMoveGroup = GetEntityQuery(new EntityQueryDesc {
        //     All = new[] {
        //         // ComponentType.ReadWrite<PathAgentStatus>(),
        //         ComponentType.ReadWrite<PathRequestAgent>(),
        //         ComponentType.ReadWrite<MoveSettings>(),
        //         ComponentType.ReadWrite<Heading>(),
        //         ComponentType.ReadOnly<Translation>(),
        //     },
        //     Options = EntityQueryOptions.FilterWriteGroup
        // });
        // RequireForUpdate(pathMoveGroup);
        // mazeGroup = GetEntityQuery(new EntityQueryDesc {
        //     All = new[] {
        //         ComponentType.ReadOnly<GridBuffer>()
        //     },
        //     Options = EntityQueryOptions.FilterWriteGroup
        // });

        em = World.DefaultGameObjectInjectionWorld.EntityManager;
        m_EndSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    // is obsolete
    // IJobForEach_BCC Have bug InvalidOperationException: The native container has been declared as [WriteOnly] in the job, but you are reading from it.
    //  wait until fix that. Use Buffer BufferFromEntity instead 
//    https: //forum.unity.com/threads/ijobforeach_b-dynamic-buffer-is-write-only.681667/

//    [RequireComponentTag(typeof(PathMovementTag))]
//    public struct PathMoveJob : IJobForEach_BCC<Waypoint, MoveSettings, Translation> {
//        
//        [ReadOnly]
//        public float dt;
//
//        public void Execute(DynamicBuffer<Waypoint> way, ref MoveSettings moveData, ref Translation position) {
//            var maxDistanceDelta = moveData.Speed * dt;
//            var lensqr = math.lengthsq(way[0].point - position.Value);
//            bool close = (double) lensqr == 0.0f || lensqr <= maxDistanceDelta * (double) maxDistanceDelta;
//            if (way.Length > 1 && close) {
//                way.RemoveAt(0);
//            }
//            moveData.targetCellPos = way[0].point;
//        }
//    }

    // TODO remake to Enities.FOrEach
    // [BurstCompile]
    // // [RequireComponentTag(typeof(PathRequestAgent))]
    // private struct PathMoveJob : IJobChunk {
    //     //ForEachWithEntity<MoveSettings, Translation, PathAgentStatus, Heading> {
    //     public float dt;
    //
    //     [NativeDisableParallelForRestriction] public BufferFromEntity<Waypoint> wayBuffer;
    //     public ArchetypeChunkEntityType EntitiesArchetype;
    //     public ArchetypeChunkComponentType<MoveSettings> MoveSettingsArchetype;
    //     [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationArchetype;
    //     public ArchetypeChunkComponentType<PathAgentStatus> PathAgentStatusArchetype;
    //     public ArchetypeChunkComponentType<Heading> HeadingArchetype;
    //
    //     public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
    //         var entitiesChunk = chunk.GetNativeArray(EntitiesArchetype);
    //         var moveSettingsChunk = chunk.GetNativeArray(MoveSettingsArchetype);
    //         var translationChunk = chunk.GetNativeArray(TranslationArchetype);
    //         var pathAgentStatusChunk = chunk.GetNativeArray(PathAgentStatusArchetype);
    //         var headingChunk = chunk.GetNativeArray(HeadingArchetype);
    //         for (int i = 0; i < chunk.Count; i++) {
    //             var entity = entitiesChunk[i];
    //             var status = pathAgentStatusChunk[i];
    //             var position = translationChunk[i];
    //             var moveData = moveSettingsChunk[i];
    //             var heading = headingChunk[i];
    //             var way = wayBuffer[entity];
    //
    //             headingChunk[i] = heading;
    //             pathAgentStatusChunk[i] = status;
    //         }
    //     }
    // }

    private static int2 CalcVectorDirection(MoveSettings moveData, Translation position) {
        var dir = math.normalizesafe(moveData.targetCellPos - position.Value);
        int2 ndir = new int2((int) math.round(dir.x), (int) math.round(dir.y));
        //TODO test performance (low perfomance) 
        // if (ndir.x == ndir.y) {
        //     if (dir.x > dir.y) {
        //         ndir.x = 1;
        //         ndir.y = 0;
        //     }
        //     else {
        //         ndir.x = 0;
        //         ndir.y = 1;
        //     }
        // }
        return ndir;
    }

    protected override void OnUpdate() {
        float dt = Time.DeltaTime;

        // is obsolete
//        var pathMoveJob = new PathMoveJob() {
//            EntitiesArchetype = GetArchetypeChunkEntityType(),
//            MoveSettingsArchetype = GetArchetypeChunkComponentType<MoveSettings>(),
//            TranslationArchetype = GetArchetypeChunkComponentType<Translation>(true),
//            HeadingArchetype = GetArchetypeChunkComponentType<Heading>(),
//            PathAgentStatusArchetype = GetArchetypeChunkComponentType<PathAgentStatus>(),
//            wayBuffer = GetBufferFromEntity<Waypoint>(),
//            dt = dt,
//        }.Schedule(pathMoveGroup, Dependency);

        // old ver
        //var wayBuffer = GetBufferFromEntity<Waypoint>();
        // var pathMoveJobHandle =
        // Entities.WithName("Path_Move_FRJob")
        //     .WithBurst()
        //     // .WithReadOnly(wayBuffer)
        //     .ForEach((Entity entity, ref MoveSettings moveData, ref Translation position,
        //         ref PathAgentStatus status,
        //         ref Heading heading) => {
        //         var way = wayBuffer[entity];
        //         if (way.Length == 0) {
        //             status.Value = AgentStatus.None;
        //             return;
        //         }
        //
        //         int lastIndex = way.Length - 1;
        //         if (status.Value == AgentStatus.Ready) {
        //             moveData.targetCellPos = way[lastIndex].point;
        //             status.Value = AgentStatus.Process;
        //             heading.VectorDirection = CalcVectorDirection(moveData, position);
        //             return;
        //         }
        //
        //         if (status.Value == AgentStatus.Process) {
        //             var sqrlen = math.lengthsq(way[lastIndex].point - position.Value);
        //             var maxDistanceDelta = moveData.speed * dt;
        //             bool close = (double) sqrlen == 0.0f || sqrlen <= maxDistanceDelta * (double) maxDistanceDelta;
        //             if (close) {
        //                 way.RemoveAt(lastIndex);
        //                 lastIndex--;
        //                 if (lastIndex != -1) {
        //                     moveData.targetCellPos = way[lastIndex].point;
        //                     heading.VectorDirection = CalcVectorDirection(moveData, position);
        //                 }
        //             }
        //         }
        //     }).Schedule();
        //Dependency = pathMoveJobHandle;

        // var wayBuffer = GetBufferFromEntity<Waypoint>();
        // var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

        // var pathMoveFrJobMoving =
        Entities.WithName("Path_Move_FRJob_Moving")
            .WithBurst()
            // .WithReadOnly(wayBuffer)
            .WithAll<Spirit, HauntingTag, PathAgentStatusProcessTag>()
            // .WithNone<PathAgentStatusNoneTag, PathAgentStatusReadyTag, PathAgentStatusFindTag>()
            .ForEach((ref DynamicBuffer<Waypoint> way, ref MoveSettings moveData, ref Heading heading,
                in Translation position) => {
                // var way = wayBuffer[entity];
                if (way.Length == 0) {
                    return;
                }

                int lastIndex = way.Length - 1;
                var sqrlen = math.lengthsq(way[lastIndex].point - position.Value);
                var maxDistanceDelta = moveData.speed * dt;
                bool close = (double) sqrlen == 0.0f || sqrlen <= maxDistanceDelta * (double) maxDistanceDelta;
                moveData.targetCellPos = way[lastIndex].point;
                heading.VectorDirection = CalcVectorDirection(moveData, position);

                if (close) {
                    way.RemoveAt(lastIndex);
                }
            }).ScheduleParallel();
    }
}