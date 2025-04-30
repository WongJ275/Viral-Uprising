using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Turret : BuildScript
{
    public float detectRadius = 5f;
    public float rotateSpeed = 5f;
    public float atkRadius = 6f;

    public Transform target = null;

    public Transform t_pillar;
    public Transform t_gun;
    public Transform t_gunpoint;

    public bool targetInSight = false;
    private float targetOutSightTimer = 0f;
    public float targetOutSightTime = 3f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            targetInSight = false;
            Collider[] cols = Physics.OverlapSphere(transform.position, detectRadius, LayermaskReference.enemy);
            if (cols.Length > 0)
            {
                Collider result = null;
                foreach (var c in cols)
                {
                    
                    RaycastHit hit;
                    if (!Helper.RaycastIgnore(t_gun.position, c.ClosestPoint(t_gun.position) - t_gun.position, out hit, Mathf.Infinity, LayermaskReference.buildingDetect, transform.root)) continue;
                    /*List<RaycastHit> hits = Physics.RaycastAll().ToList();
                    hits.Sort((x, y) => Vector3.Distance(t_gun.position, x.point).CompareTo(Vector3.Distance(t_gun.position, y.point)));
                    for (int i=0; i<hits.Count; i++)
                    {
                        if (Vector3.Distance(hits[i].transform.root.position, transform.root.position) > detectRadius) break;
                        if (hits[i].transform.root != transform.root)
                        {
                            t_hit = hits[i].transform.root;
                            break;
                        }
                    }*/
                    Transform t_hit = hit.transform.root;

                    if (t_hit == c.transform.root)
                    {
                        Debug.Log(t_hit.gameObject.name);
                        if (result == null || Vector3.Distance(transform.position, c.ClosestPoint(transform.position)) < Vector3.Distance(transform.position, result.ClosestPoint(transform.position)))
                        {
                            result = c;
                        }
                    }
                }
                if (result != null)
                {
                    target = result.transform.root;
                }
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, target.position) > atkRadius && Vector3.Distance(transform.position, target.position) > detectRadius)
            {
                target = null;
            }
            else
            {
                Debug.DrawRay(t_gun.position, t_gun.forward * 100f, Color.blue);
                RaycastHit hit;
                if (Physics.Raycast(t_gunpoint.position, t_gunpoint.forward, out hit, Mathf.Infinity, LayermaskReference.buildingDetect) && hit.transform.root == target.transform.root)
                {
                    targetInSight = true;
                    targetOutSightTimer = 0f;
                }
                else
                {
                    targetOutSightTimer += Time.deltaTime;
                    if (targetOutSightTimer >= targetOutSightTime)
                    {
                        targetInSight = false;
                        targetOutSightTimer = 0f;
                    }
                }
                if (targetInSight)
                {

                }
                Vector3 pillarDir = target.GetComponent<TargetOffset>().GetRealOffset() - t_pillar.position;
                t_pillar.rotation = Quaternion.LookRotation(Vector3.RotateTowards(t_pillar.forward, new Vector3(pillarDir.x, 0f, pillarDir.z), rotateSpeed * Time.deltaTime, 0.0f));

                Vector3 gunDir = target.GetComponent<TargetOffset>().GetRealOffset() - t_gun.position;
                //Debug.Log(gunDir);
                t_gun.rotation = Quaternion.LookRotation(Vector3.RotateTowards(t_gun.forward, new Vector3(t_gun.forward.x, gunDir.y * new Vector2(t_gun.forward.x, t_gun.forward.z).magnitude / new Vector2(gunDir.x, gunDir.z).magnitude, t_gun.forward.z), rotateSpeed * Time.deltaTime, 0.0f));
            }
            if (target != null && (target.GetComponent<EnemyController>() == null || target.GetComponent<EnemyController>().isDead)) target = null;
        }
    }
}
