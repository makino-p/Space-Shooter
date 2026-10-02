using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _enemyPrefab;

    [SerializeField]
    private GameObject _enemyContainer;

    [SerializeField]
    private bool _stopSpawning = false;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnEnemyRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SpawnEnemyRoutine()
    {
        while (_stopSpawning == false)
        {
            float randomX = Random.Range(-7f, 7f);
            Vector3 spawnPosition = new Vector3(randomX, 7, 0);
            GameObject newEnemy = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
            newEnemy.transform.parent = _enemyContainer.transform;
            yield return new WaitForSeconds(5.0f);
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
