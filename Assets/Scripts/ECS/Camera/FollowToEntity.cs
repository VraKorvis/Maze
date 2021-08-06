using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class FollowToEntity : MonoBehaviour {
    public Entity entityToFollow;
    public float3 offset = new float3(0, 0, 0);

    private EntityManager em;

    private void Start() {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void LateUpdate() {
        // if (!em.Exists(entityToFollow) || entityToFollow == Entity.Null) {
        //     return;
        // }
        //
        // Translation entPos = em.GetComponentData<Translation>(entityToFollow);
        // Rotation entRot = em.GetComponentData<Rotation>(entityToFollow);
        // var transform1 = transform;
        // transform1.position = entPos.Value + offset;
        // transform1.rotation = entRot.Value;
    }
}