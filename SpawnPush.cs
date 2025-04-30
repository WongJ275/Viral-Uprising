using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPush : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider other)
    {
        Debug.Log(other.transform.root.gameObject.name);
        Rigidbody rb = other.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.velocity = transform.forward * 100f;
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log(other.transform.root.gameObject.name);
        Rigidbody rb = other.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
    }
}
