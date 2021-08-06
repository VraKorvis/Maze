using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public class DestroyMazeWorldSystem : SystemBase {
    protected override void OnUpdate() {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var entityArray = em.GetAllEntities();
        em.DestroyEntity(entityArray);
        entityArray.Dispose();
        //Unity.Entities.World.DisposeAllWorlds();
    }
}