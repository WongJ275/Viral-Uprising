using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameManager : MonoBehaviour
{
    public int targetFrameRate = 120;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = SaveLoad.LoadSettings().maxFrameRate;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
