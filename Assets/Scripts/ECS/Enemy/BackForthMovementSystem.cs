using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PreSimulationSystemGroup))]
public class BackForthMovementSystem : SystemBase {
    private EntityQuery m_BackForthGroup;
    private EntityQuery m_CellMoving;
    private EntityQuery m_PathFinding;
    private EntityQuery m_MazeGroup;
    private EntityQuery m_Query;

    protected override void OnCreate() {
        var backForthEntityQueryDesk = new EntityQueryDesc {
            All = new[] {
                ComponentType.ReadWrite<BackForth>(),
                ComponentType.ReadWrite<MoveSettings>(),
                ComponentType.ReadWrite<Heading>(),
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<CellMovementTag>(),
            },
//            None = new[] {
//                ComponentType.ReadOnly<FearDebuff>(),
//                ComponentType.ReadOnly<AfterFearDebuffPathMoveTag>()
//            },
            Options = EntityQueryOptions.FilterWriteGroup
        };
        m_BackForthGroup = GetEntityQuery(backForthEntityQueryDesk);

        var pathFindingEntityQueryDesk = new EntityQueryDesc {
            All = new[] {
                ComponentType.ReadWrite<PathAgentStatus>(),
                ComponentType.ReadWrite<PathRequestAgent>(),
            },
            None = new[] {
                ComponentType.ReadOnly<FearDebuff>(),
                ComponentType.ReadOnly<AfterFearDebuffPathMoveTag>()
            },
            Options = EntityQueryOptions.FilterWriteGroup
        };
        m_PathFinding = GetEntityQuery(pathFindingEntityQueryDesk);
        m_MazeGroup = GetEntityQuery(new EntityQueryDesc {
            All = new[] {
                ComponentType.ReadOnly<GridBuffer>()
            },
            Options = EntityQueryOptions.FilterWriteGroup
        });
        // m_Query = GetEntityQuery(new EntityQueryDesc[] {backForthEntityQueryDesk, pathFindingEntityQueryDesk});
        RequireForUpdate(m_BackForthGroup);
    }

//     another way how should BFSpirits move
//    [BurstCompile]
//    [RequireComponentTag(typeof(CellMovementTag))]
//    [ExcludeComponent(typeof(PathMovementTag), typeof(FearDebuff))]
//    private struct BackForthJob : IJobForEach<BackForth, MoveSettings, Heading, Translation> {
//        [ReadOnly] public float dt;
//
//        public void Execute(ref BackForth backForth, ref MoveSettings moveSettings, ref Heading heading,
//            [ReadOnly] ref Translation position) {
//            var vectorDirectionCharacter = new float3(heading.VectorDirection.x, heading.VectorDirection.y, 0);
//
//            var directionTargetPoint = backForth.firstPoint - position.Value;
//
//            var dotAB = math.dot(vectorDirectionCharacter, directionTargetPoint);
//            var clampPoint = dotAB > 0 ? backForth.firstPoint : backForth.secondPoint;
//
//            var lensqr = math.lengthsq(clampPoint - position.Value);
//            var maxDistanceDelta = moveSettings.speed * dt;
//
//            bool distanceToDestination =
//                (double) lensqr == 0.0f || lensqr <= maxDistanceDelta * (double) maxDistanceDelta;
//            if (moveSettings.targetCellBlocked || distanceToDestination) {
//                SwapPoints(ref backForth);
//                heading.VectorDirection = TurnAround180(heading.VectorDirection);
//                moveSettings.isMoving = true;
//            }
//
//            vectorDirectionCharacter = new float3(heading.VectorDirection.x, heading.VectorDirection.y, 0);
//            var vectorCharacterAndPointFirst = backForth.firstPoint - position.Value;
//            var vectorCharacterAndPointSecond = backForth.secondPoint - position.Value;
//            if (math.length(vectorCharacterAndPointFirst) > backForth.distanceBetween
//                || (math.length(vectorCharacterAndPointSecond) > backForth.distanceBetween)) {
//                SwapPoints(ref backForth);
//                var dotF = math.dot(vectorDirectionCharacter, vectorCharacterAndPointFirst);
//                var dotS = math.dot(vectorDirectionCharacter, vectorCharacterAndPointSecond);
//                if (dotF < 0 || dotS < 0) {
//                    heading.VectorDirection = TurnAround180(heading.VectorDirection);
//                }
//            }
//        }
//
//        private void SwapPoints(ref BackForth backForth) {
//            var tmp = backForth.firstPoint;
//            backForth.firstPoint = backForth.secondPoint;
//            backForth.secondPoint = tmp;
//        }
//
//        private int2 TurnAround180(int2 vector) {
//            var newVector = math.round(math.mul(quaternion.Euler(0, 0, math.radians(180)),
//                new float3(vector.x, vector.y, 0)));
//            return new int2((int) newVector.x, (int) newVector.y);
//        }
//    }

    [BurstCompile]
    [RequireComponentTag(typeof(CellMovementTag), typeof(BackForth))]
    [ExcludeComponent(typeof(FearDebuff), typeof(AfterFearDebuffPathMoveTag))]
    private struct Ver2BackForthJobChunk : IJobChunk {
        [ReadOnly] public float dt;
        public float3 originWorldPos;

