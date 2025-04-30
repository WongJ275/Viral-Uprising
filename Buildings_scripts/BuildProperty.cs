using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildProperty : MonoBehaviour
{
    public int price = 10;
    private List<Mat> mats;
    public List<Material> m_holo;

    public Platform s_platform;

    void Awake()
    {
        mats = new List<Mat>();
        RecordMat();
    }

    private void RecordMat()
    {
        foreach (var mr in GetComponentsInChildren<MeshRenderer>())
        {
            mats.Add(new Mat(mr, mr.materials));
        }
    }

    public void SetMat(List<Material> materials)
    {
        foreach (var mat in mats)
        {
            mat.mr.SetMaterials(materials);
        }
    }

    public void RestoreMat()
    {
        foreach(var mat in mats)
        {
            mat.mr.SetMaterials(mat.materials);
        }
    }

    public void Init(Platform s_platform)
    {
        this.s_platform = s_platform;
    }

    private class Mat
    {
        public MeshRenderer mr;
        public List<Material> materials = new List<Material>();
        public Mat(MeshRenderer mr, Material[] materials)
        {
            this.mr = mr;
            this.materials = new List<Material>();
            foreach (var m in materials)
            {
                this.materials.Add(m);
            }  
        }
    }
}
