using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct Spawner : IComponentData {
    public Entity Prefab;
    public int    CountX;
    public int    CountY;
}

[RequiresEntityConversion]
public class SpawnerComponent : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {

    public GameObject Prefab;
    public int        CountX;
    public int        CountY;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        var spawnData = new Spawner() {
            Prefab = conversionSystem.GetPrimaryEntity(Prefab),
            CountX = CountX,
            CountY = CountY,
        };
        dstManager.AddComponentData(entity, spawnData);
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) {
        referencedPrefabs.Add(Prefab);
    }
}