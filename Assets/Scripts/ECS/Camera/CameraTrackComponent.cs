using System;
using System.Collections.Generic;
using Cinemachine;
using Unity.Entities;
using UnityEngine;

public struct CameraTrack : IComponentData {
    public float smooth;
}

[RequiresEntityConversion]
public class CameraTrackComponent : MonoBehaviour, IConvertGameObjectToEntity {
    [Range(0.1f, 1f)] public float smooth;

    [Range(0.0f, 7.0f)] public float offsetY;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        var vcam = GetComponent<CinemachineVirtualCamera>();

        dstManager.AddComponentData(entity, new CameraTrack() {
            //   Value   = vcam,
            smooth = smooth,
        });

        var transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
        offsetY = vcam.m_Lens.OrthographicSize * 0.25f;
        transposer.m_FollowOffset = new Vector3(0, offsetY, -10);
        // conversionSystem.AddHybridComponent(vcam);
        dstManager.AddComponentObject(entity, vcam);
    }
}