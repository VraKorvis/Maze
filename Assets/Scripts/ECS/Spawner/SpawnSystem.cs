using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisableAutoCreation]
public class SpawnSystem : SystemBase {
    EntityCommandBufferSystem m_EntityCommandBufferSystem;

    protected override void OnCreate() {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
    }

    // job is obsolete
//    struct SpawnJob : IJobForEachWithEntity<Spawner, LocalToWorld> {
//        public EntityCommandBuffer.Concurrent CommandBuffer;
//
//        public void Execute(Entity entity, int index, [ReadOnly] ref Spawner spawner, [ReadOnly] ref LocalToWorld location) {
//            for (var x = 0; x < spawner.CountX; x++) {
//                for (var y = 0; y < spawner.CountY; y++) {
//                    var instance = CommandBuffer.Instantiate(index, spawner.Prefab);
//
//                    // Place the instantiated in a grid with some noise
//                    var position = math.transform(location.Value, new float3(x * 1.3F, noise.cnoise(new float2(x, y) * 0.21F) *2, 0));
//                    CommandBuffer.SetComponent(index, instance, new Translation {Value = position});
//                }
//            }
//            CommandBuffer.DestroyEntity(index, entity);
//        }
//    }

    protected override void OnUpdate() {
//        var ecb = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
//        Entities
//            .WithName("SpawnInstantiateJob")
//            .WithoutBurst()
//            .WithStructuralChanges()
//            .ForEach((Entity entity, int entityInQueryIndex, in Spawner spawner, in LocalToWorld localToWorld) => {
//                
//                for (var x = 0; x < spawner.CountX; x++) {
//                    for (var y = 0; y < spawner.CountY; y++) {
//                        var instance = ecb.Instantiate(entityInQueryIndex, spawner.Prefab);
//
//                        // Place the instantiated entity in the grid with some noise
//                        var position = math.transform(localToWorld.Value, new float3(x * 1.3F, noise.cnoise(new float2(x, y) * 0.21F) *2, 0));
//                        ecb.SetComponent(entityInQueryIndex, instance, new Translation {Value = position});
//                    }
//                }
//                ecb.DestroyEntity(entityInQueryIndex, entity);
//            }).Run();
//        
////            var job = new SpawnJob {
////                CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
////            }.Schedule(this, inputDeps);
//
//        m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
//        
    }
}