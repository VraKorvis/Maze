using System;
using Cinemachine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct TrackedByCamera : IComponentData {
    // public float smooth;
    //
    // public float offsetY;
}

[RequiresEntityConversion]
public class TrackedByCameraComponent : MonoBehaviour, IConvertGameObjectToEntity {
    [Range(0.1f, 1f)]
    public float smooth;

    [Range(0.0f, 7.0f)]
    private float offsetY;

    // [Header("Set CinemachineVirtualCamera")]
    // public CinemachineVirtualCamera vCam;
    private EntityManager em;

    public Entity entityToFollow;
    public float3 offset = new float3(0, 0, 0);

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        entityToFollow = entity;
        dstManager.AddComponentData(entity, new TrackedByCamera());
    }

    private void Start() {
        // em = World.DefaultGameObjectInjectionWorld.EntityManager;
        // var transposer = vCam.GetCinemachineComponent<CinemachineTransposer>();
        // offsetY = vCam.m_Lens.OrthographicSize * 0.25f;
        // transposer.m_FollowOffset = new Vector3(0, offsetY, -10);
    }

    private void LateUpdate() {
        // if (entityToFollow == Entity.Null || !em.Exists(entityToFollow)) {
        //     return;
        // }
        //
        // LocalToWorld entPos = em.GetComponentData<LocalToWorld>(entityToFollow);
        // Rotation entRot = em.GetComponentData<Rotation>(entityToFollow);
        // transform.position = entPos.Position;
        // transform.rotation = math.slerp(transform.rotation, entRot.Value, smooth);;
    }
}