using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct QuadrantData {
    public Entity entity;
    public float3 position;
}

[UpdateInGroup(typeof(CollisionSystemGroup))]
[DisableAutoCreation]
//TODO need rework, optimize
public class QuadrantCollisionSystem : SystemBase {
    private EntityQuery aabbGroup;

    public NativeMultiHashMap<int, QuadrantData> QuadrantMultiMap;
    private int lengthOfHashMap;

    protected override void OnCreate() {
        aabbGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadWrite<AABB>(),
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadWrite<CollisionInfo>(),
            },
            Options = EntityQueryOptions.FilterWriteGroup
        });
        aabbGroup.SetChangedVersionFilter(typeof(Translation));
        // lengthOfHashMap = aabbGroup.CalculateEntityCount();
        QuadrantMultiMap = new NativeMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);
    }

//    [BurstCompile]
//    private struct UpdateCollider : IJobForEach<AABB, Translation> {
//        public void Execute(ref AABB aabb, [ChangedFilter] [ReadOnly] ref Translation position) {
//            aabb.Max = position.Value + aabb.Offset;
//            aabb.Min = position.Value - aabb.Offset;
//            aabb.Min.z = 0;
//            aabb.Max.z = 0;
//        }
//    }

    [BurstCompile]
    private struct AABBJob : IJobParallelFor {
        [ReadOnly]
        public ComponentDataFromEntity<AABB> aabb;

        [NativeDisableParallelForRestriction]
        public ComponentDataFromEntity<CollisionInfo> collisions;

        [DeallocateOnJobCompletion]
        [ReadOnly]
        public NativeArray<Entity> entities;

        public void Execute(int index) {
            for (int i = index + 1; i < entities.Length; i++) {
                if (PhysicsUtils.OverlapAABB(aabb[entities[index]], aabb[entities[i]])) {
                    var tmpCollision = collisions[entities[index]];
                    tmpCollision.self = entities[index];
                    tmpCollision.another = entities[i];
                    collisions[entities[index]] = tmpCollision;

                    tmpCollision = collisions[entities[i]];
                    tmpCollision.self = entities[i];
                    tmpCollision.another = entities[index];
                    collisions[entities[i]] = tmpCollision;
                }
            }
        }
    }

    // is obsolete
//    [BurstCompile]
//    [RequireComponentTag(typeof(AABB))]
//    private struct SetQuadrantJob : IJobForEachWithEntity<Translation> {
//        public NativeMultiHashMap<int, QuadrantData>.ParallelWriter quadrant;
//
//        public void Execute(Entity entity, int index, [ChangedFilter] [ReadOnly] ref Translation position) {
//            var key = GridUtils.GetPositionHashMapKey(position.Value);
//            quadrant.Add(key, new QuadrantData() {
//                position = position.Value,
//                entity = entity
//            });
//        }
//    }

    [BurstCompile]
    private struct AABBQuadrantJob : IJobParallelFor {
        [ReadOnly]
        public ComponentDataFromEntity<AABB> aabb;

        [NativeDisableParallelForRestriction]
        public ComponentDataFromEntity<CollisionInfo> collisions;

        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public ComponentDataFromEntity<Translation> positions;

        [DeallocateOnJobCompletion]
        [ReadOnly]
        public NativeArray<Entity> entities;

        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeMultiHashMap<int, QuadrantData> quadrant;

        public void Execute(int index) {
            var ownerKey = GridUtils.GetPositionHashMapKey(positions[entities[index]].Value);

            if (quadrant.TryGetFirstValue(ownerKey, out var quadrantData, out var iter)) {
                do {
                    if (quadrantData.entity == entities[index]) continue;

                    if (PhysicsUtils.OverlapAABB(aabb[entities[index]], aabb[quadrantData.entity])) {
                        var collision = collisions[entities[index]];
                        collision.self = entities[index];
                        collision.another = quadrantData.entity;
                        collisions[entities[index]] = collision;

                        collision = collisions[quadrantData.entity];
                        collision.self = quadrantData.entity;
                        collision.another = entities[index];
                        collisions[quadrantData.entity] = collision;
                    }
                } while (quadrant.TryGetNextValue(out quadrantData, ref iter));
            }
        }
    }

    protected override void OnDestroy() {
        QuadrantMultiMap.Dispose();
    }

    //TODO Internal: deleting an allocation that is older than its permitted lifetime of 4 frames (age = 5)
    protected override void OnUpdate() {
#if UNITY_EDITOR
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GridUtils.DebugDrawQuadrant(mousePos);
#endif

        //var updateColliderJob = new UpdateCollider().Schedule(aabbGroup, inputDeps);
        var updateColliderJobHandle =
            Entities.WithName("UpdateColliderJob")
                .WithChangeFilter<Translation>()
                .WithBurst()
                .ForEach((ref AABB aabbcollider, in Translation position) => {
                    aabbcollider.Max = position.Value + aabbcollider.Offset;
                    aabbcollider.Min = position.Value - aabbcollider.Offset;
                    aabbcollider.Min.z = 0;
                    aabbcollider.Max.z = 0;
                }).ScheduleParallel(Dependency);

        var aabbGroupSize = aabbGroup.CalculateEntityCount();

//        var aabbJob = new AABBJob() {
//            aabb       = aabb,
//            collisions = collisions,
//            entities   = entities,
//        }.Schedule(entities.Length, 32, updateColliderJob);
//        return aabbJob;

        QuadrantMultiMap.Clear();
        if (aabbGroupSize > QuadrantMultiMap.Capacity) {
            QuadrantMultiMap.Capacity = aabbGroupSize;
        }

//        var quadrantJob = new SetQuadrantJob() {
//            quadrant = QuadrantMultiMap.AsParallelWriter(),
//        }.Schedule(aabbGroup, updateColliderJob);

        // NativeMultiHashMap<int, QuadrantData>.ParallelWriter quadrant = QuadrantMultiMap.AsParallelWriter();
        NativeMultiHashMap<int, QuadrantData> quadrant = QuadrantMultiMap;

        var setQuadrantJobHandle =
            Entities.WithName("SetQuadrantJob")
                .WithAll<AABB>()
                .WithChangeFilter<Translation>()
                .WithBurst()
                .ForEach((Entity entity, int entityInQueryIndex, in Translation position) => {
                    var key = GridUtils.GetPositionHashMapKey(position.Value);
                    quadrant.Add(key, new QuadrantData() {
                        position = position.Value,
                        entity = entity
                    });
                }).Schedule(Dependency); //ScheduleParallel(Dependency)

        var updateColliderAndSetQuadrantHandle =
            JobHandle.CombineDependencies(updateColliderJobHandle, setQuadrantJobHandle);
        //Dependency = updateColliderAndSetQuadrantHandle;
        var entities = aabbGroup.ToEntityArrayAsync(Allocator.TempJob, out var quadrantHandle);
        var aabb = GetComponentDataFromEntity<AABB>();
        var collisions = GetComponentDataFromEntity<CollisionInfo>();

        var aabbQuadrantJobHandle = new AABBQuadrantJob() {
            aabb = aabb,
            collisions = collisions,
            entities = entities,
            quadrant = QuadrantMultiMap,
            positions = GetComponentDataFromEntity<Translation>(),
        }.Schedule(entities.Length, 16,
            JobHandle.CombineDependencies(quadrantHandle, updateColliderAndSetQuadrantHandle));
        Dependency = aabbQuadrantJobHandle;
    }
}