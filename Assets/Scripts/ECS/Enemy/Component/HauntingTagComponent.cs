using Unity.Entities;
using UnityEngine;

public struct HauntingTag : ISharedComponentData { }

[RequiresEntityConversion]
//[RequireComponent(typeof(SpiritComponent))]
public class HauntingTagComponent : MonoBehaviour, IConvertGameObjectToEntity {
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddSharedComponentData(entity, new HauntingTag());
        dstManager.AddComponentData(entity, new PathMovementTag());
        dstManager.AddComponentData(entity, new CellMovementTag());
        // dstManager.AddComponentData(entity, new IndicesForwardCellTemp());
        dstManager.AddComponentData(entity, new SpellImmuneTag());
        //dstManager.AddComponentData(entity, new TargetForHaunting());
    }

    private void OnValidate() {
        GetComponent<SpiritComponent>().guise = Guise.HauntingMask;
    }
}