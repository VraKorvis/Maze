using Unity.Entities;
using UnityEngine;

public struct TouchListener : IComponentData { }

[RequiresEntityConversion]
public class TouchListenerComponent : MonoBehaviour, IConvertGameObjectToEntity{
	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
		dstManager.AddComponentData(entity, new TouchListener());
	}
}