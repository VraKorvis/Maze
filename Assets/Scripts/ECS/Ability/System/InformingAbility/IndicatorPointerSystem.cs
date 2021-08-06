using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(AfterSimulationSystemGroup))]
public class IndicatorPointerSystem : SystemBase {
    private EntityQuery m_indicatorGroup;

    EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    protected override void OnCreate() {
        m_indicatorGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadOnly<IndicatorPointer>(),
                ComponentType.ReadOnly<RenderMesh>(),
                ComponentType.ReadOnly<Scale>(),
            },
            Options = EntityQueryOptions.FilterWriteGroup,
        });
        RequireForUpdate(m_indicatorGroup);
        m_EndSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

// is obsolete
//    [BurstCompile]
//    private struct CalculateIconsPositionJob : IJobForEachWithEntity<IndicatorPointer, Scale> {
//        public float3 camPos;
//        public float4x4 projectionMatrix;
//        public float3 camUp;
//        public float3 camRight;
//        public float3 camForward;
//        public float pixelWidth;
//        public float pixelHeight;
//        public float scaleFactor;
//
//        public Translation playerPos;
//        public float3 playerOffsetPos;
//        public float offsetY;
//
//        public float width;
//        public float height;
//
//        [NativeDisableParallelForRestriction] [ReadOnly]
//        public ComponentDataFromEntity<Translation> spiritsPos;
//
//        public float3 screenCenter;
//        public quaternion inverseWorldQuaternion;
//
//        public void Execute(Entity entity, int index, ref IndicatorPointer indicator, ref Scale scale) {
//            var targetEntity = indicator.targetEntity;
//
//            var targetPos = spiritsPos[targetEntity];
//
//            var direction = targetPos.Value - playerPos.Value;
//            var distanceToTarget = math.length(direction);
//
//            // TODO This bug ("Screen position out of view frustum") appears if target pos = player pos, later this case never happen, its just temporary measures
//            if (distanceToTarget < 1f) {
//                scale.Value = 0;
//                return;
//            }
//
//            float3 indicatorScreenPos = new float3(0, 0, 0);
//            direction = math.normalizesafe(direction);
//            direction = math.mul(inverseWorldQuaternion, direction);
//            var angle = math.atan2(direction.x, direction.y);
//            var cos = math.cos(angle);
//
//            // y = mx+b; x = y/m
//            float m = direction.y / direction.x;
//            if (cos > 0) {
//                //topside
//                indicatorScreenPos.y = screenCenter.y + offsetY;
//                indicatorScreenPos.x = indicatorScreenPos.y / m;
//            }
//            else {
//                //downside
//                indicatorScreenPos.y = -screenCenter.y + offsetY;
//                indicatorScreenPos.x = indicatorScreenPos.y / m;
//            }
//
//            if (indicatorScreenPos.x > screenCenter.x) {
//                // rightSide
//                indicatorScreenPos.x = screenCenter.x;
//                indicatorScreenPos.y = indicatorScreenPos.x * m;
//            }
//            else if (indicatorScreenPos.x < -screenCenter.x) {
//                //left
//                indicatorScreenPos.x = -screenCenter.x;
//                indicatorScreenPos.y = indicatorScreenPos.x * m;
//            }
//
//            indicatorScreenPos.x += playerOffsetPos.x;
//            indicatorScreenPos.y += playerOffsetPos.y;
//            indicator.screenPoint = indicatorScreenPos;
//
//            float2 targetScreen = CamUtil.ConvertWorldToScreenCoordinates(targetPos.Value, camPos, projectionMatrix,
//                camUp, camRight, camForward,
//                pixelWidth, pixelHeight, scaleFactor);
//
//            if (distanceToTarget > indicator.radius || IsOnScreen(targetScreen)) {
//                scale.Value = 0;
//            }
//            else {
//                var scaleVal = math.clamp(indicator.radius / distanceToTarget - 1f, 0.3f, 2f);
//                scale.Value = scaleVal;
//            }
//        }
//
//        private bool IsOnScreen(float2 offScreenPos) {
//            if (offScreenPos.y > 0 && offScreenPos.y < height && offScreenPos.x < width && offScreenPos.x > 0) {
//                return true;
//            }
//
//            return false;
//        }
//    }

    //is obsolete
