using UnityEngine;

public class Enemyspawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public Transform target;
    public float spawnInterval = 2f;
    private int spawned = 0;
    public float cooldownBetweenWaves = 10f;

    public GameObject SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        enemy.GetComponent<Enemy>().target = target;
        return enemy;
    }
    void Start()
    {
        SpawnEnemy();
    }
    private void Update()
    {
        spawnInterval -= Time.deltaTime;
        if (spawnInterval <= 0f && spawned < 15)
        {
            SpawnEnemy();
            spawnInterval = 2f;
            spawned++;
            if (spawned == 15 || spawned == 30 || spawned == 45)
            {
                spawnInterval = cooldownBetweenWaves;
            }
            else
            {
                spawnInterval = 2f;
            }
        }




        // spawn is performed in 3 waves of 15 enemies each, with a 5 second break between waves
        // 10seconds cooldown between waves, 15 enemies per wave, 3 waves total

    }
}
