using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Disable its mean next time you need self manually update this system each frame sys.update()
// if SYS.Enabled == false you cant manually update this sys
[UpdateInGroup(typeof(InitializationGroup))]
[UpdateBefore(typeof(UpdateCellIndexSystem))]
[DisableAutoCreation]
public class SpawnPathFindSystem : SystemBase {
    private EntityQuery spawnerGroup;
    private EntityQuery m_playerGroup;

    // private NativeArray<int> randoms;
    public int count = 100;

    EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    protected override void OnCreate() {
        spawnerGroup = GetEntityQuery(typeof(Spawner));
        m_playerGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadOnly<PlayerData>(),
                ComponentType.ReadOnly<CellIndex>(),
            },
            Options = EntityQueryOptions.FilterWriteGroup,
        });
        // randoms = new NativeArray<int>(count, Allocator.Persistent);
        m_EndSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    // is obsolete
//    private struct SpawnPathFindingSpiritJob : IJobForEachWithEntity<Spawner> {
//        [NativeDisableParallelForRestriction] public DynamicBuffer<GridBuffer> buffer;
//
//        [NativeDisableParallelForRestriction] public NativeArray<int> randoms;
//
//        public int count;
//        public EntityCommandBuffer.Concurrent CommandBuffer;
//
//        public void Execute(Entity entity, int index, ref Spawner spawner) {
//            for (int i = 0; i < count; i++) {
//                var instance = CommandBuffer.Instantiate(index, spawner.Prefab);
//
//                var ind = randoms[i];
//                var position = new Translation() {
//                    Value = buffer[ind].WorldPos,
//                };
//
//                CommandBuffer.SetComponent(index, instance, position);
//
//                var speed = (ind / 337.0f) + 0.5f;
//                CommandBuffer.SetComponent(index, instance, new MoveSettings() {
//                    startDelay = 0.2f,
//                    isMoving = true,
//                    speed = speed,
//                    targetCellPos = position.Value
//                });
//            }
//
//            CommandBuffer.DestroyEntity(index, entity);
//        }
//    }

//    protected override JobHandle OnUpdate(JobHandle inputDeps) {
//        //Instead of performing structural changes directly, a Job can add a command to an EntityCommandBuffer to perform such changes on the main thread after the Job has finished.
//        //Command buffers allow you to perform any, potentially costly, calculations on a worker thread, while queuing up the actual insertions and deletions for later.
//
//        // Schedule the job that will add Instantiate commands to the EntityCommandBuffer.
//        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
//        var gridEntity = GetEntityQuery(typeof(GridBuffer)).GetSingletonEntity();
//
//        var buffers = GetBufferFromEntity<GridBuffer>();
//        var buffer = buffers[gridEntity];
//
//        for (int i = 0; i < randoms.Length; i++) {
//            int tmp = 0;
//            int random = Random.Range(0, buffer.Length - 1);
//
//            if (buffer[random].Type == CellType.Ground) {
//                randoms[i] = random;
//            }
//            else {
//                randoms[i] = 29;
//            }
//        }
//
//        var spawnPathFindingSpiritJob = new SpawnPathFindingSpiritJob() {
//            buffer = buffer,
//            count = count,
//            randoms = randoms,
//            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
//        }.Schedule(this, inputDeps);
//
////         SpawnJob runs in parallel with no sync point until the barrier system executes.
////         When the barrier system executes we want to complete the SpawnJob and then play back the commands (Creating the entities and placing them).
////         We need to tell the barrier system which job it needs to complete before it can play back the commands.
//        m_EntityCommandBufferSystem.AddJobHandleForProducer(spawnPathFindingSpiritJob);
//
//        return spawnPathFindingSpiritJob;
//    }

    protected override void OnDestroy() {
        // randoms.Dispose();
    }

    protected override void OnUpdate() {
        //Instead of performing structural changes directly, a Job can add a command to an EntityCommandBuffer to perform such changes on the main thread after the Job has finished.
        //Command buffers allow you to perform any, potentially costly, calculations on a worker thread, while queuing up the actual insertions and deletions for later.
//        Debug.Log("Spawn path finding system");
        // randoms.Dispose();
        var randoms = new NativeArray<int>(count, Allocator.Persistent);
        // Schedule the job that will add Instantiate commands to the EntityCommandBuffer.
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var gridEntity = GetEntityQuery(typeof(GridBuffer)).GetSingletonEntity();

        var buffers = GetBufferFromEntity<GridBuffer>(true);
        var buffer = buffers[gridEntity];

        for (int i = 0; i < randoms.Length; i++) {
            int tmp = 0;
            int random = Random.Range(0, buffer.Length - 1);

            if (buffer[random].Type == CellType.Ground) {
                randoms[i] = random;
            }
            else {
                randoms[i] = 29;
            }
        }

//        Job is obsolete after update to 2019.4 LTS
//        var spawnPathFindingSpiritJob = new SpawnPathFindingSpiritJob() {
//            buffer = buffer,
//            count = count,
//            randoms = randoms,
//            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
//        }.Schedule(this, inputDeps);
        var playerEntity = m_playerGroup.GetSingletonEntity();
        var cellGoal = em.GetComponentData<CellIndex>(playerEntity);
        var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

        var spawnPathFindingSpiritJob =
            Entities.WithName("SpawnPathFindingSpiritJob")
// //            .WithAll<Spawner>()
                .WithDeallocateOnJobCompletion(randoms)
                .WithBurst()
                .WithReadOnly(buffer)
//             .WithoutBurst()
//             .WithStructuralChanges()
                .ForEach((Entity e, int entityInQueryIndex, in Spawner spawner) => {
                    for (int i = 0; i < randoms.Length; i++) {
                        var instance = ecb.Instantiate(entityInQueryIndex, spawner.Prefab);

                        var ind = randoms[i];
                        var position = new Translation() {
                            Value = buffer[ind].WorldPos,
                        };

                        ecb.SetComponent(entityInQueryIndex, instance, position);

                        var speed = (ind / 337.0f) + 0.5f;
                        ecb.SetComponent(entityInQueryIndex, instance, new MoveSettings() {
                            startDelay = 0.2f,
                            isMoving = true,
                            speed = speed,
                            targetCellPos = position.Value
                        });

                        // ecb.SetComponent(instance, new TargetForHaunting() {
                        //     focus = playerEntity,
                        //     cellIndex = cellGoal
                        // });


                        // ecb.SetComponent(instance, new PathAgentStatus() {
                        //     Value = AgentStatus.Find
                        // });
                        //
                        // ecb.SetComponent(entityInQueryIndex, instance, new PathAgentStatusNoneTag());
                        ecb.AddComponent(entityInQueryIndex, instance, new PathAgentStatusFindTag());
                        ecb.AddComponent(entityInQueryIndex, instance, new PathAgentStatusProcessTag());
                    }

                    // ecb.DestroyEntity(e);
                }).ScheduleParallel(Dependency);

        

//         SpawnJob runs in parallel with no sync point until the barrier system executes.
//         When the barrier system executes we want to complete the SpawnJob and then play back the commands (Creating the entities and placing them).
//         We need to tell the barrier system which job it needs to complete before it can play back the commands.
        m_EndSimulationEcbSystem.AddJobHandleForProducer(spawnPathFindingSpiritJob);
        Dependency = spawnPathFindingSpiritJob;
        spawnPathFindingSpiritJob.Complete();
        
        // randoms.Dispose(spawnPathFindingSpiritJob);
    }
}