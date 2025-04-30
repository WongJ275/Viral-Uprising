using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunTurret : BuildScript
{
    public float damage = 1f;
    public float cooldown = 0.5f;
    private bool onCooldown = false;
    private Turret s_turret;
    public GameObject pf_trail;

    public Transform t_gunpoint;

    public AudioSettings a_shoot;
    public AudioSettings a_hit;

    void Awake()
    {
        s_turret = transform.root.GetComponent<Turret>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!dead && !onCooldown && s_turret.targetInSight)
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        onCooldown = true;
        RaycastHit hit;
        while (!Helper.RaycastIgnore(t_gunpoint.position, t_gunpoint.forward, out hit, Mathf.Infinity, LayermaskReference.buildingDetect, transform.root))
        {
            yield return null;
        }
        GameObject go = Instantiate(pf_trail, t_gunpoint.position, Quaternion.identity);
        go.GetComponent<LineRenderer>().SetPosition(0, Vector3.zero);
        go.GetComponent<LineRenderer>().SetPosition(1, hit.point - t_gunpoint.position);
        Destroy(go, 0.05f);
        if (hit.transform != null && hit.transform.CompareTag("Enemy"))
        {
            hit.transform.root.GetComponent<EnemyController>().ChangeHealth((int)(-damage), transform.root);
        }

        GameObject a = Instantiate(GameManager.instance.pf_audio, transform.position, Quaternion.identity);
        Helper.AudioInit(a.GetComponent<AudioSource>(), a_shoot);
        Destroy(a, 2f);

        GameObject aa = Instantiate(GameManager.instance.pf_audio, hit.point, Quaternion.identity);
        Helper.AudioInit(aa.GetComponent<AudioSource>(), a_hit);
        Destroy(aa, 2f);
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }
}
