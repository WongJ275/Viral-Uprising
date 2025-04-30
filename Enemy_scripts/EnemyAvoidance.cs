using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAvoidance : MonoBehaviour
{
    private SpawningManager s_spawningManager;
    private NavMeshAgent navMeshAgent;
    private EnemyController enemyController;
    private Collider col;
    public AnimationCurve angleCurve;
    public AnimationCurve distanceCurve;
    public float angleMultiplier;
    public float distanceMultiplier;

    public float maxMagnitude = 2f;

    public List<Vector3> vl = new();

    public Vector3 o;

    void Awake()
    {
        s_spawningManager = SpawningManager.instance;
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyController = GetComponent<EnemyController>();
        col = GetComponent<Collider>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (enemyController != null && !enemyController.isDead)
        {
            vl = new List<Vector3>();
            foreach (var v in GetV())
            {
                vl.Add(v);
            }
            Vector3 offset = Vector3.zero;
            Vector2 right = new Vector2(transform.right.x, transform.right.z);
            Vector2 forward = new Vector2(transform.forward.x, transform.forward.z);
            foreach (Vector3 v in vl)
            {
                float angleRight = Vector2.Angle(right, new Vector2(v.x, v.z));
                float angleForward = Vector2.Angle(forward, new Vector2(v.x, v.z));
                offset += (angleRight > 90f ? angleCurve.Evaluate(angleForward - 90f) : -angleCurve.Evaluate(90f - angleForward)) * angleMultiplier * distanceCurve.Evaluate(v.magnitude) * distanceMultiplier * enemyController.move_speed * transform.right;
            }
            navMeshAgent.Move(offset * Time.deltaTime * Mathf.Clamp(navMeshAgent.desiredVelocity.magnitude * enemyController.speedMultiplier, 0f, maxMagnitude));
            enemyController.offset = offset;
            //transform.Translate(offset * Time.deltaTime);
            //o = offset;
            Debug.DrawLine(transform.position, transform.position + offset * 5f, Color.red);
        }
    }

    private HashSet<Vector3> GetV()
    {
        HashSet<Vector3> vs = new HashSet<Vector3>();
        foreach (GameObject enemy in SpawningManager.enemiesSpawned)
        {
            if (enemy == gameObject) continue;
            Collider c = enemy.GetComponent<Collider>();
            if (c == null) continue;
            vs.Add(c.ClosestPoint(transform.position) - col.ClosestPoint(enemy.transform.position));
        }
        return vs;
    }
}
