using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSelector : MonoBehaviour
{

    public int selectedBuilding = 0;
    public int previousSelectedBuilding = -1;

    public AudioSource selectSound;

    


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Q)) {
            previousSelectedBuilding = selectedBuilding;
            selectedBuilding = (selectedBuilding - 1 + transform.childCount) % transform.childCount;
            selectSound.Play();
        }
        else if (Input.GetKeyDown(KeyCode.E)) {
            previousSelectedBuilding = selectedBuilding;
            selectedBuilding = (selectedBuilding + 1) % transform.childCount;
            selectSound.Play();
        }


    }

}
