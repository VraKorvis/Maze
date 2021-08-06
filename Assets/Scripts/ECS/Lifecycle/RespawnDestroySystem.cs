using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class RespawnSystem : ComponentSystem {

    protected override void OnCreate() {
        RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<RespawnData>(), ComponentType.ReadOnly<MarkedToRespawnTag>()));
    }

    protected override void OnUpdate() {
        Entities.WithAll<PlayerData, MarkedToRespawnTag>()
                .ForEach((Entity           e, ref RespawnData respawnData, ref Rotation rot, ref Heading heading, ref Translation position,
                          ref MoveSettings moveData) => {
                     //heading.VectorDirection = respawnData.VectorDirection;
                     position.Value             = respawnData.point;
                     moveData.targetCellBlocked = false;
                     //rot.Value               = quaternion.identity;
                     PostUpdateCommands.RemoveComponent<MarkedToRespawnTag>(e);
                 });
    }
}
[DisableAutoCreation]
public class DestroyHauntingSystem : ComponentSystem {

    private EntityManager em;
    protected override void OnCreate() {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    protected override void OnUpdate() {
        Entities.WithAll<HauntingTag>()
            .ForEach(e => {
               em.DestroyEntity(e);
            });
    }
}
