using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class loadBtn : MonoBehaviour
{
    public void Load(string slot)
    {
        SaveLoad.SetCurrentSaveSlot(slot);
        MainMenuManager.Instance.Setup();
    }
}
