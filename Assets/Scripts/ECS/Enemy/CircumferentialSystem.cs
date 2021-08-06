using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PreSimulationSystemGroup))]
public class CircumferentialSystem : SystemBase {
    // private EntityQuery circleGroup;

    protected override void OnCreate() {
        // circleGroup = GetEntityQuery(new EntityQueryDesc() {
        //     All = new[] {
        //         ComponentType.ReadWrite<Translation>(),
        //         ComponentType.ReadWrite<Rotation>(),
        //     },
        //     Any = new[] {
        //         ComponentType.ReadWrite<Circle>(),
        //         ComponentType.ReadWrite<FlatCurveRose>()
        //     },
        //     None = new[] {
        //         ComponentType.ReadWrite<FearDebuff>(),
        //         ComponentType.ReadOnly<AfterFearDebuffPathMoveTag>(),
        //     },
        // });
        // RequireForUpdate(circleGroup);
    }

    // is obsolete
//    [BurstCompile]
//    [ExcludeComponent(typeof(FearDebuff), typeof(AfterFearDebuffPathMoveTag))]
//    private struct CircleMoveJob : IJobForEach<Circle, DirectionsSight, Translation, Rotation> {
//        public void Execute(ref Circle circle, ref DirectionsSight heading, ref Translation position,
//            ref Rotation rot) {
//            var vectorDitToCenter = circle.pivot - position.Value;
//            heading.direction = new float2(vectorDitToCenter.x, vectorDitToCenter.y);
//        }
//    }
//
//    [BurstCompile]
//    [ExcludeComponent(typeof(FearDebuff), typeof(AfterFearDebuffPathMoveTag))]
//    private struct CircumferentialMoveJob : IJobForEach<FlatCurveRose, DirectionsSight, Translation, Rotation> {
//        public void Execute(ref FlatCurveRose circle, ref DirectionsSight heading, ref Translation position,
//            ref Rotation rot) {
//            var vectorDitToCenter = circle.pivot - position.Value;
//            heading.direction = new float2(vectorDitToCenter.x, vectorDitToCenter.y);
//        }
//    }

    protected override void OnUpdate() {
        // TODO combine next two jobs because they have the same logic (use WithAny)
        // Calculating Heading for FearDebuffJob
        // var circumferentialMoveJob = new CircumferentialMoveJob().Schedule(this, inputDeps);
        // var circumferentialMoveJobHandle =
        Entities.WithName("CircumferentialMoveJob")
            .WithNone<FearDebuff, AfterFearDebuffPathMoveTag>()
            .WithBurst()
            .ForEach((ref DirectionsSight heading, in FlatCurveRose circle, in Translation position
            ) => {
                var vectorDitToCenter = circle.pivot - position.Value;
                heading.VectorDirection = new float2(vectorDitToCenter.x, vectorDitToCenter.y);
            }).ScheduleParallel();

        // var circleMoveJob = new CircleMoveJob().Schedule(this, circumferentialMoveJob);
        // var CircleMoveJobHandle =
        Entities.WithName("CircleMoveJob")
            .WithNone<FearDebuff, AfterFearDebuffPathMoveTag>()
            .WithBurst()
            .ForEach((ref DirectionsSight heading, in Circle circle, in Translation position) => {
                var vectorDirToCenter = circle.pivot - position.Value;
                heading.VectorDirection = new float2(vectorDirToCenter.x, vectorDirToCenter.y);
            }).ScheduleParallel();
    }
}