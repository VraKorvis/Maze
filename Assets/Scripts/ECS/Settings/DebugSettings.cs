using System;
using UnityEngine;
using UnityEngine.UI;

public class DebugSettings : MonoBehaviour {
    public static DebugSettings Instance;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
    }

    public Toggle blockToggle;
    private bool blockSpam;
    public bool BlockSpam => blockToggle.isOn;

    // TODO GameSettings feature
//    [Range(0.1f, 1f)]
//    [Tooltip("Slowdown speed when turn. During the turn: speed = speed * motionCoef")]
//    public float motionCoef;
//
//    [Space]
//    [Header("Just for test.Settings")]
//    [SerializeField]
//    private Slider slowDownSpeed;
//
//    [SerializeField]
//    private Slider characterSpeedSlider;
//
//    [SerializeField]
//    private Slider rotateSpeedSlider;
//    
//
//    [SerializeField]
//    private Slider timeCountSLider;
//
//    [SerializeField]
//    private float timeCountForTurn = 0.5f;
}