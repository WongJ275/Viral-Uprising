using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GunPosition : MonoBehaviour
{

    public GameObject gunPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = gunPosition.transform.position;
        transform.rotation = gunPosition.transform.rotation;
    }
}
