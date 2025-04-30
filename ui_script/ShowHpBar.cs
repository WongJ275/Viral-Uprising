using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHpBar : MonoBehaviour
{
    void LateUpdate()
    {
        RaycastHit hit;
        if (Helper.RaycastIgnore(Camera.main.transform.position, Camera.main.transform.forward, out hit, 1000f, LayermaskReference.sightTarget))
        {
            HpBar s_hpBar = hit.transform.root.GetComponent<HpBar>();
            if (s_hpBar != null)
            {
                //Debug.Log(hit.transform.name);
                s_hpBar.Show();
                if (Helper.RaycastIgnore(Camera.main.transform.position, Camera.main.transform.forward, out hit, 1000f, LayermaskReference.sightTargetWithoutShield))
                {
                    s_hpBar = hit.transform.root.GetComponent<HpBar>();
                    if (s_hpBar != null)
                    {
                        //Debug.Log(hit.transform.name);
                        s_hpBar.Show();

                    }
                }
            }
        }
    }
}
