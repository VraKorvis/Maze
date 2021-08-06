using Unity.Entities;
using Unity.Transforms;

// [UpdateInGroup(typeof(AfterSimulationSystemGroup))]
[DisableAutoCreation]
public class CameraFollowSystem : SystemBase {
    private EntityQuery m_player;
    private EntityManager em;

    protected override void OnCreate() {
        // m_player = GetEntityQuery(new EntityQueryDesc() {
        //     All = new[] {
        //         ComponentType.ReadOnly<PlayerData>(),
        //     },
        //     Options = EntityQueryOptions.FilterWriteGroup,
        // });
        // RequireForUpdate(m_player);
        // em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    protected override void OnUpdate() {
        // var playerEntity = m_player.GetSingletonEntity();
        // var playerPosition = em.GetComponentData<Translation>(playerEntity);
        // var playerRotation = em.GetComponentData<Rotation>(playerEntity);
        //
        // Entities.WithName("CameraFollowJob")
        //     .WithAll<TrackedByCamera>()
        //     .WithBurst()
        //     .ForEach((ref Translation translation, ref Rotation rotation) => {
        //         translation.Value = playerPosition.Value;
        //         rotation.Value = playerRotation.Value;
        //     }).Schedule();
    }
}