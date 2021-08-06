using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AttackFoundEnemies {
    private static readonly float DEBUG_TIMER = 10;

    public static NativeList<Entity> FilterEnemies(NativeList<Entity> enemies, Translation playerPos,
        CellIndex playerCell, AttackAbility attackAbility) {
        EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var foundEnemies = new NativeList<Entity>(Allocator.Persistent);
        for (int i = 0; i < enemies.Length; i++) {
            var spiritPos = em.GetComponentData<Translation>(enemies[i]);
            var spiritPos2d = new float2(spiritPos.Value.x, spiritPos.Value.y);

            var direction = spiritPos.Value - playerPos.Value;
            var direction2d = new float2(direction.x, direction.y);

            if (attackAbility.throughObstacle) {
                var distToNeighborCell =
                    math.distance(playerCell.coord, em.GetComponentData<CellIndex>(enemies[i]).coord);
                if (distToNeighborCell < 1.5f) {
                    Debug.DrawRay(playerPos.Value, direction, Color.green, DEBUG_TIMER);
                    foundEnemies.Add(enemies[i]);
                    continue;
                }

                var playerPos2d = new float2(playerPos.Value.x, playerPos.Value.y);
                RaycastHit2D hit = Physics2D.Raycast(playerPos2d, direction2d, attackAbility.radius);

                var distanceToSpirit = math.distance(playerPos2d, spiritPos2d);
                if (hit.collider != null && hit.distance < distanceToSpirit) {
                    Debug.DrawRay(playerPos.Value, direction, Color.red, DEBUG_TIMER);
                }
                else {
                    Debug.DrawRay(playerPos.Value, direction, Color.green, DEBUG_TIMER);
                    foundEnemies.Add(enemies[i]);
                }
            }
            else {
                Debug.DrawRay(playerPos.Value, direction, Color.green, DEBUG_TIMER);
                foundEnemies.Add(enemies[i]);
            }
        }

        return foundEnemies;
    }
}
// is obsolete
//[BurstCompile]
//[RequireComponentTag(typeof(Spirit))]
//[ExcludeComponent(typeof(HauntingTag), typeof(SpellImmuneTag))]
//public struct FilterFoundNearestEnemiesJob : IJobForEachWithEntity<Translation> {
//    [ReadOnly] public float radiusAbility;
//
//    [ReadOnly] public Translation playerPos;
//
//    [NativeDisableParallelForRestriction] public NativeList<Entity> founds;
//
//    public void Execute(Entity entity, int index, ref Translation position) {
//        var distanceToSpirit = math.distance(position.Value, playerPos.Value);
//        if (distanceToSpirit < radiusAbility) {
//            founds.Add(entity);
//        }
//    }
//}

[BurstCompile]
[RequireComponentTag(typeof(Spirit))]
[ExcludeComponent(typeof(HauntingTag), typeof(SpellImmuneTag))]
public struct FilterFoundNearestEnemiesJobChunk : IJobChunk {
    public ArchetypeChunkComponentType<Translation> TranslationArchetype;
    [ReadOnly] public ArchetypeChunkEntityType EntitiesArch;
    [ReadOnly] public float radiusAbility;

    [ReadOnly] public Translation playerPos;

    [NativeDisableParallelForRestriction] public NativeList<Entity> founds;

    public void Execute(Entity entity, int index, ref Translation position) {
        var distanceToSpirit = math.distance(position.Value, playerPos.Value);
        if (distanceToSpirit < radiusAbility) {
            founds.Add(entity);
        }
    }

    public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
        var translationChunk = chunk.GetNativeArray(TranslationArchetype);
        var entitiesChunk = chunk.GetNativeArray(EntitiesArch);
        for (int i = 0; i < chunk.Count; i++) {
            var position = translationChunk[i];
            var entity = entitiesChunk[i];
            var distanceToSpirit = math.distance(position.Value, playerPos.Value);
            if (distanceToSpirit < radiusAbility) {
                founds.Add(entity);
            }
        }
    }
}