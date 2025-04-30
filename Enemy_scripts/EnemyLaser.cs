using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricLines;

public class EnemyLaser : MonoBehaviour
{
    public float atkRate = 1f;
    private float atkTimer = 0f;

    public float atkDamage = 1f;

    public Transform t_gunpoint;

    private EnemyStatus s_status;
    private EnemyController s_controller;

    public float atkFov = 20f;

    public Transform t_gunDisplay;

    public AnimationCurve ac_damageCharge;
    public Gradient g_colorCharge;
    public float maxCharge = 5f;
    public float chargeTimer = 0f;
    public float loseChargeRate = 3f;

    private VolumetricLineBehavior s_laser;
    private GameObject go_laser;

    public float maxRange = 5f;

    public GameObject go_flash;
    public GameObject pf_hit;

    public AudioSettings a_hit;

    public float laserVolume = 0.3f;

    void Awake()
    {
        s_status = GetComponentInParent<EnemyStatus>();
        s_controller = GetComponentInParent<EnemyController>();
        s_laser = GetComponentInChildren<VolumetricLineBehavior>();
        go_laser = s_laser.gameObject;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (s_controller != null && !s_controller.isDead)
        {
            atkTimer = Mathf.Clamp(atkTimer - Time.deltaTime * ComputeSpeedMultiplier(), 0, atkRate);

            if (s_controller.t_atkTarget != null)
            {
                Vector3 targetDir = s_controller.t_atkTarget.GetComponent<TargetOffset>().GetRealOffset() - t_gunDisplay.position;
                t_gunDisplay.rotation = Quaternion.LookRotation(Vector3.RotateTowards(t_gunDisplay.forward, new Vector3(t_gunDisplay.forward.x, targetDir.y * new Vector2(t_gunDisplay.forward.x, t_gunDisplay.forward.z).magnitude / new Vector2(targetDir.x, targetDir.z).magnitude, t_gunDisplay.forward.z), 100f, 0f));
                //t_gunDisplay.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, targetDir, 100f, 0f));
                //Vector3.RotateTowards(t_gun.forward, new Vector3(t_gun.forward.x, gunDir.y * new Vector2(t_gun.forward.x, t_gun.forward.z).magnitude / new Vector2(gunDir.x, gunDir.z).magnitude, t_gun.forward.z)
                if (Vector3.Angle(targetDir, transform.forward) < atkFov && targetDir.magnitude < maxRange)
                {
                    Atk();
                    chargeTimer = Mathf.Clamp(chargeTimer + Time.deltaTime * ComputeSpeedMultiplier(), 0f, maxCharge);
                    //Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, LayermaskReference.blocked);
                    //if (hit.collider != null /*&& hit.collider.transform.root.CompareTag("Player")*/)
                    //{
                    //    Atk();
                    //}
                    GetComponent<AudioSource>().volume = chargeTimer / maxCharge * laserVolume;
                }
                else
                {
                    chargeTimer = Mathf.Clamp(chargeTimer - Time.deltaTime * ComputeSpeedMultiplier() * loseChargeRate, 0f, maxCharge);
                    if (go_laser.activeInHierarchy) { go_laser.SetActive(false); go_flash.SetActive(false); }
                }
            }
            else
            {
                chargeTimer = Mathf.Clamp(chargeTimer - Time.deltaTime * ComputeSpeedMultiplier() * loseChargeRate, 0f, maxCharge);
                if (go_laser.activeInHierarchy) { go_laser.SetActive(false); go_flash.SetActive(false); }
            }
        }
        else
        {
            GetComponent<AudioSource>().volume = 0f;
        }
    }

    public void Atk()
    {
        RaycastHit hit;
        if (Helper.RaycastIgnore(t_gunpoint.position, t_gunpoint.forward, out hit, 100f, LayermaskReference.blocked, transform.root))
        {
            if (!go_laser.activeInHierarchy) { go_laser.SetActive(true); }
            if (!go_flash.activeInHierarchy) { go_flash.SetActive(true); }
            s_laser.EndPos = new Vector3(0f, 0f, Vector3.Distance(hit.point, t_gunpoint.position));
            s_laser.LineColor = g_colorCharge.Evaluate(chargeTimer / maxCharge);
            if (atkTimer > 0f) return;

            atkTimer = atkRate;
            Debug.Log("EnemyLaser: Atk");

            float damage = atkDamage * ac_damageCharge.Evaluate(chargeTimer / maxCharge);
            if (hit.transform.root.CompareTag("Player"))
            {
                //Debug.Log("Bullet Hit Player: " + damage);
                hit.transform.root.GetComponent<PlayerHealth>().TakeDamage((int)damage);
            }
            else if (hit.transform.root.CompareTag("BreakableBuilding"))
            {
                //Debug.Log("Bullet Hit Building: " + damage);
                hit.transform.root.GetComponent<BuildingHealth>().TakeDamage((int)damage);
            }
            else if (hit.transform.root.CompareTag("Base"))
            {
                //Debug.Log("Bullet Hit Base: " + damage);
                hit.transform.root.GetComponent<BaseController>().ChangeHealth((int)(-damage));
            }

            GameObject hitEffect = Instantiate(pf_hit, hit.point, Quaternion.identity);
            GameObject a = Instantiate(GameManager.instance.pf_audio, hit.point, Quaternion.identity);
            Helper.AudioInit(a.GetComponent<AudioSource>(), a_hit);
            Destroy(a, 10f);
            Destroy(hitEffect, 3f);
        }
        else
        {
            if (go_laser.activeInHierarchy) { go_laser.SetActive(false); }
        }
        //GameObject go = Instantiate(pf_hitEffect, transform.position, transform.rotation);
        //Destroy(go, 2f);
    }

    private float ComputeSpeedMultiplier()
    {
        return (s_status.statusEffects[EnemyStatus.Status.Stun].Count > 0 && s_status.statusEffects[EnemyStatus.Status.Stun][0].timeLeft > s_status.statusEffects[EnemyStatus.Status.Stun][0].duration - s_status.statusEffects[EnemyStatus.Status.Stun][0].param) ? 0f : 1f;
    }
}
