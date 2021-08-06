using Unity.Entities;
using UnityEngine;

public struct HomeTag : IComponentData { }

public class HomeTagComponent : MonoBehaviour, IConvertGameObjectToEntity {

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponentData(entity, new HomeTag());
    }
}