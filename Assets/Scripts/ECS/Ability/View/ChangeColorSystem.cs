using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class ChangeColorSystem : ComponentSystem {
    // private ComputeBuffer _buffer;
    private EntityQuery sonarGroup;

    private NativeArray<Color> colorBuffer;

    protected override void OnCreate() {
        sonarGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadWrite<RenderMesh>(),
                ComponentType.ReadOnly<IndicatorPointer>(),
                ComponentType.ReadOnly<ChangeColor>(),
            },
            Options = EntityQueryOptions.FilterWriteGroup
        });
    }

    protected override void OnUpdate() {
//        var ents  = sonarGroup.ToEntityArray(Allocator.TempJob);
//        var rm    = EntityManager.GetSharedComponentData<RenderMesh>(ents[0]);
//        ents.Dispose();
//        var count = sonarGroup.CalculateEntityCount();
//        var buffer = new ComputeBuffer(count, 4 * sizeof(float));
//        colorBuffer = new NativeArray<Color>(sonarGroup.CalculateEntityCount(), Allocator.TempJob);
//        for (int i = 0; i < colorBuffer.Length; i++) {
//            colorBuffer[i] = Color.blue;
//        }
//        Entities.WithAll<SonarIndicator, ChangeColor>()
//                .ForEach((Entity e, ref SonarIndicator sonar,ref ChangeColor color) => {
//                     var mesh = EntityManager.GetSharedComponentData<RenderMesh>(e);
//                     mesh.material.SetBuffer("_myColorBuffer", buffer);
//                     colorBuffer[sonar.computeIndex] = color.color;
//                     PostUpdateCommands.RemoveComponent<ChangeColor>(e);
//                 });
//
//        
//        buffer.SetData(colorBuffer);
//        rm.material.SetBuffer("_myColorBuffer", buffer);
//        buffer.Dispose();
//        colorBuffer.Dispose();
//        
    }
}

public struct ChangeColor : IComponentData {
    public Color color;
}