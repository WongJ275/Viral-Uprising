using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LineFade : MonoBehaviour
{
    public float fadeRate = 2f;
    private LineRenderer lr;

    private float t = 1f;
    private float w;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        w = lr.widthMultiplier;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (t > 0f)
        {
            t -= Time.deltaTime * fadeRate;
            /*GradientAlphaKey[] aks = alphakeys.Select(x => x).ToArray();
            for (int i = 0; i < aks.Length; i++)
            {
                aks[i].alpha = alphakeys[i].alpha * t;
            }
            lr.colorGradient.SetKeys(colorKeys, aks);*/
            lr.endColor = new Color(lr.endColor.r, lr.endColor.g, lr.endColor.b, t);
            lr.widthMultiplier = t * w;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
