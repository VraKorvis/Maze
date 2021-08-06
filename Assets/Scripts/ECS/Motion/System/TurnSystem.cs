using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public class TurnSystem : SystemBase {
    private EntityQuery worldDirGroup;
    private EntityQuery turnTagGroup;
    private EntityQuery playerGroup;

    protected override void OnCreate() {
        worldDirGroup = GetEntityQuery(ComponentType.ReadWrite<WorldTurn>());

        turnTagGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadWrite<Rotation>(),
            },
            Options = EntityQueryOptions.FilterWriteGroup,
        });
        playerGroup = GetEntityQuery(ComponentType.ReadOnly<PlayerData>());
        RequireSingletonForUpdate<ClickToTurn>();
        RequireSingletonForUpdate<PlayerData>();
    }

    [BurstCompile]
    [RequireComponentTag(typeof(RotateTag))]
    [ExcludeComponent(typeof(TurnOffTag))]
    private struct RotateJobChunk : IJobChunk {
        public float dt;
        public ArchetypeChunkComponentType<Rotation> RotationArchetype;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var rotationChunk = chunk.GetNativeArray(RotationArchetype);
            for (int i = 0; i < chunk.Count; i++) {
                var rotation = rotationChunk[i];
                rotation.Value = math.mul(math.normalize(rotation.Value),
                    quaternion.AxisAngle(math.forward(quaternion.identity), 3.0f * dt));
                rotationChunk[i] = rotation;
            }
        }
    }

    // job is obsolete
//    [ExcludeComponent(typeof(TurnTag))]
//    private struct CalculateAngleJob : IJobForEachWithEntity<WorldTurn, ClickToTurn, Rotation, LocalToWorld> {
//        public EntityCommandBuffer.Concurrent CommandBuffer;
//
//        public void Execute(Entity entity, int index, ref WorldTurn worldTurn,
//            [ChangedFilter] [ReadOnly] ref ClickToTurn click,
//            ref Rotation rot,
//            ref LocalToWorld localToWorld) {
//            float3 newDirect = math.round(math.mul(quaternion.Euler(0, 0, math.radians(90 * (int) click.Clockwise)),
//                new float3(worldTurn.direction.x, worldTurn.direction.y, 0)));
//
//            worldTurn.startAngle = rot.Value;
//            worldTurn.direction = new int2((int) newDirect.x, (int) newDirect.y);
//            worldTurn.goalAngle =
//                math.mul(FromToRot90(localToWorld.Up, new float3(worldTurn.direction.x, worldTurn.direction.y, 0)),
//                    rot.Value);
//            worldTurn.timeCount = 0;
//            worldTurn.isSpinningNow = true;
//            CommandBuffer.AddComponent(index, entity, new TurnTag());
//        }
//    }

    // TODO rewrite method
    // y/x = sin(A) / cos(A) = tan(A)   
    // V.x = cos(A)
    // V.y = sin(A)
    // A = atan2(V.y, V.x)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static quaternion FromToRot90(float3 fromDirection, float3 toDirection) {
        quaternion quaternion = Quaternion.FromToRotation(fromDirection, toDirection);
        return quaternion;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static quaternion FromToRot(float3 v1, float3 v2) {
//            quaternion q = quaternion.identity;
//            var lengthV1 = math.lengthsq(v1);
//            var lengthV2 = math.lengthsq(v2);
//            float3 a = math.cross(v1, v2);
//            q.value.xyz = a;
//            q.value.w   = math.sqrt((lengthV1*lengthV1) * ( lengthV2*lengthV2)) + math.dot(v1, v2);

        var lengthV1 = math.length(v1);
        var lengthV2 = math.length(v2);
        float k_cos_theta = math.dot(v1, v2);
        float k = math.sqrt(lengthV1 * lengthV1 * lengthV2 * lengthV2);

        quaternion q = quaternion.identity;
        float4 f4 = q.value;
        if (k_cos_theta / k == -1) {
            // 180 degree rotation around any orthogonal vector
            f4.w = 0;
            f4.xyz = math.cross(v1, v2);
            q.value = f4;

            return q;
        }

        f4.w = k_cos_theta + k;
        f4.xyz = math.cross(v1, v2);
        q.value = f4;
        return q;
    }

    // job is obsolete
