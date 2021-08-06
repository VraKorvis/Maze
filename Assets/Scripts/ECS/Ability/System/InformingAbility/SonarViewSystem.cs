using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(AfterSimulationSystemGroup))]
[UpdateAfter(typeof(IndicatorPointerSystem))]
public class SonarViewSystem : SystemBase {
    private EntityQuery indicatorGroup;

    protected override void OnCreate() {
        indicatorGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadOnly<IndicatorPointer>(),
                ComponentType.ReadOnly<RenderMesh>(),
                ComponentType.ReadOnly<Scale>(),
            },
            Options = EntityQueryOptions.FilterWriteGroup,
        });
        RequireForUpdate(indicatorGroup);
    }

    protected override void OnUpdate() {
        var camMain = Camera.main;
//         var sonarViewJobHandle =
        Entities.WithName("SonarViewJob_ScreenToWorld")
            .WithoutBurst()
            .ForEach((Entity e, ref Translation position, in IndicatorPointer indicator) => {
                var pos = camMain.ScreenToWorldPoint(indicator.screenPoint);
                pos.z = 0;
                position.Value = pos;
            }).Run();
    }
}