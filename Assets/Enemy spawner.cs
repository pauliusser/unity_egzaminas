using UnityEngine;

public class Enemyspawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public Transform target;
    public float spawnInterval = 2f;

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
        if (spawnInterval <= 0f)
        {
            SpawnEnemy();
            spawnInterval = 2f;
        }
    }
}
