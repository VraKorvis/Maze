using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

//[WriteGroup(typeof(RenderMesh))]
//[WriteGroup(typeof(Scale))]
public struct IndicatorPointer : IComponentData {
    public Entity targetEntity;
    public float3 screenPoint;
    public float radius;
    public float duration;
}