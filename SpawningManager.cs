using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class SpawningManager : MonoBehaviour
{
    public static SpawningManager instance;

    public List<WaveInfo> waveInfos;
    public GameObject[] pf_enemies;
    public Transform[] spawnPoints;
    public bool autoSetSpawnPoints;

    public bool preparing = false;
    public int currentWave = -1;
    public static int remainingEnemies;
    public static HashSet<GameObject> enemiesSpawned;

    public GameObject pf_enemyPathAssistant;
    // Start is called before the first frame update

    public float timeToNextWave;

    private KeyCode key_emergency = KeyCode.I;
    private float t_emergency = 3f;
    private float emergencyTimer = 0f;

    void Awake()
    {
        instance = this;
        remainingEnemies = 0;
        enemiesSpawned = new HashSet<GameObject>();
    }
    void Start()
    {
        timeToNextWave = -waveInfos[0].prepareTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentWave < waveInfos.Count)
        {
            if (preparing && timeToNextWave >= 0f)
            {
                preparing = false;
                StartWave();
            }
            if (preparing == false && remainingEnemies <= 0)
            {

                preparing = true;
                currentWave++;
                if (currentWave >= waveInfos.Count) { GameManager.instance.Win(); }
                timeToNextWave = currentWave >= waveInfos.Count ? 0f : -waveInfos[currentWave].prepareTime;
            }
            timeToNextWave += Time.deltaTime;
        }

        if (Input.GetKey(key_emergency))
        {
            emergencyTimer += Time.deltaTime;
            if (emergencyTimer >= t_emergency)
            {
                ClearWave();
                emergencyTimer = 0f;
            }
        }
        else
        {
            emergencyTimer = 0f;
        }
    }

    public void StartWave()
    {
        WaveInfo waveInfo = waveInfos[currentWave];
        remainingEnemies = waveInfo.spawnInfos.Count;
        for (int i = 0; i < waveInfo.spawnInfos.Count; i++)
        {
            StartCoroutine(SpawnEnemy(pf_enemies[waveInfo.spawnInfos[i].pf_enemy_ID], waveInfo.spawnInfos[i].spawnPointID, waveInfo.spawnInfos[i].spawnTime));
        }
    }

    IEnumerator SpawnEnemy(GameObject pf, int spawnID, float spawnTime)
    {
        yield return new WaitForSeconds(spawnTime);
        GameObject enemy = Instantiate(pf, spawnPoints[spawnID].position, spawnPoints[spawnID].rotation);
        GameObject pathAssistant = Instantiate(pf_enemyPathAssistant, spawnPoints[spawnID].position, spawnPoints[spawnID].rotation);
        enemy.GetComponent<EnemyController>().Init(pathAssistant.GetComponent<NavMeshAgent>());
        pathAssistant.GetComponent<EnemyPathAssistant>().Init(enemy.transform);
        enemiesSpawned.Add(enemy);
    }

    public void ClearWave()
    {
        List<GameObject> enemies = new List<GameObject>();
        enemies = enemiesSpawned.ToList();
        foreach (GameObject go in enemies)
        {
            if (go == null) continue;
            go.GetComponent<EnemyController>().Die();
        }
    }

    void OnValidate()
    {
        if (autoSetSpawnPoints)
        {
            autoSetSpawnPoints = false;
            spawnPoints = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                spawnPoints[i] = transform.GetChild(i);
            }
        }
        if (pf_enemies == null || pf_enemies.Length == 0) return;
        for (int i = 0; i < waveInfos.Count; i++)
        {
            for (int j = 0; j < waveInfos[i].spawnInfos.Count; j++)
            {
                if (waveInfos[i].spawnInfos[j].pf_enemy_ID >= 0 && waveInfos[i].spawnInfos[j].pf_enemy_ID < pf_enemies.Length)
                {
                    if (waveInfos[i].spawnInfos[j].info != pf_enemies[waveInfos[i].spawnInfos[j].pf_enemy_ID].name) waveInfos[i].spawnInfos[j].info = pf_enemies[waveInfos[i].spawnInfos[j].pf_enemy_ID].name;
                }
                else
                {
                    if (waveInfos[i].spawnInfos[j].info != "n/a") waveInfos[i].spawnInfos[j].info = "n/a";
                }
            }
        }
    }
}

[Serializable]
[Tooltip("Wave Info")]
public struct WaveInfo
{
    [Tooltip("The time to start the wave since the game start / last wave completed")]
    public float prepareTime;
    [Tooltip("List of spawn infos for this wave")]
    public List<SpawnInfo> spawnInfos;
}

[Serializable]
[Tooltip("Spawn Info for one enemy")]
public class SpawnInfo
{
    [Tooltip("The enemy prefab ID from pf_enemies")]
    public int pf_enemy_ID;
    [Tooltip("The time to spawn the enemy since wave start")]
    public float spawnTime;
    [Tooltip("The spawn point ID in the scene")]
    public int spawnPointID;

    public string info;
}
