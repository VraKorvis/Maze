using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(CollisionSystemGroup))]
public class DestroySystem : ComponentSystem {

    protected override void OnUpdate() {
        Entities.WithAll<DestroyTag>().WithNone<PlayerData>().ForEach(e => {
            PostUpdateCommands.DestroyEntity(e);
        });
    }
}
