using UnityEngine;
using System.Collections;
using Cinemachine;
using UnityEngine.UI;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraFollow : MonoBehaviour {

    [HideInInspector] private CinemachineVirtualCamera vCam;
    [HideInInspector] private Transform vCamTransform;

    [SerializeField] private AbstractPlayerController player;

    [SerializeField] private Slider camSmoothFollow;
    private float smoothCam;

    void Awake() {
        vCam = GetComponent<CinemachineVirtualCamera>();
        vCamTransform = vCam.GetComponent<Transform>();
    }
   
    void FixedUpdate() {
       // TrackingRotatePlayer();
    }

    private void TrackingRotatePlayer() {
        Quaternion tmpRot = player.rb2dTransform.rotation;
        smoothCam = camSmoothFollow.value;
        vCamTransform.rotation = Quaternion.Lerp(vCamTransform.rotation, tmpRot, smoothCam);
    }
}
