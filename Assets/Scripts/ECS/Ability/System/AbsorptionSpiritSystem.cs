using Unity.Entities;

[UpdateInGroup(typeof(AfterSimulationSystemGroup))]
[DisableAutoCreation]
//TODO need rework
public class AbsorptionSpiritSystem : SystemBase {
    //private EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    protected override void OnCreate() {
        //        m_EndSimulationEcbSystem = World.DefaultGameObjectInjectionWorld
        //            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate() {
        //        var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entities.WithName("AbsorptionSpiritJob")
            .WithNone<MarkedToRespawnTag, Invulnerability>()
            .WithoutBurst()
            .WithStructuralChanges()
            .WithChangeFilter<CollisionInfo>()
            .ForEach((Entity e, ref PlayerData player, ref CollisionInfo collision) => {
                var enemyEntity = collision.another;
                if (em.HasComponent<Spirit>(enemyEntity)) {
                    var enemyData = em.GetComponentData<Spirit>(enemyEntity);
                    if (enemyData.guise == player.currentGuise) {
                        em.AddComponentData(enemyEntity, new DestroyTag());
                    }
                    else {
                        // ecb.AddComponent(e, new MarkedToRespawnTag());
                    }
                }

                collision.another = Entity.Null;
            }).Run();
    }
}