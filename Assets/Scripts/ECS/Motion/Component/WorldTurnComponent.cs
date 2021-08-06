using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct WorldTurn : IComponentData {
    
    public int2       direction;
    public quaternion quaternion;
    public float      angularVelocity;
    public quaternion startAngle;
    public quaternion goalAngle;
    public float      timeCount;
    public bool       isSpinningNow;
    
}

[RequiresEntityConversion]
public class WorldTurnComponent : MonoBehaviour, IConvertGameObjectToEntity {
    
    [Tooltip("World Angular Velocity when turn off (convert to radians)")]
    [Range(0f,360f)]
    public float      angularVelocity;
   
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponentData(entity, new WorldTurn() {
            direction           = new int2(0,1),
            quaternion      = quaternion.identity,
            isSpinningNow   = false,
            timeCount       = 0,
            angularVelocity = math.radians(angularVelocity)
        });
        dstManager.AddComponentData(entity, new TurnTag());
    }
}