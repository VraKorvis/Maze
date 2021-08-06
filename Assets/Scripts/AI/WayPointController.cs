using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointController : MonoBehaviour {

    public static WayPointController instance;
    [SerializeField] public LineRenderer[] wayIsland;
    [SerializeField] public LineRenderer[] waySimple;

    // Use this for initialization
    void Awake () {
	    instance = this;
	}
	

}
