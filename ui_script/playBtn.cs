using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class playBtn : MonoBehaviour
{
    public int missionSceneID = 1;
    public void Play(int id)
    {
        Cursor.lockState = CursorLockMode.Locked;
        PlayerPrefs.SetInt("missionID", id);
        SceneManager.LoadSceneAsync(missionSceneID);
    }
}
