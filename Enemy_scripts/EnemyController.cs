using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;
using Unity.Mathematics;

public class EnemyController : MonoBehaviour
{
    private Rigidbody rb;
    private EnemyStatus s_status;
    [SerializeField]
    private Transform t_player;
    [SerializeField]
    private Transform t_station;
    [SerializeField]
    private Transform t_currentTarget;
    [SerializeField]
    private Transform t_obstacle;
    private List<Transform> t_targets = new List<Transform>();
    private EnemyGun s_gun;

    private UIScript uiScript;

    public float move_speed = 2f;
    public float rot_speed = 0.5f;

    public float detectFov = 90f;
    public float detectDist = 15f;
    public float atkFov = 30f;


    //state: move towards, move away, attack

    public AnimationCurve lock_AC;
    public float lockLossRate = 0.1f;
    public float lockOffset = 10f;
    public float atkDist = 5f;
    public float atkStopDist = 10f;

    public int maxHealth = 10;
    public float curHealth;

    public bool isDead = false;

    [Tooltip("range of possible coin drop, both included")]
    public Vector2Int coinDrop;

    [SerializeField]
    private NavMeshAgent groundAgent;
    [SerializeField]
    private NavMeshAgent groundObstaclesAgent;
    private Vector3 extend;

    public float obstacleDetectionDistance = 5f;

    public Transform t_atkTarget;

    [Header("Aggro")]
    public List<Aggro> aggros = new List<Aggro>();
    public AnimationCurve aggroSightCurve;
    public AnimationCurve aggroAtkCurve;
    public float aggroSightPlayerMultiplier = 2f;
    public float aggroAtkPlayerMultiplier = 2f;
    public float maxAggro = 30f;
    public float aggroSetTargetThreshold = 20f;
    public float aggroLoseTargetThreshold = 10f;
    public float aggroLossRate = 1f;
    public float aggroTargetBoost = 10f;
    public float aggroObstacleLossRate = 5f;
    public float pathRerouteDistance = 0f;

    

    public Vector3[] corners;
    public Vector3[] corners_;

    public float speedMultiplier;

    public float walkCutoff = 0.2f;
    private Animator animator;

    public AudioSettings audioDead;

    public enum EnemyState
    {
        Forward,
        Backward,
        Attack
    }

    public void Init(NavMeshAgent navMeshAgent)
    {
        groundAgent = navMeshAgent;
    }
    

    void Awake()
    {
        s_status = GetComponent<EnemyStatus>();
        s_gun = GetComponentInChildren<EnemyGun>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        curHealth = maxHealth;
        //groundAgent = GetComponentsInChildren<NavMeshAgent>().Aggregate((x,y)=>x=y.transform!=transform?y:x);
        groundObstaclesAgent = GetComponent<NavMeshAgent>();
        extend = GetComponent<Collider>().bounds.extents;
        //groundAgent.avoidancePriority = UnityEngine.Random.Range(1, 99);
        //groundAgent.radius = UnityEngine.Random.Range(GetComponent<Collider>().bounds.extents.x, GetComponent<Collider>().bounds.extents.x * 1.5f);
        //groundObstaclesAgent.avoidancePriority = UnityEngine.Random.Range(1, 99);
        groundObstaclesAgent.radius = UnityEngine.Random.Range(GetComponent<Collider>().bounds.extents.x * 1.1f, GetComponent<Collider>().bounds.extents.x * 1.5f);
        uiScript = GameObject.Find("In-game UI").GetComponent<UIScript>();
    }

    // Start is called before the first frame update
    void Start()
    {
        t_player = GameObject.FindGameObjectWithTag("Player").transform;
        t_station = GameObject.FindGameObjectWithTag("Base").transform;
        t_currentTarget = t_station;
        aggros.Add(new Aggro { aggro = 0f, target = t_player });

        //StartCoroutine(RebootIsDaWae());
    }

    void FixedUpdate()
    {
        /*rb.velocity = (t_currentTarget.position - transform.position).normalized * move_speed * ComputeSpeedMultiplier();*/
    }

