using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ShadowRotate : MonoBehaviour {
    public Tilemap tm1;
    public Tilemap tm2;

    public Camera cam;

    private Transform camTransform;
//    private float     defRot = 45f;

    private void Awake() {
        camTransform = cam.GetComponent<Transform>();
    }

    void Update() {
//        var camRot = camTransform.rotation.eulerAngles;
//        var matrix = tm1.orientationMatrix;
//        var newRot = matrix.rotation;
//        var ang    = camRot.z;
//       
//        if (ang <= 90 || ang >= 270) {
//            if (ang >= 180) {
//                ang -= 180;
//            }
//
//            
//            var zRot = camRot.z / 2 + defRot;
//            newRot = Quaternion.Euler(zRot, 0, 0);
//            matrix.SetTRS(new Vector3(0, -0.5f, 0), newRot, Vector3.one);
//            tm1.orientationMatrix = matrix;
//        }
    }
}