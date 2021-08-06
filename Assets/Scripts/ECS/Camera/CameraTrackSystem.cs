using System.Collections.Generic;
using Cinemachine;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;

[UpdateInGroup(typeof(AfterSimulationSystemGroup))]
// [UpdateInGroup(typeof(LateSimulationSystemGroup))]
// [DisableAutoCreation]
public class CameraTrackSystem : SystemBase {
    private EntityQuery cameraTrackGroup;
    private EntityQuery playerGroup;

    // private readonly List<CameraTrack> m_UniqueTypes = new List<CameraTrack>();

    protected override void OnCreate() {
        //  Enabled = SceneManager.GetActiveScene().name.StartsWith("maze");
        cameraTrackGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadWrite<CameraTrack>()
            },
            Options = EntityQueryOptions.FilterWriteGroup
        });
        playerGroup = GetEntityQuery(ComponentType.ReadOnly<PlayerData>());
        RequireForUpdate(playerGroup);
        RequireForUpdate(cameraTrackGroup);
    }

    protected override void OnUpdate() {
        //            EntityManager.GetAllUniqueSharedComponentData(m_UniqueTypes);

        //            for (int typeIndex = 1; typeIndex < m_UniqueTypes.Count; typeIndex++) {
        //                Debug.Log(m_UniqueTypes.Count);
        //                var settings = m_UniqueTypes[typeIndex];
        //                cameraTrackGroup.SetFilter(settings);
        //
        //                var vcam      = m_UniqueTypes[typeIndex].Value;
        //                
        //                var transform = vcam.Follow;
        //                var nPos      = em.GetComponentData<Translation>(trackedEntity).Value;
        //                var nRot      = em.GetComponentData<Rotation>(trackedEntity).Value;
        //                var smooth    = settings.smooth;
        //                transform.position = nPos;
        //                transform.rotation = math.slerp(transform.rotation, nRot, smooth);
        //                vcam.Follow        = transform;
        //                Debug.Log(nPos);
        //                Debug.Log(transform.position);
        //            }
        //        m_UniqueTypes.Clear();

        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        var trackedEntity = playerGroup.GetSingletonEntity();
        var vcamEntity = cameraTrackGroup.GetSingletonEntity();
        var cameraTrack = em.GetComponentData<CameraTrack>(vcamEntity);
        var vcam = em.GetComponentObject<CinemachineVirtualCamera>(vcamEntity);

        //var vcam = cameraTrack.Value;
        var transform = vcam.Follow;
        var nPos = em.GetComponentData<Translation>(trackedEntity).Value;
        var nRot = em.GetComponentData<Rotation>(trackedEntity).Value;
        var smooth = cameraTrack.smooth;
        transform.position = nPos;
        transform.rotation = math.slerp(transform.rotation, nRot, smooth);
        //        Debug.Log(nRot);
        //        Debug.Log(transform.rotation);
        vcam.Follow = transform;
    }
}