using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricLines;

public class EnemySniper : MonoBehaviour
{
    public float atkRate = 1f;
    private float atkTimer = 0f;

    public float atkDamage = 1f;

    public Transform t_gunpoint;

    private EnemyStatus s_status;
    private EnemyController s_controller;

    public float atkFov = 20f;

    public Transform t_gunDisplay;

    public Transform t_spine;

    public Transform t_gunPos;

    public float chargeTime = 2f;

    public VolumetricLineBehavior s_laser;

    public GameObject pf_flash;

    public GameObject pf_line;

    public GameObject pf_hit;

    public AudioSettings a_shoot;
    public AudioSettings a_hit;

    void Awake()
    {
        s_status = GetComponentInParent<EnemyStatus>();
        s_controller = GetComponentInParent<EnemyController>();
    }
    // Start is called before the first frame update
    void Start()
    {
        offsetY = t_gunpoint.position.y - t_spine.position.y;
    }

    private float offsetY;

    // Update is called once per frame
    void Update()
    {
        if (s_controller != null && !s_controller.isDead)
        {
            atkTimer = Mathf.Clamp(atkTimer - Time.deltaTime * ComputeSpeedMultiplier(), 0, atkRate);

            if (s_controller.t_atkTarget != null)
            {
                //Debug.Log(-Helper.ComputeOffsetAngle(t_spine.position, transform.forward, offsetY, s_controller.t_atkTarget.GetComponent<TargetOffset>().GetRealOffset()) + "\n" + t_spine.localRotation.eulerAngles.x);
                float offsetAngle = Mathf.MoveTowardsAngle(t_spine.localRotation.eulerAngles.x, -Helper.ComputeOffsetAngle(t_spine.position, transform.forward, offsetY, s_controller.t_atkTarget.GetComponent<TargetOffset>().GetRealOffset()), Time.deltaTime * 30f);
                //Debug.Log(offsetAngle);
                t_spine.localRotation = Quaternion.Euler(-Helper.ComputeOffsetAngle(t_spine.position, transform.forward, offsetY, s_controller.t_atkTarget.GetComponent<TargetOffset>().GetRealOffset()), 0f, 0f);
                Vector3 targetDir = s_controller.t_atkTarget.GetComponent<TargetOffset>().GetRealOffset() - t_gunDisplay.position;
                //t_gunDisplay.rotation = Quaternion.LookRotation(Vector3.RotateTowards(t_gunDisplay.forward, new Vector3(t_gunDisplay.forward.x, targetDir.y * new Vector2(t_gunDisplay.forward.x, t_gunDisplay.forward.z).magnitude / new Vector2(targetDir.x, targetDir.z).magnitude, t_gunDisplay.forward.z), 100f, 0f));
                //t_gunDisplay.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, targetDir, 100f, 0f));
                //Vector3.RotateTowards(t_gun.forward, new Vector3(t_gun.forward.x, gunDir.y * new Vector2(t_gun.forward.x, t_gun.forward.z).magnitude / new Vector2(gunDir.x, gunDir.z).magnitude, t_gun.forward.z)
                if (Vector3.Angle(targetDir, t_gunpoint.forward) < atkFov)
                {
                    Atk();
                }
            }
        }
        t_gunDisplay.position = t_gunPos.position;
        t_gunDisplay.rotation = t_gunPos.rotation;
    }

    public void Atk()
    {
        RaycastHit hit;
        if (Helper.RaycastIgnore(t_gunpoint.position, t_gunpoint.forward, out hit, 100f, LayermaskReference.blocked, transform.root))
        {
            if (atkTimer > 0f) return;
            atkTimer = atkRate;
            StartCoroutine(Shoot());
        }
        //GameObject go = Instantiate(pf_hitEffect, transform.position, transform.rotation);
        //Destroy(go, 2f);
    }

    private IEnumerator Shoot()
    {
        float t = 0f;
        RaycastHit hit;
        s_laser.gameObject.SetActive(true);
        while (t < chargeTime)
        {
            if (s_controller == null || s_controller.isDead) yield break;
            float dist = 100f;
            if (Helper.RaycastIgnore(t_gunpoint.position, t_gunpoint.forward, out hit, Mathf.Infinity, LayermaskReference.blocked, transform.root))
            {
                dist = hit.distance;
                //Helper.DrawCube(hit.point, Vector3.one, Color.black);
            }
            s_laser.EndPos = new Vector3(0f, 0f, dist);
            s_laser.LineWidth = 0.02f * t / chargeTime;
            //s_laser.LineColor = new Color(s_laser.LineColor.r, s_laser.LineColor.g, s_laser.LineColor.b, t / chargeTime);
            t += Time.deltaTime;
            yield return null;
        }
        if (s_controller == null || s_controller.isDead) yield break;

        GameObject a = Instantiate(GameManager.instance.pf_audio, t_gunpoint);
        Helper.AudioInit(a.GetComponent<AudioSource>(), a_shoot);
        Destroy(a, 2f);

        s_laser.gameObject.SetActive(false);
        Debug.Log("EnemySniper: Atk");
        GameObject flash = Instantiate(pf_flash, t_gunpoint);
        GameObject line = Instantiate(pf_line);
        Destroy(flash, 3f);
        if (!Helper.RaycastIgnore(t_gunpoint.position, t_gunpoint.forward, out hit, Mathf.Infinity, LayermaskReference.blocked, transform.root))
        {
            line.GetComponent<LineRenderer>().SetPosition(0, t_gunPos.position);
            line.GetComponent<LineRenderer>().SetPosition(1, t_gunPos.position + t_gunPos.forward * 100f);
            yield break;
        }
        else
        {
            line.GetComponent<LineRenderer>().SetPosition(0, t_gunPos.position);
            line.GetComponent<LineRenderer>().SetPosition(1, hit.point);
            GameObject aa = Instantiate(GameManager.instance.pf_audio, hit.point, Quaternion.identity);
            Helper.AudioInit(aa.GetComponent<AudioSource>(), a_hit);
            Destroy(aa, 2f);
        }
        float damage = atkDamage;
        Debug.Log(hit.transform.root.name);
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
        Destroy(hitEffect, 3f);
    }

    private float ComputeSpeedMultiplier()
    {
        return (s_status.statusEffects[EnemyStatus.Status.Stun].Count > 0 && s_status.statusEffects[EnemyStatus.Status.Stun][0].timeLeft > s_status.statusEffects[EnemyStatus.Status.Stun][0].duration - s_status.statusEffects[EnemyStatus.Status.Stun][0].param) ? 0f : 1f;
    }
}
