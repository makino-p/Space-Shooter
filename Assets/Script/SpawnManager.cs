using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    // Все пикапы из обоих списков собираются в таблицу по powerupID;
    // частота выпадения задаётся весами ниже, а не тем, в каком списке лежит префаб.
    [SerializeField]
    private GameObject[] rarePowerups;
    [SerializeField]
    private GameObject _enemyPrefab;
    [SerializeField]
    private GameObject[] powerups;

    [SerializeField]
    private GameObject _enemyContainer;

    [Header("Waves")]
    [SerializeField]
    private int _totalWaves = 5;            // последняя волна заканчивается боссом
    [SerializeField]
    private int _baseEnemiesPerWave = 4;
    [SerializeField]
    private int _extraEnemiesPerWave = 3;   // +3 врага каждую следующую волну
    [SerializeField]
    private float _timeBetweenWaves = 3f;

    [Header("Pickup weights (больше = чаще)")]
    [SerializeField] private int _weightTripleShot = 12;
    [SerializeField] private int _weightSpeed = 10;
    [SerializeField] private int _weightShield = 10;
    [SerializeField] private int _weightAmmo = 35;     // частый
    [SerializeField] private int _weightHealth = 5;    // редкий
    [SerializeField] private int _weightRadial = 4;    // редкий
    [SerializeField] private int _weightNegative = 10;
    [SerializeField] private int _weightHoming = 4;    // редкий
    [SerializeField] private float _minPickupDelay = 4f;
    [SerializeField] private float _maxPickupDelay = 7f;

    private bool _stopSpawning = false;
    private bool _hasStarted = false;
    private int _currentWave = 0;
    private UI_Manager _uiManager;
    private Player _player;
    private readonly Dictionary<int, GameObject> _powerupById = new Dictionary<int, GameObject>();

    public int CurrentWave { get { return _currentWave; } }

    void Start()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null) _uiManager = canvas.GetComponent<UI_Manager>();
        GameObject p = GameObject.Find("Player");
        if (p != null) _player = p.GetComponent<Player>();
        BuildPowerupTable();
    }

    void BuildPowerupTable()
    {
        List<GameObject> all = new List<GameObject>();
        if (powerups != null) all.AddRange(powerups);
        if (rarePowerups != null) all.AddRange(rarePowerups);

        foreach (GameObject prefab in all)
        {
            if (prefab == null) continue;
            Powerup pu = prefab.GetComponent<Powerup>();
            if (pu != null && !_powerupById.ContainsKey(pu.ID))
            {
                _powerupById.Add(pu.ID, prefab);
            }
        }
    }

    public void StartSpawning()
    {
        if (_hasStarted)
        {
            return;
        }
        _hasStarted = true;
        StartCoroutine(WaveRoutine());
        StartCoroutine(SpawnPowerupRoutine());
    }

    // ---------------- Волны ----------------
    IEnumerator WaveRoutine()
    {
        yield return new WaitForSeconds(2f);

        for (int wave = 1; wave <= _totalWaves && !_stopSpawning; wave++)
        {
            _currentWave = wave;
            bool isFinal = wave == _totalWaves;
            if (_uiManager != null) _uiManager.ShowWave(wave, isFinal);
            yield return new WaitForSeconds(2f);

            int enemyCount = _baseEnemiesPerWave + (wave - 1) * _extraEnemiesPerWave;
            float spawnDelay = Mathf.Max(0.7f, 2.2f - 0.3f * (wave - 1));

            for (int i = 0; i < enemyCount && !_stopSpawning; i++)
            {
                SpawnEnemy(wave);
                yield return new WaitForSeconds(spawnDelay);
            }

            // ждать, пока волна не будет уничтожена (или враги улетят)
            while (!_stopSpawning && _enemyContainer.transform.childCount > 0)
            {
                yield return new WaitForSeconds(0.5f);
            }

            if (_stopSpawning) yield break;

            if (isFinal)
            {
                if (_uiManager != null) _uiManager.ShowMessage("WARNING: BOSS INCOMING", 2.5f, new Color(1f, 0.3f, 0.3f));
                yield return new WaitForSeconds(2.5f);
                SpawnBoss();
            }
            else
            {
                yield return new WaitForSeconds(_timeBetweenWaves);
            }
        }
    }

    void SpawnEnemy(int wave)
    {
        Vector3 spawnPosition = new Vector3(Random.Range(-8f, 8f), 7, 0);
        GameObject newEnemy = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
        newEnemy.transform.parent = _enemyContainer.transform;

        Enemy enemy = newEnemy.GetComponent<Enemy>();
        if (enemy == null) return;

        EnemyType type = PickEnemyType(wave);
        float shieldChance = Mathf.Clamp01(0.1f * (wave - 1));
        bool shielded = Random.value < shieldChance;
        float speed = 2.5f + 0.25f * wave;
        if (type == EnemyType.Beamer) speed = 2f;
        enemy.Setup(type, shielded, speed);
    }

    // Новые типы врагов открываются с каждой волной.
    EnemyType PickEnemyType(int wave)
    {
        List<EnemyType> pool = new List<EnemyType> { EnemyType.Basic, EnemyType.Basic, EnemyType.ZigZag };
        if (wave >= 2) { pool.Add(EnemyType.Shooter); pool.Add(EnemyType.Shooter); }
        if (wave >= 3) { pool.Add(EnemyType.Dodger); pool.Add(EnemyType.RearGunner); }
        if (wave >= 4) { pool.Add(EnemyType.Beamer); }
        return pool[Random.Range(0, pool.Count)];
    }

    void SpawnBoss()
    {
        GameObject bossObject = Instantiate(_enemyPrefab, new Vector3(0f, 8.5f, 0f), Quaternion.identity);
        bossObject.name = "Boss";
        bossObject.transform.parent = _enemyContainer.transform;
        bossObject.transform.localScale = _enemyPrefab.transform.localScale * 2.2f;

        GameObject laserPrefab = null;
        Enemy enemy = bossObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            laserPrefab = enemy.LaserPrefab;
            DestroyImmediate(enemy); // поведение обычного врага боссу не нужно
        }

        Boss boss = bossObject.AddComponent<Boss>();
        boss.Setup(laserPrefab, this);
    }

    public void OnBossDefeated()
    {
        _stopSpawning = true;
        if (_uiManager != null) _uiManager.ShowMessage("YOU WIN!", 0f, new Color(0.4f, 1f, 0.5f));
    }

    // ---------------- Пикапы ----------------
    IEnumerator SpawnPowerupRoutine()
    {
        yield return new WaitForSeconds(3.0f);
        while (_stopSpawning == false)
        {
            SpawnRandomPowerup();
            yield return new WaitForSeconds(Random.Range(_minPickupDelay, _maxPickupDelay));
        }
    }

    void SpawnRandomPowerup()
    {
        int ammoWeight = _weightAmmo;
        if (_player != null && _player.Ammo == 0)
        {
            ammoWeight *= 3; // без патронов шанс получить Ammo выше
        }

        int[] ids = { 0, 1, 2, 3, 4, 5, 6, 7 };
        int[] weights = { _weightTripleShot, _weightSpeed, _weightShield, ammoWeight,
                          _weightHealth, _weightRadial, _weightNegative, _weightHoming };

        int total = 0;
        for (int i = 0; i < ids.Length; i++)
        {
            if (CanSpawnPowerup(ids[i])) total += weights[i];
        }
        if (total <= 0) return;

        int roll = Random.Range(0, total);
        for (int i = 0; i < ids.Length; i++)
        {
            if (!CanSpawnPowerup(ids[i])) continue;
            if (roll < weights[i])
            {
                SpawnPowerup(ids[i]);
                return;
            }
            roll -= weights[i];
        }
    }

    bool CanSpawnPowerup(int id)
    {
        if (id == 6) return _powerupById.ContainsKey(1); // негативный делается из Speed
        if (id == 7) return _powerupById.ContainsKey(5); // Homing делается из Radial
        return _powerupById.ContainsKey(id);
    }

    void SpawnPowerup(int id)
    {
        Vector3 spawnPosition = new Vector3(Random.Range(-8f, 8f), 7, 0);

        if (id == 6)
        {
            GameObject neg = Instantiate(_powerupById[1], spawnPosition, Quaternion.identity);
            neg.name = "Negative_Powerup";
            neg.GetComponent<Powerup>().Configure(6, new Color(1f, 0.2f, 0.2f, 1f));
            return;
        }
        if (id == 7)
        {
            GameObject homing = Instantiate(_powerupById[5], spawnPosition, Quaternion.identity);
            homing.name = "Homing_Powerup";
            homing.GetComponent<Powerup>().Configure(7, new Color(1f, 0.4f, 1f, 1f));
            return;
        }

        Instantiate(_powerupById[id], spawnPosition, Quaternion.identity);
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
