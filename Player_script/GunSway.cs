using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSway : MonoBehaviour
{

    public float multiplier;
    public float smooth;
    private float mouseSens;

    void Start()
    {
        mouseSens = SaveLoad.LoadSettings().mouseSens;
    }


    void Update()
    {
        float moveX = GameManager.instance.showingSettingsMenu ? 0f : -Input.GetAxis("Mouse X") * multiplier;
        Quaternion horizontal = Quaternion.AngleAxis(moveX, Vector3.up);

        Quaternion targetRotation = horizontal;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smooth * mouseSens / 10f / Mathf.PI);
    }

}
