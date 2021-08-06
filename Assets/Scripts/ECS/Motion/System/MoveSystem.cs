using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


[UpdateInGroup(typeof(PreSimulationSystemGroup))]
public class MoveSystem : SystemBase {
    private EntityQuery moveGroup;
    private EntityQuery m_FCR_group;
    private EntityQuery m_circle_group;

    protected override void OnCreate() {
        //    Enabled = SceneManager.GetActiveScene().name.StartsWith("maze");
        moveGroup = GetEntityQuery(new EntityQueryDesc {
            All = new[] {
                ComponentType.ReadOnly<MoveSettings>(),
                ComponentType.ReadOnly<Heading>(),
                ComponentType.ReadWrite<Translation>(),
            },
            Options = EntityQueryOptions.FilterWriteGroup
        });
        RequireForUpdate(moveGroup);
        m_FCR_group = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadOnly<MoveSettings>(),
                ComponentType.ReadWrite<FlatCurveRose>(),
                ComponentType.ReadWrite<Translation>(),
            },
        });
        //m_FCR_group.SetChangedVersionFilter(new ComponentType []{ typeof(InputA), typeof(InputB)});

        m_circle_group = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadOnly<MoveSettings>(),
                ComponentType.ReadWrite<Circle>(),
                ComponentType.ReadWrite<Translation>(),
            },
        });
    }

//    [BurstCompile]
//    [RequireComponentTag(typeof(CellMovementTag))]
//    private struct MoveTowardsJob : IJobForEach<MoveSettings, Translation> {
//        public float dt;
//
//        public void Execute([ReadOnly] ref MoveSettings moveSettings, ref Translation position) {
//            position.Value =
//                PhysicsUtils.MoveTowards(position.Value, moveSettings.targetCellPos, moveSettings.speed * dt);
//        }
//    }

// MoveCircleJob, MoveCurveOfEightJob, MoveTowardsJob : IJobForEach is obsolete -> IJobChunk
//    [BurstCompile]
//    [ExcludeComponent(typeof(PathMovementTag), typeof(FearDebuff), typeof(FlatCurveRose),
//        typeof(AfterFearDebuffPathMoveTag))]
//    private struct MoveCircleJob : IJobForEach<Circle, MoveSettings, Translation> {
//        public float dt;
//
//        public void Execute(ref Circle circle, [ReadOnly] ref MoveSettings moveData, ref Translation position) {
//            // circle.angle += dt * moveData.Speed;
//            if (moveData.speed > 0) {
//                circle.angle.z += dt * math.degrees(moveData.speed) * (int) circle.Clockwise;
//                quaternion q = quaternion.Euler(math.radians(circle.angle));
//                position.Value = circle.pivot + math.mul(q, new float3(1, 0, 0) * -circle.radius);
//            }
//        }
//    }

// 
//    [BurstCompile]
//    [ExcludeComponent(typeof(PathMovementTag), typeof(FearDebuff))]
//    private struct MoveCurveOfEightJob : IJobForEach<FlatCurveRose, MoveSettings, Translation> {
//        public float dt;
//
//        public void Execute(ref FlatCurveRose flatCurve, [ReadOnly] ref MoveSettings moveData,
//            ref Translation position) {
//            //circle.angle += dt * moveData.Speed;
//            //var delta = math.radians( 4 * t *moveData.Speed);
//            //           var delta =  2 * t ;
//            //           var r =  math.sin(delta);
//            //
//            //           var x = r * math.cos(delta);
//            //           var y = r * math.sin(delta);
//            //           var nPos = new float3(x, y, 0);
//
//            if (moveData.speed > 0) {
//                flatCurve.t += dt;
//
//                var tt = flatCurve.t * moveData.speed;
//                var r = flatCurve.radius * math.sin((2f / 4f) * tt);
//                var x = r * math.cos(tt);
//                var y = r * math.sin(tt);
//
//                var scale = 2 / (3 - math.cos(2 * tt));
//                //            var x     = scale * math.cos(tt);
//                //            var y     = scale * math.sin(2 * tt) / 2;
//
//                var nPos = new float3(x, y, 0);
//                position.Value = flatCurve.pivot + nPos;
//            }
//        }
//    }

    [BurstCompile]
    //  [RequireComponentTag(typeof(CellMovementTag))]
    private struct MoveTowardsJobChunk : IJobChunk {
        public float dt;

        [ReadOnly] public ArchetypeChunkComponentType<MoveSettings> MoveSettingsArchetypeChunkType;
        public ArchetypeChunkComponentType<Translation> TranslationArchetypeChunkType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var moveDataChunk = chunk.GetNativeArray(MoveSettingsArchetypeChunkType);
            var translationChunk = chunk.GetNativeArray(TranslationArchetypeChunkType);
            for (int i = 0; i < chunk.Count; i++) {
                var position = translationChunk[i];
                var moveSettings = moveDataChunk[i];
                position.Value =
                    PhysicsUtils.MoveTowards(position.Value, moveSettings.targetCellPos, moveSettings.speed * dt);
                translationChunk[i] = position;
            }
        }
    }