        public ArchetypeChunkComponentType<PathAgentStatus> PathAgentStatusArchetype;
        public ArchetypeChunkComponentType<PathRequestAgent> PathRequestAgentArchetype;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationArchetype;
        public ArchetypeChunkComponentType<Heading> HeadingArchetype;
        public ArchetypeChunkComponentType<MoveSettings> MoveSettingsArchetype;
        public ArchetypeChunkComponentType<BackForth> BackForthArchetype;
        [ReadOnly] public ArchetypeChunkEntityType EntityArchetype;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var pathAgentStatusChunk = chunk.GetNativeArray(PathAgentStatusArchetype);
            var pathRequestAgentChunk = chunk.GetNativeArray(PathRequestAgentArchetype);
            var translationChunk = chunk.GetNativeArray(TranslationArchetype);
            var headingChunk = chunk.GetNativeArray(HeadingArchetype);
            var moveSettingsChunk = chunk.GetNativeArray(MoveSettingsArchetype);
            var backForthChunk = chunk.GetNativeArray(BackForthArchetype);
            var entityChunk = chunk.GetNativeArray(EntityArchetype);

            for (int i = 0; i < chunk.Count; i++) {
                var status = pathAgentStatusChunk[i];
                var agent = pathRequestAgentChunk[i];
                var backForth = backForthChunk[i];
                var position = translationChunk[i];
                var moveSettings = moveSettingsChunk[i];
                var heading = headingChunk[i];
                var entity = entityChunk[i];
                if (status.Value == AgentStatus.None) {
                    agent.destination = GridUtils.WorldToCellCoord(backForth.firstPoint, originWorldPos);
                    agent.owner = entity;
                    agent.focus = Entity.Null;
                    agent.startCoord = GridUtils.WorldToCellCoord(position.Value, originWorldPos);
                    pathRequestAgentChunk[i] = agent;
                    status.Value = AgentStatus.Find;
                    pathAgentStatusChunk[i] = status;
                }

                var lensqr = math.lengthsq(backForth.firstPoint - position.Value);

                var maxDistanceDelta = moveSettings.speed * dt;

                bool close = (double) lensqr == 0.0f || lensqr <= maxDistanceDelta * (double) maxDistanceDelta;
                if (close) {
                    SwapPoints(ref backForth);
                    moveSettings.isMoving = true;
                    moveSettingsChunk[i] = moveSettings;
                    var dir = math.normalizesafe(backForth.firstPoint - position.Value);
                    heading.VectorDirection = new int2((int) math.round(dir.x), (int) math.round(dir.y));
                    headingChunk[i] = heading;
                }
            }
        }
    }

    //TODO need optimisation
    protected override void OnUpdate() {
        //var backForthJob = new BackForthJob {dt = Time.DeltaTime}.Schedule(backForthGroup, inputDeps);

        var entityGrid = m_MazeGroup.GetSingletonEntity();
        var gridBuffer = World.DefaultGameObjectInjectionWorld.EntityManager.GetBuffer<GridBuffer>(entityGrid);

//        var ver2BackForthJob = new Ver2BackForthJobChunk {
//            PathAgentStatusArchetype = GetArchetypeChunkComponentType<PathAgentStatus>(),
//            PathRequestAgentArchetype = GetArchetypeChunkComponentType<PathRequestAgent>(),
//            TranslationArchetype = GetArchetypeChunkComponentType<Translation>(true),
//            HeadingArchetype = GetArchetypeChunkComponentType<Heading>(),
//            MoveSettingsArchetype = GetArchetypeChunkComponentType<MoveSettings>(),
//            BackForthArchetype = GetArchetypeChunkComponentType<BackForth>(),
//            EntityArchetype = GetArchetypeChunkEntityType(),
//            dt = Time.DeltaTime,
//            originWorldPos = gridBuffer[0].WorldPos,
//        }.Schedule(m_Query, Dependency);
//        ver2BackForthJob.Complete();
//        (Entity entity, ref BackForth backForth,  ref PathRequestAgent pathReq, in Translation position, ref Heading heading, ref MoveSettings moveSettings)

        Entities.WithName("BackForth_CheckAgentStatus")
            .WithAll<CellMovementTag>()
            .WithReadOnly(gridBuffer)
            .WithNone<FearDebuff, AfterFearDebuffPathMoveTag, PathMovementTag>()
            .ForEach((Entity entity, ref PathAgentStatus status, ref PathRequestAgent pathReq, in BackForth backForth,
                in Translation position) => {
                if (status.Value == AgentStatus.None) {
                    pathReq.destination = GridUtils.WorldToCellCoord(backForth.firstPoint, gridBuffer[0].WorldPos);
                    pathReq.owner = entity;
                    pathReq.focus = Entity.Null;
                    pathReq.startCoord = GridUtils.WorldToCellCoord(position.Value, gridBuffer[0].WorldPos);
                    status.Value = AgentStatus.Find;
                }
            }).ScheduleParallel();

        var dt = Time.DeltaTime;
        Entities.WithName("BackForth_Path_Job")
            .WithAll<CellMovementTag>()
            .WithNone<FearDebuff, AfterFearDebuffPathMoveTag, PathMovementTag>()
            .ForEach((ref MoveSettings moveSettings,
                ref Heading heading, ref BackForth backForth, in Translation position) => {
                var lensqr = math.lengthsq(backForth.firstPoint - position.Value);
                var maxDistanceDelta = moveSettings.speed * dt;
                bool close = (double) lensqr == 0.0f || lensqr <= maxDistanceDelta * (double) maxDistanceDelta;
                if (close) {
                    SwapPoints(ref backForth);
                    moveSettings.isMoving = true;
                    var dir = math.normalizesafe(backForth.firstPoint - position.Value);
                    heading.VectorDirection = new int2((int) math.round(dir.x), (int) math.round(dir.y));
                }
            }).ScheduleParallel();
    }

    private static void SwapPoints(ref BackForth backForth) {
        var tmp = backForth.firstPoint;
        backForth.firstPoint = backForth.secondPoint;
        backForth.secondPoint = tmp;
    }
}