//    private struct ClearIfNotExistIndicatorPointer : IJobForEachWithEntity<IndicatorPointer> {
//        public float dt;
//
//        [NativeDisableParallelForRestriction] [ReadOnly]
//        public ComponentDataFromEntity<Translation> translation;
//
//        public EntityCommandBuffer.Concurrent commandBuffer;
//
//        public void Execute(Entity entity, int index, ref IndicatorPointer indicator) {
//            var targetEntity = indicator.entity;
//            indicator.duration -= dt;
//            if (!translation.Exists(targetEntity) || indicator.duration < 0) {
//                commandBuffer.DestroyEntity(index, entity);
//            }
//        }
//    }

    private static bool IsOnScreen(float2 offScreenPos, float height, float width) {
        if (offScreenPos.y > 0 && offScreenPos.y < height && offScreenPos.x < width && offScreenPos.x > 0) {
            return true;
        }

        return false;
    }

    protected override void OnUpdate() {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var player = GetSingletonEntity<PlayerData>();
        var playerPos = em.GetComponentData<Translation>(player);
        var worldTurn = GetSingletonEntity<WorldTurn>();
        var worldQuaternion = EntityManager.GetComponentData<WorldTurn>(worldTurn).quaternion;

        var cam = Camera.main;
        var width = Screen.width;
        var height = Screen.height;
        var screenCenter = new float3(width / 2f, height / 2f, 0);
        var playerOffsetPos = cam.WorldToScreenPoint(playerPos.Value);
        var offset = screenCenter.y - playerOffsetPos.y;

        var transform = cam.transform;
        float3 camPos = transform.position;
        float4x4 projectionMatrix = cam.projectionMatrix;
        float3 camUp = transform.up;
        float3 camRight = transform.right;
        float3 camForward = transform.forward;
        float pixelWidth = cam.pixelWidth;
        float pixelHeight = cam.pixelHeight;
        float scaleFactor = 1;

//        var clearIfNotExistIndicatorPointerJob = new ClearIfNotExistIndicatorPointer() {
//            dt = Time.DeltaTime,
//            translation = GetComponentDataFromEntity<Translation>(),
//            commandBuffer = ecb.ToConcurrent(),
//        };
//        var clearIfNotExistIndicatorPointerJobHandle = clearIfNotExistIndicatorPointerJob.Schedule(indicatorGroup);
        // clearIfNotExistIndicatorPointerJob.Complete();

        var dt = Time.DeltaTime;
        var translation = GetComponentDataFromEntity<Translation>(true);
        var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();
        var clearIfNotExistIndicatorPointerJobHandle =
            Entities.WithName("clearIfNotExistIndicatorPointer")
                .WithoutBurst()
                // .WithStructuralChanges()
                .WithReadOnly(translation)
                .ForEach((Entity entity, int entityInQueryIndex, ref IndicatorPointer indicator) => {
                    var targetEntity = indicator.targetEntity;
                    indicator.duration -= dt;
                    if (!translation.Exists(targetEntity) || indicator.duration < 0) {
                        ecb.DestroyEntity(entityInQueryIndex, entity);
                    }
                }).ScheduleParallel(Dependency);
        m_EndSimulationEcbSystem.AddJobHandleForProducer(clearIfNotExistIndicatorPointerJobHandle);
        Dependency = clearIfNotExistIndicatorPointerJobHandle;

//        var calculateIconsPosition = new CalculateIconsPositionJob() {
//            camPos = camPos,
//            projectionMatrix = projectionMatrix,
//            camUp = camUp,
//            camRight = camRight,
//            camForward = camForward,
//            pixelWidth = pixelWidth,
//            pixelHeight = pixelHeight,
//            scaleFactor = scaleFactor,
//
//            playerPos = playerPos,
//            playerOffsetPos = playerOffsetPos,
//            offsetY = offset,
//            width = width,
//            height = height,
//            spiritsPos = GetComponentDataFromEntity<Translation>(),
//            inverseWorldQuaternion = math.inverse(worldQuaternion),
//            screenCenter = screenCenter,
//        };

        var spiritsPos = GetComponentDataFromEntity<Translation>();
        var inverseWorldQuaternion = math.inverse(worldQuaternion);
        var offsetY = offset;
        // var calculateIconsPositionJobHandle =
        Entities.WithName("CalculateIconsPositionJob")
            .WithBurst()
            .WithReadOnly(spiritsPos)
            .ForEach((ref IndicatorPointer indicator, ref Scale scale) => {
                var targetEntity = indicator.targetEntity;

                var targetPos = spiritsPos[targetEntity];

                var direction = targetPos.Value - playerPos.Value;
                var distanceToTarget = math.length(direction);

                // TODO This bug ("Screen position out of view frustum") appears if target pos = player pos, later this case never happen, its just temporary measures
                if (distanceToTarget < 1f) {
                    scale.Value = 0;
                    return;
                }

                float3 indicatorScreenPos = new float3(0, 0, 0);
                direction = math.normalizesafe(direction);
                direction = math.mul(inverseWorldQuaternion, direction);
                var angle = math.atan2(direction.x, direction.y);
                var cos = math.cos(angle);

                // y = mx+b; x = y/m
                float m = direction.y / direction.x;
                if (cos > 0) {
                    //topside
                    indicatorScreenPos.y = screenCenter.y + offsetY;
                    indicatorScreenPos.x = indicatorScreenPos.y / m;
                }
                else {
                    //downside
                    indicatorScreenPos.y = -screenCenter.y + offsetY;
                    indicatorScreenPos.x = indicatorScreenPos.y / m;
                }

                if (indicatorScreenPos.x > screenCenter.x) {
                    // rightSide
                    indicatorScreenPos.x = screenCenter.x;
                    indicatorScreenPos.y = indicatorScreenPos.x * m;
                }
                else if (indicatorScreenPos.x < -screenCenter.x) {
                    //left
                    indicatorScreenPos.x = -screenCenter.x;
                    indicatorScreenPos.y = indicatorScreenPos.x * m;
                }

                indicatorScreenPos.x += playerOffsetPos.x;
                indicatorScreenPos.y += playerOffsetPos.y;
                indicator.screenPoint = indicatorScreenPos;

                float2 targetScreen = CamUtil.ConvertWorldToScreenCoordinates(targetPos.Value, camPos,
                    projectionMatrix,
                    camUp, camRight, camForward,
                    pixelWidth, pixelHeight, scaleFactor);

                if (distanceToTarget > indicator.radius || IsOnScreen(targetScreen, height, width)) {
                    scale.Value = 0;
                }
                else {
                    var scaleVal = math.clamp(indicator.radius / distanceToTarget, 0.1f, 3f);
                    scale.Value = scaleVal;
                }
            }).ScheduleParallel(Dependency);
        // m_EndSimulationEcbSystem.AddJobHandleForProducer(calculateIconsPositionJobHandle);
        // Dependency = calculateIconsPositionJobHandle;
    }
}