using System;
using Unity.Entities;
using UnityEngine;

public struct ClickToTurn : IComponentData {
	public Clockwise Clockwise;
	public bool needCalculateAngle;
}

public class ClickToTurnComponent : MonoBehaviour, IConvertGameObjectToEntity {
	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
		dstManager.AddComponentData(entity, new ClickToTurn());
	}
}

[Serializable]
public enum Clockwise {
	Clock=-1,
	CounterClock=1
}