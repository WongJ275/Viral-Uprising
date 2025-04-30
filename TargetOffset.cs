using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetOffset : MonoBehaviour
{
    public Vector3 Offset;
    public Vector3 GetRealOffset()
    {
        return transform.position + transform.right * Offset.x + transform.up * Offset.y + transform.forward * Offset.z;
    }

    void OnDrawGizmos()
    {
        Helper.DrawCube(GetRealOffset(), Vector3.one * 0.2f, Color.magenta);
    }
}
