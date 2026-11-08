using System.Collections;
using UnityEngine;

// Босс финальной волны. Создаётся SpawnManager из префаба Enemy (компонент Enemy удаляется).
// Спускается в центр экрана, остаётся там и по кругу использует три атаки.
public class Boss : MonoBehaviour
{
    [SerializeField]
    private int _maxHealth = 30;
    [SerializeField]
    private Vector3 _stopPosition = new Vector3(0f, 3.2f, 0f);
    [SerializeField]
    private float _enterSpeed = 1.5f;

    private int _health;
    private bool _hasArrived = false;
    private bool _isDead = false;
    private GameObject _laserPrefab;
    private SpawnManager _spawnManager;
    private UI_Manager _uiManager;
    private Player _player;
    private SpriteRenderer _renderer;
    private Animator _anim;
    private AudioSource _audioSource;
    private Collider2D _collider;
    private Color _baseColor = new Color(1f, 0.55f, 0.55f, 1f);

    private static readonly Color ShotColor = new Color(1f, 0.5f, 0.1f, 1f);

    public void Setup(GameObject laserPrefab, SpawnManager spawnManager)
    {
        _laserPrefab = laserPrefab;
        _spawnManager = spawnManager;
    }

    void Start()
    {
        _health = _maxHealth;
        _renderer = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _collider = GetComponent<Collider2D>();
        GameObject p = GameObject.Find("Player");
        if (p != null) _player = p.GetComponent<Player>();
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null) _uiManager = canvas.GetComponent<UI_Manager>();

        if (_renderer != null) _renderer.color = _baseColor;
        if (_uiManager != null) _uiManager.ShowBossHealth(1f);
    }

    void Update()
    {
        if (_isDead || _hasArrived)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, _stopPosition, _enterSpeed * Time.deltaTime);
        if (transform.position == _stopPosition)
        {
            _hasArrived = true;
            StartCoroutine(AttackLoop());
        }
    }

    IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(1f);
        while (!_isDead)
        {
            yield return StartCoroutine(RadialBurst());
            yield return new WaitForSeconds(1.2f);
            if (_isDead) yield break;

            yield return StartCoroutine(AimedSpread());
            yield return new WaitForSeconds(1.2f);
            if (_isDead) yield break;

            yield return StartCoroutine(BeamCage());
            yield return new WaitForSeconds(1.5f);
        }
    }

    // Атака 1: кольцо выстрелов во все стороны, две волны со сдвигом.
    IEnumerator RadialBurst()
    {
        for (int wave = 0; wave < 2; wave++)
        {
            int count = 16;
            float offset = wave * (180f / count);
            for (int i = 0; i < count; i++)
            {
                float angle = (360f / count) * i + offset;
                Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.down;
                EnemyProjectile.Spawn(_laserPrefab, transform.position + dir * 2.2f, dir, 4.5f, ShotColor);
            }
            yield return new WaitForSeconds(0.6f);
        }
    }

    // Атака 2: веер из 5 выстрелов в сторону игрока, 3 залпа.
    IEnumerator AimedSpread()
    {
        for (int volley = 0; volley < 3; volley++)
        {
            if (_player == null) yield break;
            Vector3 toPlayer = (_player.transform.position - transform.position).normalized;
            for (int i = -2; i <= 2; i++)
            {
                Vector3 dir = Quaternion.Euler(0, 0, i * 12f) * toPlayer;
                EnemyProjectile.Spawn(_laserPrefab, transform.position + dir * 2.2f, dir, 7f, ShotColor);
            }
            yield return new WaitForSeconds(0.45f);
        }
    }

    // Атака 3: три лазерных луча с проходами между ними. Сначала мигающее предупреждение.
    IEnumerator BeamCage()
    {
        float playerX = _player != null ? _player.transform.position.x : 0f;
        // центральный луч бьёт туда, где сейчас игрок; два других — по бокам
        float[] offsets = { playerX - transform.position.x, -4.5f, 4.5f };
        foreach (float x in offsets)
        {
            EnemyBeam.Spawn(_laserPrefab, transform, new Vector3(x, -1.8f, 0f), 12f, 1.1f, 0.9f,
                            new Color(1f, 0.2f, 0.2f, 1f), 0.6f);
        }
        yield return new WaitForSeconds(2.2f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isDead)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            Player p = other.GetComponent<Player>();
            if (p != null) p.Damage();
            return;
        }

        if (other.CompareTag("Laser") && other.GetComponent<SpriteRenderer>() != null)
        {
            Destroy(other.gameObject);
            if (!_hasArrived)
            {
                return; // неуязвим, пока заходит на позицию
            }
            TakeDamage(1);
        }
    }

    void TakeDamage(int amount)
    {
        _health -= amount;
        if (_uiManager != null) _uiManager.ShowBossHealth((float)_health / _maxHealth);

        if (_health <= 0)
        {
            Die();
        }
        else
        {
            StopCoroutine("Flash");
            StartCoroutine("Flash");
        }
    }

    IEnumerator Flash()
    {
        if (_renderer == null) yield break;
        _renderer.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        _renderer.color = _baseColor;
    }

    void Die()
    {
        _isDead = true;
        StopAllCoroutines();
        if (_renderer != null) _renderer.color = _baseColor;
        if (_collider != null) _collider.enabled = false;
        if (_anim != null) _anim.SetTrigger("OnEnemyDeath");
        if (_audioSource != null && _audioSource.isActiveAndEnabled) _audioSource.Play();
        if (_player != null) _player.AddScore(500);
        if (_uiManager != null) _uiManager.HideBossHealth();

        // убрать оставшиеся лучи босса
        foreach (EnemyBeam beam in FindObjectsOfType<EnemyBeam>())
        {
            Destroy(beam.gameObject);
        }

        if (_spawnManager != null) _spawnManager.OnBossDefeated();
        Destroy(gameObject, 2.37f);
    }
}
