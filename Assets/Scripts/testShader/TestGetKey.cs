using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGetKey : MonoBehaviour
{
    
    public  Material waveMaterial;
    private float    waveSpread = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1)) {
           Debug.Log(KeyCode.Alpha1);
        }
        
        if (Input.GetKeyUp(KeyCode.Mouse0)) {
            Debug.Log(KeyCode.Mouse0);
            Vector2    clickPos = Input.mousePosition;
            Ray        ray      = Camera.main.ScreenPointToRay(clickPos);
            RaycastHit hit;
            Debug.Log(clickPos);
            if (Physics.Raycast(ray, out hit)) {
                Debug.Log("Found an object - distance: " + hit.distance);
                waveMaterial.SetVector("_ClickPos", hit.point);
                waveSpread = 0;
            }
        }

        waveMaterial.SetFloat("_WaveSpread", waveSpread);
        waveSpread += 0.1f;
    }
}
