using Unity.Entities;
using UnityEngine;

public struct IslandSpiritTag : IComponentData {
    
}

public class IslandSpiritComponent : MonoBehaviour, ICheckCorrectnessPathMovement, IConvertGameObjectToEntity {
   
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {

        dstManager.AddComponentData(entity, new IslandSpiritTag());
    }

    public bool CheckCorrectnessPathMovement() {
        //TODO Check at least one wall near within spirit
        return true;
    }
}