//    [RequireComponentTag(typeof(PlayerData))]
//    [ExcludeComponent(typeof(TurnTag))]
//    private struct PlayerDirectionVectorJob : IJobForEachWithEntity<Heading> {
//        [ReadOnly] public int2 dir;
//
//        public EntityCommandBuffer.Concurrent CommandBuffer;
//
//        public void Execute(Entity entity, int index, ref Heading heading) {
//            heading.VectorDirection = new int2(dir.x, dir.y);
//            CommandBuffer.AddComponent(index, entity, new TurnTag());
//        }
//    }

    [RequireComponentTag(typeof(TurnTag))]
    private struct SettingsTurnOffJobChunk : IJobChunk {
        public float dt;
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public ArchetypeChunkComponentType<ClickToTurn> ClickToTurnArchetype;
        public ArchetypeChunkComponentType<WorldTurn> WorldTurnArchetype;
        public ArchetypeChunkEntityType EntitiesArchetype;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var worldTurnChunk = chunk.GetNativeArray(WorldTurnArchetype);
            var ClickToTurnChunk = chunk.GetNativeArray(ClickToTurnArchetype);
            var entitiesChunk = chunk.GetNativeArray(EntitiesArchetype);
            for (int i = 0; i < chunk.Count; i++) {
                var worldTurn = worldTurnChunk[i];
                var entity = entitiesChunk[i];
                var clickToTurn = ClickToTurnChunk[i];
                if (worldTurn.timeCount < 1f) {
                    //TODO Dampen
                    float smooth1 = math.smoothstep(0, 1f, worldTurn.timeCount);
                    float smooth2 = math.smoothstep(0, 1f, smooth1);
                    worldTurn.quaternion = math.slerp(worldTurn.startAngle, worldTurn.goalAngle, worldTurn.timeCount);
                    worldTurn.timeCount += dt * worldTurn.angularVelocity;
                    worldTurn.isSpinningNow = true;
                }
                else {
                    worldTurn.quaternion = worldTurn.goalAngle;
                    worldTurn.isSpinningNow = false;
                    worldTurn.timeCount = 0;
                    // TODO need check chunkIndex might not work
                    CommandBuffer.RemoveComponent<ClickToTurn>(chunkIndex, entity);
                }

                worldTurnChunk[i] = worldTurn;
            }
        }
    }

    [BurstCompile]
    [RequireComponentTag(typeof(TurnTag))]
    [ExcludeComponent(typeof(TurnOffTag))]
    private struct TurnOffJobChunk : IJobChunk {
        [ReadOnly] public WorldTurn WorldTurn;

        public ArchetypeChunkComponentType<Rotation> RotationArchetype;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var rotationChunk = chunk.GetNativeArray(RotationArchetype);
            for (int i = 0; i < chunk.Count; i++) {
                var rotation = rotationChunk[i];
                rotation.Value = WorldTurn.quaternion;
                rotationChunk[i] = rotation;
            }
        }
    }

    protected override void OnUpdate() {
        var dt = Time.DeltaTime;
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var worldTurnOffEntity = worldDirGroup.GetSingletonEntity();
        var isSpinningNowWorld = em.GetComponentData<WorldTurn>(worldTurnOffEntity).isSpinningNow;
        var playerEntity = playerGroup.GetSingletonEntity();

        if (!isSpinningNowWorld) {
            em.RemoveComponent<TurnTag>(worldTurnOffEntity);
        }

        // If use this System inside another system then need use PostUpdateCommandBuffer.RemoveComponent instead EntityManager.RemoveComponent
        em.RemoveComponent<TurnTag>(playerEntity);

        // calculate goal angle
        //var ecb = new EntityCommandBuffer(Allocator.TempJob);
//        var calculateAngleJob = new CalculateAngleJob() {
//            CommandBuffer = ecb.ToConcurrent(),
//        };
//        var calculateHandler = calculateAngleJob.Schedule(this, inputDeps);
//        calculateHandler.Complete();

        Entities.WithName("Calculate_Angle")
            .WithNone<TurnTag>()
            .WithoutBurst()
            .WithStructuralChanges()
            .ForEach((Entity entity, ref WorldTurn worldTurn,
                ref ClickToTurn click, ref Rotation rot, ref LocalToWorld localToWorld) => {
                //  Debug.Log("Before_Calculate_Angle:  " + worldTurn.goalAngle);
                float3 newDirect = math.round(math.mul(quaternion.Euler(0, 0, math.radians(90 * (int) click.Clockwise)),
                    new float3(worldTurn.direction.x, worldTurn.direction.y, 0)));

                worldTurn.startAngle = rot.Value;
                worldTurn.direction = new int2((int) newDirect.x, (int) newDirect.y);
                worldTurn.goalAngle =
                    math.mul(FromToRot90(localToWorld.Up, new float3(worldTurn.direction.x, worldTurn.direction.y, 0)),
                        rot.Value);
                worldTurn.timeCount = 0;
                worldTurn.isSpinningNow = true;
                em.AddComponent<TurnTag>(entity);
                // Debug.Log("Calculate_Angle:  " + worldTurn.goalAngle);
            }).Run();

//        Entities.WithName("Debug_Angle")
//            .WithoutBurst()
//            .ForEach((ref WorldTurn worldTurn) => {
//               // Debug.Log(eulerXYZ);
//                Debug.Log("startAngle: " + worldTurn.goalAngle + " goalAngle: " + worldTurn.goalAngle);
//            }).Run();

        var worldTurnOff = em.GetComponentData<WorldTurn>(worldTurnOffEntity);

        if (worldTurnOff.isSpinningNow) {
            em.AddComponent<TurnTag>(worldTurnOffEntity);
        }

//        var playerDirection = new PlayerDirectionVectorJob() {
//            dir = worldTurnOff.direction,
//            CommandBuffer = ecb.ToConcurrent(),
//        }.Schedule(this, inputDeps);
//        playerDirection.Complete();

        var dir = worldTurnOff.direction;
        Entities.WithName("Player_direction")
            .WithAll<PlayerData>()
            .WithNone<TurnTag>()
            .WithStructuralChanges()
            .WithoutBurst()
            .ForEach((Entity entity, ref Heading heading) => {
                //  Debug.Log("Player_Direction_Before: " + heading.VectorDirection);
                heading.VectorDirection = new int2(dir.x, dir.y);
                //  Debug.Log("Player_Direction_After: " + heading.VectorDirection);
                em.AddComponent<TurnTag>(entity);
            }).Run();

        var needCalculateAngle = em.GetComponentData<ClickToTurn>(worldTurnOffEntity).needCalculateAngle;
//        var worldTurnArchetype = GetArchetypeChunkComponentType<WorldTurn>();
//        var clickToTurnArchetype = GetArchetypeChunkComponentType<ClickToTurn>();
//        var entities = GetArchetypeChunkEntityType();
        if (needCalculateAngle) {
//            var settingsTurnOffJob = new SettingsTurnOffJobChunk() {
//                ClickToTurnArchetype = clickToTurnArchetype,
//                WorldTurnArchetype = worldTurnArchetype,
//                EntitiesArchetype = entities,
//                dt = dt,
//                CommandBuffer = ecb.ToConcurrent(),
//            };
//
//            var settingsTurnOffHandle = settingsTurnOffJob.Schedule(this, inputDeps);
//            settingsTurnOffHandle.Complete();
            Entities
                .WithName("Settings_TurnOff_Job")
                .WithAll<TurnTag>()
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((Entity entity, ref WorldTurn worldTurn, ref ClickToTurn clickToTurn) => {
                    if (worldTurn.timeCount < 1f) {
                        //TODO Dampen
                        float smooth1 = math.smoothstep(0, 1f, worldTurn.timeCount);
                        float smooth2 = math.smoothstep(0, 1f, smooth1);
                        worldTurn.quaternion =
                            math.slerp(worldTurn.startAngle, worldTurn.goalAngle,
                                smooth2); // use smooth2 for smooth or worldTurn.timeCount
                        worldTurn.timeCount += dt * worldTurn.angularVelocity;
                        worldTurn.isSpinningNow = true;
                    }
                    else {
                        worldTurn.quaternion = worldTurn.goalAngle;
                        worldTurn.isSpinningNow = false;
                        worldTurn.timeCount = 0;
                        em.RemoveComponent<ClickToTurn>(entity);
                    }

                    //Debug.Log("Settings_TurnOff_Job:  " + worldTurn.goalAngle);
                }).Run();
        }

        var worldTurnOffEntity_2 = worldDirGroup.GetSingletonEntity();
        var worldTurnOff2 = em.GetComponentData<WorldTurn>(worldTurnOffEntity_2);
        var rotationArchetype = GetArchetypeChunkComponentType<Rotation>();

//        Debug.Log("World_Quaternion_BeforeApply: " + worldTurnOff2.quaternion);
        var turnJobHandle = new TurnOffJobChunk() {
            RotationArchetype = rotationArchetype,
            WorldTurn = worldTurnOff2
        }.Schedule(turnTagGroup);
        Dependency = turnJobHandle;
        // turnJob.Complete();

        // !!!!! no needed for this job. Wrong Angle
//        var rotateJob = new RotateJobChunk() {
//            RotationArchetype = rotationArchetype,
//            dt = dt,
//        }.Schedule(turnTagGroup, turnHandle);
//        rotateJob.Complete();
//        Entities.WithName("Debug_Angle2")
//            .WithoutBurst()
//            .ForEach((ref WorldTurn worldTurn) => {
//                // Debug.Log(eulerXYZ);
//                Debug.Log("startAngle: " + worldTurn.goalAngle + " goalAngle: " + worldTurn.goalAngle);
//            }).Run();
    }
}