using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Circle : IComponentData {
    public float radius;
    public float3 pivot;
    public float3 angle;
    public Clockwise Clockwise;
}

[Serializable]
[RequiresEntityConversion]
//[RequireComponent(typeof(SpiritComponent))]
public class CircleComponent : MonoBehaviour, IConvertGameObjectToEntity {
    public float radius;
    public Clockwise Clockwise;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        var data = new Circle() {
            radius = radius,
            pivot = GetComponent<Transform>().position,
            angle = new float3(0, 0, 0),
            Clockwise = Clockwise,
        };
        dstManager.AddComponentData(entity, data);
//        dstManager.AddComponentData(entity, new CellMovementTag());
//        dstManager.AddComponentData(entity, new IndicesForwardCellTemp());
        dstManager.AddComponentData(entity, new PathRequestAgent());
        dstManager.AddComponentData(entity, new PathAgentStatus());
        dstManager.AddBuffer<Waypoint>(entity);
    }

    private void OnValidate() {
        GetComponent<SpiritComponent>().guise = Guise.Circle;
    }
}