//     [BurstCompile]
//     [ExcludeComponent(typeof(PathMovementTag), typeof(FearDebuff), typeof(FlatCurveRose),
//         typeof(AfterFearDebuffPathMoveTag))]
//     private struct MoveCircleJobChunk : IJobChunk {
//         public float dt;
//         public ArchetypeChunkComponentType<Circle> CircleArchetypeChunkType;
//         [ReadOnly] public ArchetypeChunkComponentType<MoveSettings> MoveSettingsArchetypeChunkType;
//         public ArchetypeChunkComponentType<Translation> TranslationArchetypeChunkType;
//
//         public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
//             // circle.angle += dt * moveData.Speed;
//             var circleCurveRoseChunk = chunk.GetNativeArray(CircleArchetypeChunkType);
//             var moveDataChunk = chunk.GetNativeArray(MoveSettingsArchetypeChunkType);
//             var translationChunk = chunk.GetNativeArray(TranslationArchetypeChunkType);
//             for (int i = 0; i < chunk.Count; i++) {
//                 var moveData = moveDataChunk[i];
//                 if (moveData.speed > 0) {
//                     var circle = circleCurveRoseChunk[i];
//                     var position = translationChunk[i];
//                     circle.angle.z += dt * math.degrees(moveData.speed) * (int) circle.Clockwise;
//                     circleCurveRoseChunk[i] = circle;
//                     quaternion q = quaternion.Euler(math.radians(circle.angle));
//                     position.Value = circle.pivot + math.mul(q, new float3(1, 0, 0) * -circle.radius);
//                     translationChunk[i] = position;
//                 }
//             }
//         }
//     }
//
//     [BurstCompile]
//     [ExcludeComponent(typeof(PathMovementTag), typeof(FearDebuff))]
//     private struct MoveCurveOfEightJobChunk : IJobChunk {
//         public float dt;
//
//         public ArchetypeChunkComponentType<FlatCurveRose> FlatCurveRoseArchetypeChunkType;
//         [ReadOnly] public ArchetypeChunkComponentType<MoveSettings> MoveSettingsArchetypeChunkType;
//         public ArchetypeChunkComponentType<Translation> TranslationArchetypeChunkType;
//
//         public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
//             var flatCurveRoseChunk = chunk.GetNativeArray(FlatCurveRoseArchetypeChunkType);
//             var moveDataChunk = chunk.GetNativeArray(MoveSettingsArchetypeChunkType);
//             var translationChunk = chunk.GetNativeArray(TranslationArchetypeChunkType);
//             //circle.angle += dt * moveData.Speed;
//             //var delta = math.radians( 4 * t * moveData.Speed);
// //           var delta =  2 * t ;
// //           var r =  math.sin(delta);
// //
// //           var x = r * math.cos(delta);
// //           var y = r * math.sin(delta);
// //           var nPos = new float3(x, y, 0);
//
//             for (int i = 0; i < chunk.Count; i++) {
//                 var moveData = moveDataChunk[i];
//                 if (moveData.speed > 0) {
//                     var flatCurve = flatCurveRoseChunk[i];
//                     flatCurve.t += dt;
//                     flatCurveRoseChunk[i] = flatCurve;
//                     var tt = flatCurve.t * moveData.speed;
//                     var r = flatCurve.radius * math.sin((2f / 4f) * tt);
//                     var x = r * math.cos(tt);
//                     var y = r * math.sin(tt);
//
//                     var scale = 2 / (3 - math.cos(2 * tt));
// //            var x     = scale * math.cos(tt);
// //            var y     = scale * math.sin(2 * tt) / 2;
//
//                     var nPos = new float3(x, y, 0);
//                     var position = translationChunk[i];
//                     position.Value = flatCurve.pivot + nPos;
//                     translationChunk[i] = position;
//                 }
//             }
//         }
//     }

    protected override void OnUpdate() {
        var dt = Time.DeltaTime;
        // var flatCurveRoseArchetypeChunkType = GetArchetypeChunkComponentType<FlatCurveRose>();
        // var moveSettingsArchetypeChunkType = GetArchetypeChunkComponentType<MoveSettings>(true);
        // var translationArchetypeChunkType = GetArchetypeChunkComponentType<Translation>();
        // var circleArchetypeChunkType = GetArchetypeChunkComponentType<Circle>();

        // var moveTowardsJob = new MoveTowardsJobChunk() {
        //     dt = dt,
        //     MoveSettingsArchetypeChunkType = moveSettingsArchetypeChunkType,
        //     TranslationArchetypeChunkType = translationArchetypeChunkType,
        // }.Schedule(moveGroup, Dependency);

        //var moveTowardsJobHandle = 
        Entities.WithName("MoveTowardsJob")
            .WithBurst()
            .ForEach((Entity entity, ref MoveSettings moveSettings, ref Translation position) => {
                position.Value =
                    PhysicsUtils.MoveTowards(position.Value, moveSettings.targetCellPos, moveSettings.speed * dt);
            }).ScheduleParallel();
        // Dependency = moveTowardsJobHandle;

        // var moveCircleJob = new MoveCircleJobChunk() {
        //     dt = dt,
        //     CircleArchetypeChunkType = circleArchetypeChunkType,
        //     MoveSettingsArchetypeChunkType = moveSettingsArchetypeChunkType,
        //     TranslationArchetypeChunkType = translationArchetypeChunkType,
        // }.Schedule(m_circle_group, Dependency);

        //var moveCircleJobHandle = 
        Entities.WithName("MoveCircleJob")
            //.WithAll<>()
            .WithBurst()
            .WithNone<PathMovementTag, FearDebuff, AfterFearDebuffPathMoveTag>()
            .ForEach((ref MoveSettings moveData, ref Translation position, ref Heading heading, ref Circle circle) => {
                if (moveData.speed > 0) {
                    circle.angle.z += dt * math.degrees(moveData.speed) * (int) circle.Clockwise;
                    quaternion q = quaternion.Euler(math.radians(circle.angle));
                    position.Value = circle.pivot + math.mul(q, new float3(1, 0, 0) * -circle.radius);
                }
            }).ScheduleParallel();
        //Dependency = moveCircleJobHandle;

        // var moveCurveEightJob = new MoveCurveOfEightJobChunk() {
        //     dt = dt,
        //     FlatCurveRoseArchetypeChunkType = flatCurveRoseArchetypeChunkType,
        //     MoveSettingsArchetypeChunkType = moveSettingsArchetypeChunkType,
        //     TranslationArchetypeChunkType = translationArchetypeChunkType
        // }.Schedule(m_FCR_group, Dependency);

        // var moveCurveEightJobHandle = 
        Entities.WithName("MoveCurveOfEightJob")
            //.WithAll<>()
            .WithBurst()
            .WithNone<PathMovementTag, FearDebuff, AfterFearDebuffPathMoveTag>()
            .ForEach((ref MoveSettings moveData, ref Translation position, ref Heading heading,
                ref FlatCurveRose flatCurve) => {
                if (moveData.speed > 0) {
                    flatCurve.t += dt;
                    var tt = flatCurve.t * moveData.speed;
                    var r = flatCurve.radius * math.sin((2f / 4f) * tt);
                    var x = r * math.cos(tt);
                    var y = r * math.sin(tt);
                    var scale = 2 / (3 - math.cos(2 * tt));
//            var x     = scale * math.cos(tt);
//            var y     = scale * math.sin(2 * tt) / 2;
                    var nPos = new float3(x, y, 0);
                    position.Value = flatCurve.pivot + nPos;
                }
            }).ScheduleParallel();
        //Dependency = moveCurveEightJobHandle;
    }
}