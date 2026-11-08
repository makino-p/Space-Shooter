using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField]
    private GameObject _explosionPrefab;
    [SerializeField]
    private float _rotateSpeed = 3.0f;
    private SpawnManager _spawnManager;
    // Start is called before the first frame update
    void Start()
    {
        GameObject spawnManagerObject = GameObject.Find("SpawnManager");
        if (spawnManagerObject != null)
        {
            _spawnManager = spawnManagerObject.GetComponent<SpawnManager>();
        }

        if (_spawnManager == null)
        {
            Debug.LogError("Asteroid: SpawnManager not found in the scene.");
        }
        if (_explosionPrefab == null)
        {
            Debug.LogError("Asteroid: Explosion Prefab is not assigned in the Inspector.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward * _rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Laser"))
        {
            // Сначала уничтожаем лазер и астероид, чтобы ошибка ниже
            // не позволила лазеру "пройти сквозь" астероид.
            Destroy(other.gameObject);
            Destroy(this.gameObject);

            if (_explosionPrefab != null)
            {
                Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            }

            if (_spawnManager != null)
            {
                _spawnManager.StartSpawning();
            }
        }
    }
}