    private float ComputeSpeedMultiplier()
    {
        float speed = s_status.statusEffects[EnemyStatus.Status.Slow].Select(x => x.param).Aggregate(1f, (x, y) => x * y) *
            ((s_status.statusEffects[EnemyStatus.Status.Stun].Count > 0 && s_status.statusEffects[EnemyStatus.Status.Stun][0].timeLeft > s_status.statusEffects[EnemyStatus.Status.Stun][0].duration - s_status.statusEffects[EnemyStatus.Status.Stun][0].param) ? 0f : 1f);
        //Debug.Log(s_status.statusEffects[EnemyStatus.Status.Stun].Count > 0 && s_status.statusEffects[EnemyStatus.Status.Stun][0].timeLeft > s_status.statusEffects[EnemyStatus.Status.Stun][0].duration - s_status.statusEffects[EnemyStatus.Status.Stun][0].param);
        return speed;
    }

    // Update is called once per frame
    void Update()
    {
        speedMultiplier = ComputeSpeedMultiplier();
        if (t_station == null || isDead) { return; }
        //change later
        Transform t = /*t_atkTarget != null ? t_atkTarget : (t_obstacle != null ? t_obstacle : t_currentTarget)*/t_destination;
        Vector3 targetDir = t != null ? t.position - transform.position : Vector3.forward;
        //targetDir.y = 0;
        transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, new Vector3(targetDir.x, 0f, targetDir.z), rot_speed * Time.deltaTime * speedMultiplier, 0.0f), Vector3.up);
        //if (Vector3.Angle(targetDir, transform.forward) < atkFov)
        //{
        //    Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, atkDist, LayermaskReference.blocked);
        //    if (hit.collider != null /*&& hit.collider.transform.root.CompareTag("Player")*/)
        //    {
        //        s_gun.Atk();
        //    }
        //}

        if (t_currentTarget == null) { t_currentTarget = t_station; }

        

        if (CheckSighted(t_currentTarget))
        {
            //Debug.Log("atk target");
            if (atkTargetCoroutine == null && t_currentTarget != t_atkTarget)
            {
                atkTargetCoroutine = StartCoroutine(SetAtkTarget(t_currentTarget, 2f));
            }
        }
        else
        {
            groundAgent.SetDestination(t_currentTarget.position);
            NavMeshPath groundPath = groundAgent.path;

            Transform obs = ObstacleDetection(groundPath.corners, obstacleDetectionDistance);
            t_obstacle = obs;
            //Debug.Log("obs" + obs==null);
            if (obs!=null)
            {
                if (t_destination != t_obstacle)
                //if (Vector2.Distance(new Vector2(groundObstaclesAgent.destination.x, groundObstaclesAgent.destination.z), new Vector2(t_obstacle.position.x, t_obstacle.position.z)) > 3f)
                {
                    groundObstaclesAgent.SetDestination(t_obstacle.position);
                    t_destination = t_obstacle;
                    //Debug.Log("set obs");
                    //Debug.Log("set obs" + new Vector2(groundObstaclesAgent.destination.x, groundObstaclesAgent.destination.z) + "" + new Vector2(t_obstacle.position.x, t_obstacle.position.z));
                }
            }
            else
            {
                if (t_destination != t_currentTarget)
                //if (Vector2.Distance(new Vector2(groundObstaclesAgent.destination.x, groundObstaclesAgent.destination.z), new Vector2(t_currentTarget.position.x, t_currentTarget.position.z)) > 3f)
                {
                    groundObstaclesAgent.SetDestination(t_currentTarget.position);
                    t_destination = t_currentTarget;
                    //Debug.Log("set target");
                }
            }
            

            if (t_obstacle != null && CheckSighted(t_obstacle))
            {
                //Debug.Log("atk obstacle");
            }
            else
            {
                //Debug.Log("not atk");
            }
            if (atkTargetCoroutine == null && t_atkTarget != (t_obstacle == null ? t_currentTarget : t_obstacle))
            {
                atkTargetCoroutine = StartCoroutine(SetAtkTarget((t_obstacle == null ? t_currentTarget : t_obstacle), 2f));
            }

            /*Debug.Log("diff" + (((groundObstaclePath.corners.Length > 0) ?  Vector3.Distance(groundObstaclePath.corners[^1], t_currentTarget.position) +" " + groundObstaclesAgent.remainingDistance : 0f)
                + " " + ((groundPath.corners.Length > 0) ? (Vector3.Distance(groundPath.corners[^1], t_currentTarget.position) + " " + groundAgent.remainingDistance) : 0f)));
            if (((groundObstaclePath.corners.Length > 0) ? Vector3.Distance(groundObstaclePath.corners[^1], t_currentTarget.position) + groundObstaclesAgent.remainingDistance : 0f)
                - ((groundPath.corners.Length > 0) ? groundAgent.remainingDistance : 0f) <= pathRerouteDistance)
            {
                Debug.Log("reroute");
                ObstacleDetection(groundObstaclePath.corners, obstacleDetectionDistance);
            }
            else
            {
                groundObstaclesAgent.SetPath(groundPath);
                ObstacleDetection(groundPath.corners, obstacleDetectionDistance);
                Debug.Log("go");
            }*/
        }   


        ComputeAggro();

        if (t_currentTarget != null)
        {
            //groundObstaclesAgent.SetDestination(t_obstacle==null?t_currentTarget.position:t_obstacle.position);
            corners = groundObstaclesAgent.path.corners;
            for (int i = 0; i < groundObstaclesAgent.path.corners.Length; i++)
            {
                corners[i].y += extend.y;
            }
            for (int i = 0; i < groundObstaclesAgent.path.corners.Length - 1; i++)
            {
                Debug.DrawLine(corners[i], corners[i + 1], Color.green);
            }
            if (corners != null && corners.Length > 0)
            {
                Debug.DrawLine(corners[^1], t_currentTarget.position, Color.green);
            }
            corners_ = groundAgent.path.corners;
            for (int i = 0; i < groundAgent.path.corners.Length; i++)
            {
                corners_[i].y += extend.y;
            }
            for (int i = 0; i < groundAgent.path.corners.Length - 1; i++)
            {
                Debug.DrawLine(corners_[i], corners_[i + 1], Color.blue);
            }
            if (corners_ != null && corners_.Length > 0)
            {
                Debug.DrawLine(corners_[^1], t_currentTarget.position, Color.blue);
            }
        }

        /*
        //Debug.DrawRay(transform.position, transform.forward * 100, Color.green, 2f);
        Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, atkDist);
        if (hit.collider != null && hit.collider.transform.root.CompareTag("Player"))
        {
            s_gun.Atk();
        }*/

        groundObstaclesAgent.speed = move_speed * speedMultiplier;
        groundObstaclesAgent.acceleration = move_speed * speedMultiplier * 100f;
        if (groundObstaclesAgent.velocity.magnitude > move_speed * (speedMultiplier + 0.05f))
        {
            groundObstaclesAgent.velocity = groundObstaclesAgent.velocity.normalized * speedMultiplier;
        }
        //Debug.Log("groundObstaclesAgent.velocity" + groundObstaclesAgent.velocity + " groundObstaclesAgent.desiredVelocity" + groundObstaclesAgent.desiredVelocity + " " + Mathf.Clamp(speedMultiplier, 0f, 1f));
        if (animator != null)
        {
            animator.SetBool("move", groundObstaclesAgent.velocity.magnitude > move_speed * walkCutoff);
        }

        desiredV = groundObstaclesAgent.desiredVelocity;
        currentV = groundObstaclesAgent.velocity;

        if (Vector3.Angle(groundObstaclesAgent.velocity, groundObstaclesAgent.desiredVelocity + offset) >= 45f)
        {
            groundObstaclesAgent.velocity = Vector3.Lerp(groundObstaclesAgent.velocity, groundObstaclesAgent.desiredVelocity, 50f * Time.deltaTime);
        }

        //Debug.Log(groundObstaclesAgent.destination + "" + groundObstaclesAgent.hasPath + "" + groundObstaclesAgent.pathStatus + "" + groundObstaclesAgent.isPathStale);
    }

    void LateUpdate()
    {
        
    }

    public Vector3 desiredV;
    public Vector3 currentV;
    public Vector3 offset;

    public Transform t_destination;

    /*private IEnumerator RebootIsDaWae()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            groundObstaclesAgent.enabled = false;
            groundObstaclesAgent.enabled = true;
        }
    }*/

    private bool CheckSighted(Transform t)
    {
        if (Vector3.Angle(t.position - transform.position, transform.forward) < detectFov)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, t.position - transform.position, out hit, detectDist, LayermaskReference.blocked) && hit.transform.root == t.root)
            {
                return true;
            }
        }
        return false;
    }

    private void ComputeAggro()
    {
        //deal with breakables
        HashSet<Transform> breakables = BreakableCollection.t_breakables;
        RaycastHit hit;
        foreach (Transform breakable in breakables)
        {
            if (Vector3.Distance(breakable.position, transform.position) < 10f && Physics.Raycast(transform.position, breakable.position - transform.position, out hit, 10f, LayermaskReference.obstacles) && hit.transform.root == breakable.root)
            {
                if (aggros.Any(x => x.target == breakable))
                {
                    aggros.Find(x => x.target == breakable).aggro += Time.deltaTime * aggroSightCurve.Evaluate(hit.distance);
                }
                else
                {
                    aggros.Add(new Aggro { aggro = 0f, target = breakable });
                }
            }
        }
        //deal with player
        if (Vector3.Distance(t_player.position, transform.position) < 10f && Physics.Raycast(transform.position, t_player.position - transform.position, out hit, 10f, LayermaskReference.obstacles) && hit.transform.root == t_player.root)
        {
            if (aggros.Any(x => x.target == t_player))
            {
                aggros.Find(x => x.target == t_player).aggro += Time.deltaTime * aggroSightCurve.Evaluate(hit.distance) * aggroSightPlayerMultiplier;
            }
            else
            {
                aggros.Add(new Aggro { aggro = 0f, target = t_player });
            }
        }
        //validate aggro list and compute aggro loss
        for (int i = aggros.Count - 1; i >= 0; i--)
        {
            if (!(breakables.Any(x => x == aggros[i].target) || aggros[i].target == t_player)) { aggros.RemoveAt(i); continue; }
            aggros[i].aggro = Mathf.Clamp(aggros[i].aggro - Time.deltaTime * aggroLossRate, 0f, maxAggro);
        }

        int curTargetIndex = aggros.FindIndex(x => x.target == t_currentTarget);
        if (t_currentTarget == t_station || t_currentTarget == null)
        {
            //check if some aggro pass threashold
            for (int i = 0; i < aggros.Count; i++)
            {
                if (aggros[i].aggro > aggroSetTargetThreshold && aggros[i].target != t_currentTarget)
                {
                    SetTarget(i);
                    break;
                }
            }
        }
        if (curTargetIndex >= 0)
        {
            //check if current target aggro less than lose threashold
            if (aggros[curTargetIndex].aggro < aggroLoseTargetThreshold)
            {
                t_currentTarget = t_station;
            }
            //cehck if any aggro higher than current target
            for (int i = 0; i < aggros.Count; i++)
            {
                if (aggros[i].aggro > aggros[curTargetIndex].aggro && aggros[i].target != t_currentTarget)
                {
                    SetTarget(i);
                    break;
                }
            }
        }
    }

    /*private void SetTarget(Transform t, bool boost = true, bool reset = true)
    {
        while (t.parent != null && t.CompareTag("BreakableBuilding")) { t = t.parent; }
        int i = aggros.FindIndex(x => x.target == t);
        if (i == -1) return;
        t_currentTarget = t;
        if (boost) aggros[i].aggro = Mathf.Clamp(aggroSetTargetThreshold + aggroTargetBoost, 0f, maxAggro);
        if (reset)
        {
            for (int j = 0; j < aggros.Count; j++)
            {
                if (j != i)
                {
                    aggros[j].aggro = 0f;
                }
            }
        }
        
    }*/

    private void SetTarget(int i, bool boost = true, bool reset = true)
    {
        t_currentTarget = aggros[i].target;
        t_obstacle = null;
        if (boost) aggros[i].aggro = Mathf.Clamp(aggroSetTargetThreshold + aggroTargetBoost, 0f, maxAggro);
        if (reset)
        {
            for (int j = 0; j < aggros.Count; j++)
            {
                if (j != i)
                {
                    aggros[j].aggro = 0f;
                }
            }
        }
    }

    private Coroutine atkTargetCoroutine;
    private Transform t_pendingAtkTarget;

    IEnumerator SetAtkTarget(Transform t, float waitTime = 2f)
    {
        t_atkTarget = t;
        yield return new WaitForSeconds(waitTime);
        atkTargetCoroutine = null;
    }

    public List<Collider> headshotCollider;

    public void ChangeHealth(float amount, Transform t = null, Collider col = null)
    {
        float amountMultiplied = amount;
        float headshotMultiplier = 1.5f;
        if (headshotCollider.Contains(col))
        {
            amount *= headshotMultiplier;
            //Debug.Log("heashot");
        }
        if (t == t_player)
        {
            GameManager.shotHit++;
            if (headshotCollider.Contains(col))
            {
                GameManager.headshotHit++;
            }
        }
        curHealth = Mathf.Clamp(curHealth + amount, 0, maxHealth);
        if (curHealth <= 0 && !isDead)
        {
            Die();
        }
        int i = aggros.FindIndex(x => x.target == t);
        if (i >= 0)
        {
            aggros[i].aggro += aggroAtkCurve.Evaluate(Mathf.Abs(amount) / maxHealth) * (t == t_player ? aggroAtkPlayerMultiplier : 1f);
        }
    }

    public void Die(float multiplier = 1.0f)
    {
        if (isDead) return;
        isDead = true;
        uiScript.CrosshairKill();
        SpawningManager.enemiesSpawned.Remove(gameObject);
        SpawningManager.remainingEnemies--;
        GameCoinsManager.instance.changeCoins(UnityEngine.Random.Range(coinDrop.x, coinDrop.y + 1));
        GameManager.killed++;
        //Destroy(gameObject);
        GetComponent<HpBar>().Dead();
        GameObject a = Instantiate(GameManager.instance.pf_audio, transform);
        Helper.AudioInit(a.GetComponent<AudioSource>(), audioDead);
        Destroy(a, 10f);
        foreach (var v in GetComponentsInChildren<Collider>())
        {
            v.enabled = false;
        }
        foreach (var v in GetComponentsInChildren<Rigidbody>())
        {
            v.isKinematic = false;
            v.detectCollisions = false;
            v.velocity = Vector3.zero;
            v.useGravity = false;
        }
        foreach (var v in GetComponentsInChildren<NavMeshAgent>())
        {
            v.enabled = false;
        }
        GetComponent<EnemyDeath>().Death(multiplier);
    }

    private Transform ObstacleDetection(Vector3[] corners, float dist)
    {
        float counter = 0f;
        float maxDist = 0f;
        for (int i = 0; i < corners.Length - 1; i++)
        {
            maxDist += Vector3.Distance(corners[i], corners[i + 1]);
        }
        int index = 0;
        RaycastHit hit;
        while (counter < dist && index < corners.Length - 1)
        {
            if (!Physics.BoxCast(corners[index], extend, corners[index + 1] - corners[index], out hit, transform.rotation, Mathf.Clamp(Vector3.Distance(corners[index], corners[index + 1]), 0f, maxDist - counter), LayermaskReference.breakables)) { index++; continue; }
            if (hit.transform.root != transform.root && hit.transform.root != t_currentTarget.root)
            {
                //SetTarget(hit.transform, true, false);
                //aggros.Find(x => x.target == hit.transform).aggro = Mathf.Clamp(aggros.Find(x => x.target == hit.transform).aggro - Time.deltaTime * aggroObstacleLossRate, 0f, maxAggro);
                return hit.transform.root;
            }
            counter += Vector3.Distance(corners[index], corners[index + 1]);
            index++;
        }
        return null;
        
    }

    [Serializable]
    public class Aggro
    {
        public float aggro;
        public Transform target;
    }
}

[Serializable]
public struct floatRange
{
    public float min;
    public float max;

    public floatRange(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
}
