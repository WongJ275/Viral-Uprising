using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class EnemyGun : MonoBehaviour
{
    public float atkRate = 1f;
    private float atkTimer = 0f;

    public float atkCost = 2f;
    public float atkDamage = 1f;
    public float maxCost = 10f;
    public float atkThreashold = 0.7f;
    public float curCost = 0f;

    private bool initAtk = false;

    public GameObject pf_bullet;

    public Transform t_gunpoint;

    private EnemyStatus s_status;
    private EnemyController s_controller;

    public float atkFov = 30f;

    public Transform t_gunDisplay;
    public GameObject go_gunDisplay;
    public Transform t_chest;
    public Transform t_rotateRef;

    public GameObject pf_flash;

    public AudioSettings shootAudio;

    void Awake()
    {
        curCost = maxCost;
        s_status = GetComponentInParent<EnemyStatus>();
        s_controller = GetComponentInParent<EnemyController>();
    }
    // Start is called before the first frame update
    void Start()
    {
        t_rotateRef.rotation = t_chest.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (s_controller != null && !s_controller.isDead)
        {
            curCost = Mathf.Clamp(curCost + Time.deltaTime * ComputeSpeedMultiplier(), 0, maxCost);
            atkTimer = Mathf.Clamp(atkTimer - Time.deltaTime * ComputeSpeedMultiplier(), 0, atkRate);

            if (s_controller.t_atkTarget != null)
            {
                Vector3 targetDir = s_controller.t_atkTarget.GetComponent<TargetOffset>().GetRealOffset() - transform.position;
                transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, new Vector3(transform.forward.x, targetDir.y * new Vector2(transform.forward.x, transform.forward.z).magnitude / new Vector2(targetDir.x, targetDir.z).magnitude, transform.forward.z), 100f, 0f));
                //Vector3.RotateTowards(t_gun.forward, new Vector3(t_gun.forward.x, gunDir.y * new Vector2(t_gun.forward.x, t_gun.forward.z).magnitude / new Vector2(gunDir.x, gunDir.z).magnitude, t_gun.forward.z)
                if (Vector3.Angle(targetDir, transform.forward) < atkFov)
                {
                    Atk();
                    //Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, LayermaskReference.blocked);
                    //if (hit.collider != null /*&& hit.collider.transform.root.CompareTag("Player")*/)
                    //{
                    //    Atk();
                    //}
                }
            }
            t_chest.rotation = t_rotateRef.rotation;
        }

        go_gunDisplay.transform.position = t_gunDisplay.transform.position;
        go_gunDisplay.transform.rotation = t_gunDisplay.transform.rotation;
    }

    public void Atk()
    {
        if (atkTimer > 0f || curCost < atkCost) return;
        if (curCost > maxCost * atkThreashold && initAtk == false) initAtk = true; curCost += maxCost * (1f - atkThreashold) * UnityEngine.Random.Range(0.2f, 0.5f);
        if (!initAtk) return;
        curCost -= atkCost;
        atkTimer = atkRate;
        Debug.Log("EnemyGun: Atk");
        Debug.DrawRay(t_gunpoint.position, transform.forward * 100, Color.red, 2f);
        GameObject bullet = Instantiate(pf_bullet, t_gunpoint.position, transform.rotation);
        bullet.GetComponent<EnemyBullet>().Init(transform.root);
        GameObject flash = Instantiate(pf_flash, t_gunpoint.position, transform.rotation);
        Destroy(flash, 1f);
        if (curCost < atkCost) initAtk = false; curCost = maxCost * atkThreashold * UnityEngine.Random.Range(0.2f, 0.5f);

        GameObject a = Instantiate(GameManager.instance.pf_audio, t_gunpoint);
        Helper.AudioInit(a.GetComponent<AudioSource>(), shootAudio);
        Destroy(a, 2f);

    }

    private float ComputeSpeedMultiplier()
    {
        return (s_status.statusEffects[EnemyStatus.Status.Stun].Count > 0 && s_status.statusEffects[EnemyStatus.Status.Stun][0].timeLeft > s_status.statusEffects[EnemyStatus.Status.Stun][0].duration - s_status.statusEffects[EnemyStatus.Status.Stun][0].param) ? 0f : 1f;
    }
}
