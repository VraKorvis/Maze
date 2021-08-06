using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

//[UpdateInGroup(typeof(LateSimulationSystemGroup))]
//public class TurnSystemV2 : SystemBase {
//    
//    private EntityQuery worldDirGroup;
//    private EntityQuery playerGroup;
//    private EntityQuery turnTagGroup;
//   
//    protected override void OnCreate() {
//        worldDirGroup = GetEntityQuery(ComponentType.ReadWrite<WorldTurn>());
//        playerGroup = GetEntityQuery(ComponentType.ReadOnly<PlayerData>());
//        turnTagGroup = GetEntityQuery(new EntityQueryDesc() {
//            All = new[] {
//                ComponentType.ReadWrite<Rotation>(),
//            },
//            Options = EntityQueryOptions.FilterWriteGroup,
//        });
//    }
//
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private quaternion FromToRot90(float3 fromDirection, float3 toDirection) {
//        quaternion quaternion = Quaternion.FromToRotation(fromDirection, toDirection);
//        return quaternion;
//    }
//    
//    [BurstCompile]
//    [RequireComponentTag(typeof(TurnTag))]
//    [ExcludeComponent(typeof(TurnOffTag))]
//    private struct TurnOffJobChunk : IJobChunk {
//        //ForEach<Rotation> {
//        [ReadOnly] public WorldTurn WorldTurn;
//
//        public ArchetypeChunkComponentType<Rotation> RotationArchetype;
//
//        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
//            var rotationChunk = chunk.GetNativeArray(RotationArchetype);
//            for (int i = 0; i < chunk.Count; i++) {
//                var rotation = rotationChunk[i];
//                rotation.Value = WorldTurn.quaternion;
//                rotationChunk[i] = rotation;
//            }
//        }
//    }
//    
//    [BurstCompile]
//    [RequireComponentTag(typeof(RotateTag))]
//    [ExcludeComponent(typeof(TurnOffTag))]
//    private struct RotateJobChunk : IJobChunk {
//        // ForEach<Rotation> {
//        public float dt;
//
//        public ArchetypeChunkComponentType<Rotation> RotationArchetype;
//
//        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
//            var rotationChunk = chunk.GetNativeArray(RotationArchetype);
//            for (int i = 0; i < chunk.Count; i++) {
//                var rotation = rotationChunk[i];
//                rotation.Value = math.mul(math.normalize(rotation.Value),
//                    quaternion.AxisAngle(math.forward(quaternion.identity), 3.0f * dt));
//                rotationChunk[i] = rotation;
//            }
//        }
//    }
//    
//    protected override void OnUpdate() {
//        var dt = Time.DeltaTime;
//        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
//        var playerEntity = playerGroup.GetSingletonEntity();
//        var worldTurnOffEntity = worldDirGroup.GetSingletonEntity();
//
//        Entities.WithName("World_turn") // Shown in error messages and profiler
//            .WithAll<TurnTag>()
//            .WithoutBurst()
//            .WithStructuralChanges()
//            .ForEach(
//                (Entity entity, ref WorldTurn worldTurn) => { // got crash if in instead ref
//                    if (!worldTurn.isSpinningNow) {
//                        em.RemoveComponent<TurnTag>(entity);
//                    }
//                }
//            ).Run();
//        em.RemoveComponent<TurnTag>(playerEntity);
// Calculate Angle 
//        Entities.WithName("Calculate_Angle_for_World_turn") 
//            .WithChangeFilter<ClickToTurn>()
//            .WithoutBurst()
//            .WithStructuralChanges()
//            .ForEach(
//                (Entity entity, ref WorldTurn worldTurn, ref Rotation rot,
//                    ref LocalToWorld localToWorld, in ClickToTurn click) => {
//                    
//                    if (!worldTurn.isSpinningNow) {
//                        em.RemoveComponent<TurnTag>(entity);
//                        bool dd = EntityManager.HasComponent<WorldTurn>(entity);
//                    }
//                    float3 newDirect = math.round(math.mul(quaternion.Euler(0, 0, math.radians(90 * (int) click.Clockwise)),
//                        new float3(worldTurn.direction.x, worldTurn.direction.y, 0)));
//
//                    worldTurn.startAngle = rot.Value;
//                    worldTurn.direction = new int2((int) newDirect.x, (int) newDirect.y);
//                    worldTurn.goalAngle =
//                        math.mul(FromToRot90(localToWorld.Up, new float3(worldTurn.direction.x, worldTurn.direction.y, 0)),
//                            rot.Value);
//                    worldTurn.timeCount = 0;
//                    worldTurn.isSpinningNow = true;
//                    //CommandBuffer.AddComponent(index, entity, new TurnTag());
//                    em.AddComponent<TurnTag>(entity);
//                }
//            ).Run();
//       
//        var worldTurnOff = em.GetComponentData<WorldTurn>(worldTurnOffEntity);
//        
//        if (worldTurnOff.isSpinningNow) {
//            em.AddComponentData(worldTurnOffEntity, new TurnTag());
//        }
//        
//       // Player Direction Vector Job
//       int2 dir = worldTurnOff.direction;
//       Entities
//           .WithName("Calculate_player_direction_turn") 
//           .WithNone<TurnTag>()
//           .WithoutBurst()
//           .WithStructuralChanges()
//           .ForEach(codeToRun: (Entity entity, ref Heading heading) => {
//                   heading.VectorDirection = new int2(dir.x, dir.y);
//                   em.AddComponent<TurnTag>(entity);
//           }
//           ).Run();
//       
//       var needCalculateAngle = em.GetComponentData<ClickToTurn>(worldTurnOffEntity).needCalculateAngle;
//       if (needCalculateAngle) {
//           Entities
//               .WithName("need_Calculate_Angle_angle_for_turn") 
//               .WithAll<TurnTag>()
//               .WithoutBurst()
//               .WithStructuralChanges()
//               .ForEach((Entity entity, ref WorldTurn worldTurn, ref ClickToTurn clickToTurn) => {
//                   if (worldTurn.timeCount < 1f) {
//                       //TODO Dampen
//                       float smooth1 = math.smoothstep(0, 1f, worldTurn.timeCount);
//                       float smooth2 = math.smoothstep(0, 1f, smooth1);
//                       worldTurn.quaternion = math.slerp(worldTurn.startAngle, worldTurn.goalAngle, worldTurn.timeCount);
//                       worldTurn.timeCount += dt * worldTurn.angularVelocity;
//                       worldTurn.isSpinningNow = true;
//                   }
//                   else {
//                       worldTurn.quaternion = worldTurn.goalAngle;
//                       worldTurn.isSpinningNow = false;
//                       worldTurn.timeCount = 0;
//                       em.RemoveComponent<ClickToTurn>(entity);
//                   }
//               }).Run();
//
//           var worldTurnOff2 = em.GetComponentData<WorldTurn>(worldTurnOffEntity);
//           var rotationArchetype = GetArchetypeChunkComponentType<Rotation>();
//           
//           var turnJob = new TurnOffJobChunk() {
//               RotationArchetype = rotationArchetype,
//               WorldTurn = worldTurnOff2
//           };
//           var turnHandle = turnJob.Schedule(turnTagGroup);
////           turnHandle.Complete();
//           
//           var rotateJob = new RotateJobChunk() {
//               RotationArchetype = rotationArchetype,
//               dt = dt,
//           }.Schedule(turnTagGroup, turnHandle);
//           rotateJob.Complete();

//       };
//    }
//}