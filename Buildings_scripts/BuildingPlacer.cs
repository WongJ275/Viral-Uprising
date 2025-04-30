using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    public GameObject buildingPreview, selectedBuilding;
    public int selectedBuildingID, previousSelectedBuildingID = 0;
    public RaycastHit hit;
    public Camera playerCam;
    public Platform platform;
    public bool displaying;
    public AudioSource buildSound;

    public List<Material> m_holo;

    private GameObject[] go_buildables;

    public AudioSettings a_noMoneyCry;

    // Start is called before the first frame update
    void Start()
    {
        //destroy buildings that is not equipped
        if (!SaveLoad.initiated)
        {
            SaveLoad.Initiate();
        }
        List<int> buildingsEquipped = SaveLoad.currentSaveData.buildingsBought;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (!buildingsEquipped.Contains(i))
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        go_buildables = GameObject.FindGameObjectsWithTag("Buildable");
    }

    // Update is called once per frame
    void Update()
    {
        

        selectedBuildingID = transform.GetComponent<BuildingSelector>().selectedBuilding;
        selectedBuilding = transform.GetChild(selectedBuildingID).gameObject;

        if (previousSelectedBuildingID != selectedBuildingID && displaying == true)
        {
            DestroyBuildingPreview();
            previousSelectedBuildingID = selectedBuildingID;
            DisplayBuilding();
        }


        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit)) 
        {
            if (hit.collider.tag == "Buildable" && hit.distance < 15f) {

                platform = hit.collider.GetComponent<Platform>();

                if (platform.occupied == false && displaying == false)   
                {
                    DisplayBuilding();
                }
                else if (platform.occupied == true && displaying == true)
                {
                    DestroyBuildingPreview();
                }
                else if (platform.occupied == false && displaying == true)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0) && !GameManager.instance.showingSettingsMenu)
                    {
                        if (GameCoinsManager.instance.totalCoins >= selectedBuilding.GetComponent<BuildProperty>().price)
                        {
                            GameCoinsManager.instance.totalCoins -= selectedBuilding.GetComponent<BuildProperty>().price;
                            PlaceBuilding();
                        }
                        else
                        {
                            GameObject a = Instantiate(GameManager.instance.pf_audio, transform.position, Quaternion.identity);
                            Helper.AudioInit(a.GetComponent<AudioSource>(), a_noMoneyCry);
                            Destroy(a, 2f);
                        }
                    }
                }
            }
            else 
            {
                DestroyBuildingPreview();
                for (int i = 0; i < go_buildables.Length; i++)
                {
                    go_buildables[i].transform.GetChild(0).gameObject.SetActive(!go_buildables[i].GetComponent<Platform>().occupied);
                    go_buildables[i].transform.GetChild(1).gameObject.SetActive(false);
                }
            }
            
        }
        
    }

    void DisplayBuilding()
    {
        hit.collider.transform.GetChild(1).gameObject.SetActive(true);

        Vector3 buildingPos = new Vector3(hit.collider.transform.GetComponent<Renderer>().bounds.center.x, hit.collider.transform.GetComponent<Renderer>().bounds.center.y, hit.collider.transform.GetComponent<Renderer>().bounds.center.z);
        buildingPreview = Instantiate(selectedBuilding, buildingPos, hit.collider.transform.rotation);

        foreach (Collider col in buildingPreview.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
        foreach (BuildScript bs in buildingPreview.transform.root.GetComponentsInChildren<BuildScript>())
        {
            bs.enabled = false;
        }
        foreach (HpBar hpBar in buildingPreview.transform.root.GetComponentsInChildren<HpBar>())
        {
            hpBar.enabled = false;
        }
        buildingPreview.SetActive(true);
        buildingPreview.transform.localScale = selectedBuilding.transform.lossyScale;
        buildingPreview.GetComponent<BuildProperty>().SetMat(m_holo);
        displaying = true;
    }

    void PlaceBuilding()
    {
        platform.occupied = true;
        DestroyBuildingPreview();
        Vector3 buildingPos = new Vector3(hit.collider.transform.GetComponent<Renderer>().bounds.center.x, hit.collider.transform.GetComponent<Renderer>().bounds.center.y, hit.collider.transform.GetComponent<Renderer>().bounds.center.z);
        GameObject built = Instantiate(selectedBuilding, buildingPos, hit.collider.transform.rotation);
        built.transform.localScale = selectedBuilding.transform.lossyScale;
        built.SetActive(true);
        built.GetComponent<BuildProperty>().Init(platform);
        built.GetComponent<BuildProperty>().RestoreMat();

        buildSound.Play();
    }

    public void DestroyBuildingPreview()
    {
        if (buildingPreview != null)
        {
            Destroy(buildingPreview);
            displaying = false;
        }
    }
}
