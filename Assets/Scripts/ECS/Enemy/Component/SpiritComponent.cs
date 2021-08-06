using System;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

public struct Spirit : IComponentData {
   public Guise guise;
}

[Serializable]
[RequiresEntityConversion]
public class SpiritComponent : MonoBehaviour, IConvertGameObjectToEntity {
    [Tooltip("ID enemy Guise. Corresponds to the player's IDGuise")]
    [DisplayWithoutEdit]
    public Guise guise;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponentData(entity, new Spirit() {
            guise = guise,
        });
    }

}

[Serializable]
public enum WayOrient {
    Horizontal,
    Vertical
}
