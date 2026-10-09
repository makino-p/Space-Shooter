using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _enemyPrefab;
    [SerializeField]
    private GameObject[] powerups;

    [SerializeField]
    private GameObject _enemyContainer;
    
    private bool _stopSpawning = false;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnEnemyRoutine());
        StartCoroutine(SpawnPowerupRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SpawnEnemyRoutine()
    {
        while (_stopSpawning == false)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(-7f, 7f), 7, 0);
            GameObject newEnemy = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
            newEnemy.transform.parent = _enemyContainer.transform;
            yield return new WaitForSeconds(3.0f);
        }
    }

    IEnumerator SpawnPowerupRoutine()
    {
        while (_stopSpawning == false)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(-7f, 7f), 7, 0);
            int randomIndex = Random.Range(0, 3);
            Instantiate(powerups[randomIndex], spawnPosition, Quaternion.identity);
            yield return new WaitForSeconds(Random.Range(3, 8));
        }
    }
public void OnPlayerDeath()
{
    _stopSpawning = true;

    foreach (Transform enemy in _enemyContainer.transform)
    {
        Destroy(enemy.gameObject);
    }
}
}
