using Unity.Entities;
using UnityEngine;

public class LeaderAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
    public GameObject followerObject;

    // private void Awake() {
    //     FollowToEntity followEntity = followerObject.GetComponent<FollowToEntity>();
    //
    //     if (followEntity == null) {
    //         followEntity = followerObject.AddComponent<FollowToEntity>();
    //     }
    //
    //     followEntity.entityToFollow = entity;
    // }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
//         FollowToEntity followEntity = followerObject.GetComponent<FollowToEntity>();
//         
//         if (followEntity == null) {
//             followEntity = followerObject.AddComponent<FollowToEntity>();
//             // dstManager.AddComponentData(followEntity.entityToFollow, new CameraFollowEntity());
//         }
//         
//         followEntity.entityToFollow = entity;
// //        Debug.Log("Converting ShamanEnt to entity...");
    }
}