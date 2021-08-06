using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(AfterSimulationSystemGroup))]
[DisableAutoCreation]
public class DebugCharactersMovementHeadingSystem : SystemBase {
    private static readonly float DEBUG_TIMER = 0.01f;
    // private EntityQuery m_HeadingLineGroup;

    protected override void OnCreate() {
        // m_HeadingLineGroup = GetEntityQuery(new EntityQueryDesc {
        //     All = new[] {
        //         ComponentType.ReadWrite<Heading>(),
        //     },
        //     Options = EntityQueryOptions.FilterWriteGroup
        // });
        // RequireForUpdate(m_HeadingLineGroup);
    }

    [RequireComponentTag(typeof(Spirit), typeof(Heading))]
    [ExcludeComponent(typeof(DebugCharactersHeadingLineTag))]
    private struct AddDebugHeadingLineToAllCharactersChunkJob : IJobChunk {
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            throw new System.NotImplementedException();
        }
    }

    protected override void OnUpdate() {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entities.WithName("AddDebugHeadingLineToAllCharactersJob")
            .WithAny<Heading>()
            .WithNone<DebugCharactersHeadingLineTag>()
            .WithoutBurst()
            .WithStructuralChanges()
            .ForEach((Entity e) => { em.AddComponent<DebugCharactersHeadingLineTag>(e); }).Run();

        Entities.WithName("AddDebugHeadingLineToCircleCharactersJob")
            // .WithAny<DirectionsSight>()
            .WithoutBurst()
            .WithStructuralChanges()
            .ForEach((Entity e, ref Translation position, ref DirectionsSight heading) => {
                var heading3d = new Vector3(heading.VectorDirection.x, heading.VectorDirection.y, 0);
                Debug.DrawRay(position.Value, heading3d, Color.blue, DEBUG_TIMER);
            }).Run();

        Entities.WithName("ShowDebugHeadingLineToAllCharactersJob")
            .WithAll<DebugCharactersHeadingLineTag>()
            //.WithNone<DebugCharactersHeadingLineTag>()
            .WithNone<DirectionsSight>()
            .WithoutBurst()
            .WithStructuralChanges()
            .ForEach((Entity e, ref Translation position, ref Heading heading) => {
                var heading3d = new Vector3(heading.VectorDirection.x, heading.VectorDirection.y, 0);
                Debug.DrawRay(position.Value, heading3d, Color.blue, DEBUG_TIMER);
            }).Run();
    }
}