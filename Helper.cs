using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using System.IO;
using System;

public static class Helper
{
    public static void DrawCube(Vector3 original, Vector3 size, Color color, float duration = 0.1f)
    {
        //Debug.Log("draw");
        Debug.DrawLine(original + new Vector3(-size.x / 2, -size.y / 2, -size.z / 2), original + new Vector3(size.x / 2, -size.y / 2, -size.z / 2), color, duration);
        Debug.DrawLine(original + new Vector3(-size.x / 2, -size.y / 2, -size.z / 2), original + new Vector3(-size.x / 2, -size.y / 2, size.z / 2), color, duration);
        Debug.DrawLine(original + new Vector3(size.x / 2, -size.y / 2, size.z / 2), original + new Vector3(size.x / 2, -size.y / 2, -size.z / 2), color, duration);
        Debug.DrawLine(original + new Vector3(size.x / 2, -size.y / 2, size.z / 2), original + new Vector3(-size.x / 2, -size.y / 2, size.z / 2), color, duration);

        Debug.DrawLine(original + new Vector3(-size.x / 2, size.y / 2, -size.z / 2), original + new Vector3(size.x / 2, size.y / 2, -size.z / 2), color, duration);
        Debug.DrawLine(original + new Vector3(-size.x / 2, size.y / 2, -size.z / 2), original + new Vector3(-size.x / 2, size.y / 2, size.z / 2), color, duration);
        Debug.DrawLine(original + new Vector3(size.x / 2, size.y / 2, size.z / 2), original + new Vector3(size.x / 2, size.y / 2, -size.z / 2), color, duration);
        Debug.DrawLine(original + new Vector3(size.x / 2, size.y / 2, size.z / 2), original + new Vector3(-size.x / 2, size.y / 2, size.z / 2), color, duration);

        Debug.DrawLine(original + new Vector3(-size.x / 2, -size.y / 2, -size.z / 2), original + new Vector3(-size.x / 2, size.y / 2, -size.z / 2), color, duration);
        Debug.DrawLine(original + new Vector3(size.x / 2, -size.y / 2, -size.z / 2), original + new Vector3(size.x / 2, size.y / 2, -size.z / 2), color, duration);
        Debug.DrawLine(original + new Vector3(-size.x / 2, -size.y / 2, size.z / 2), original + new Vector3(-size.x / 2, size.y / 2, size.z / 2), color, duration);
        Debug.DrawLine(original + new Vector3(size.x / 2, -size.y / 2, size.z / 2), original + new Vector3(size.x / 2, size.y / 2, size.z / 2), color, duration);

    }

    public static bool RaycastIgnore(Vector3 original, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask, params Transform[] ignored)
    {
        List<RaycastHit> hits = Physics.RaycastAll(original, direction, maxDistance, layerMask).ToList();
        hits.Sort((x, y) => Vector3.Distance(original, x.point).CompareTo(Vector3.Distance(original, y.point)));
        /*string s = "";
        for (int i = 0; i < hits.Count; i++)
        {
            s += hits[i].transform.name + Vector3.Distance(original, hits[i].point);
        }
        Debug.Log(s);*/
        for (int i = 0; i < hits.Count; i++)
        {
            if (ignored.Contains(hits[i].transform.root)) continue;
            if (Vector3.Distance(hits[i].point, original) > maxDistance) break;
            hitInfo = hits[i];
            return true;
        }
        hitInfo = default(RaycastHit);
        return false;
    }

    public static float ComputeOffsetAngle(Vector3 origin, Vector3 forward, float offsetY, Vector3 target)
    {
        Vector3 diff = target - origin;
        //Debug.Log((Vector3.Angle(forward + diff.y / new Vector2(diff.x, diff.z).magnitude * Vector3.up, forward) * (diff.y > 0f ? 1f : -1f)) + "\n" + Mathf.Asin(offsetY / diff.magnitude) * Mathf.Rad2Deg);
        return (Vector3.Angle(forward + diff.y / new Vector2(diff.x, diff.z).magnitude * Vector3.up, forward) * (diff.y > 0f ? 1f : -1f)) - Mathf.Asin(offsetY / diff.magnitude) * Mathf.Rad2Deg;
    }

    public static void AudioInit(AudioSource audioSource, AudioSettings audioSettings)
    {
        audioSource.clip = audioSettings.audioClip;
        audioSource.loop = audioSettings.loop;
        audioSource.volume = audioSettings.volume;
        audioSource.pitch = audioSettings.pitch;
        audioSource.Play();
    }

    public static void WriteLog(string message)
    {
#if UNITY_EDITOR
        Debug.Log(message);
        using (StreamWriter outputFile = new StreamWriter(Application.dataPath + "/log.txt"))
        {
            outputFile.WriteLine(DateTime.Now.ToString("HH:mm:ss tt") + " | " + message);
        }
        //File.WriteLine(DateTime.Now.ToString("HH:mm:ss tt") + " | " + Application.persistentDataPath + "/log.txt", message);
#else
        using (StreamWriter outputFile = new StreamWriter(Application.persistentDataPath + "/log.txt"))
        {
                outputFile.WriteLine(DateTime.Now.ToString("HH:mm:ss tt") + " | " + message);
        }
#endif
    }
}

[Serializable]
public class AudioSettings
{ 
    public AudioClip audioClip;
    public float volume = 1f;
    public bool loop;
    public float pitch = 1f;
}

