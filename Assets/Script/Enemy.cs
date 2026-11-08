using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Basic,      // прямо вниз или под углом
    ZigZag,     // волнообразное движение
    Shooter,    // стреляет вниз и сбивает пикапы перед собой
    RearGunner, // стреляет назад, когда оказался позади игрока
    Dodger,     // уклоняется от выстрелов игрока
    Beamer      // стреляет лазерным лучом
}

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float _speed = 1.5f;
    [SerializeField]
    private GameObject _laserPrefab;

    [Header("Type")]
    [SerializeField]
    private EnemyType _type = EnemyType.Basic;
    [SerializeField]
    private bool _hasShield = false;

    [Header("Aggression (ram)")]
    [SerializeField]
    private float _ramDistance = 3f;
    [SerializeField]
    private float _ramSpeedMultiplier = 2.2f;

    private Player player;
    private Animator _anim;
    private AudioSource _audioSource;
    private SpriteRenderer _renderer;
    private Collider2D _collider;
    private GameObject _shieldVisual;
    private bool _isDead = false;

    // движение
    private Vector3 _moveDirection = Vector3.down;
    private float _zigZagAmplitude = 2.5f;
    private float _zigZagFrequency = 2f;
    private float _zigZagBaseX;
    private float _spawnTime;
    private float _dodgeTimer = 0f;
    private float _dodgeCooldown = 0f;
    private float _dodgeDirection = 0f;

    // стрельба
    private float _nextFireTime;
    private float _nextPickupCheck;
    private float _nextRearShot;

    private static readonly Color ProjectileColor = new Color(1f, 0.25f, 0.25f, 1f);

    public GameObject LaserPrefab { get { return _laserPrefab; } }

    // Вызывается SpawnManager сразу после Instantiate.
    public void Setup(EnemyType type, bool shielded, float speed)
    {
        _type = type;
        _hasShield = shielded;
        _speed = speed;
    }

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _renderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        GameObject p = GameObject.Find("Player");
        if (p != null)
        {
            player = p.GetComponent<Player>();
        }
        _anim = GetComponent<Animator>();

        _spawnTime = Time.time;
        _zigZagBaseX = transform.position.x;
        _nextFireTime = Time.time + Random.Range(1f, 2.5f);

        ApplyTypeLook();
        if (_hasShield)
        {
            CreateShieldVisual();
        }

        // Basic иногда влетает под углом
        if (_type == EnemyType.Basic && Random.value < 0.5f)
        {
            float sideX = transform.position.x > 0 ? -1f : 1f;
            _moveDirection = new Vector3(sideX * Random.Range(0.35f, 0.7f), -1f, 0f).normalized;
        }
    }

    void ApplyTypeLook()
    {
        if (_renderer == null)
        {
            return;
        }
        switch (_type)
        {
            case EnemyType.ZigZag: _renderer.color = new Color(0.6f, 1f, 0.6f); break;
            case EnemyType.Shooter: _renderer.color = new Color(1f, 0.75f, 0.4f); break;
            case EnemyType.RearGunner: _renderer.color = new Color(0.8f, 0.6f, 1f); break;
            case EnemyType.Dodger: _renderer.color = new Color(0.5f, 0.9f, 1f); break;
            case EnemyType.Beamer: _renderer.color = new Color(1f, 0.45f, 0.75f); break;
        }
    }

    void CreateShieldVisual()
    {
        _shieldVisual = new GameObject("Enemy_Shield");
        _shieldVisual.transform.SetParent(transform, false);
        _shieldVisual.transform.localPosition = Vector3.zero;
        _shieldVisual.transform.localScale = Vector3.one * 3.6f;
        SpriteRenderer sr = _shieldVisual.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSprites.Ring;
        sr.color = new Color(0.3f, 0.8f, 1f, 0.9f);
        if (_renderer != null)
        {
            sr.sortingLayerID = _renderer.sortingLayerID;
            sr.sortingOrder = _renderer.sortingOrder + 1;
        }
    }

    void Update()
    {
        if (_isDead)
        {
            return;
        }

        Move();
        HandleWeapons();

        if (_shieldVisual != null)
        {
            float pulse = 3.6f + 0.15f * Mathf.Sin(Time.time * 5f);
            _shieldVisual.transform.localScale = Vector3.one * pulse;
        }

        float bottom = _type == EnemyType.RearGunner ? -7f : -6f;
        if (transform.position.y < bottom || Mathf.Abs(transform.position.x) > 12f)
        {
            Destroy(this.gameObject);
        }
    }

    // ---------------- Движение ----------------
    void Move()
    {
        // Агрессия: если игрок рядом — таран
        if (player != null && _type != EnemyType.Beamer)
        {
            Vector3 toPlayer = player.transform.position - transform.position;
            if (toPlayer.magnitude < _ramDistance && toPlayer.y < 0.5f)
            {
                transform.position += toPlayer.normalized * _speed * _ramSpeedMultiplier * Time.deltaTime;
                return;
            }
        }

        switch (_type)
        {
            case EnemyType.ZigZag:
                {
                    float t = Time.time - _spawnTime;
                    float x = _zigZagBaseX + Mathf.Sin(t * _zigZagFrequency) * _zigZagAmplitude;
                    x = Mathf.Clamp(x, -9.5f, 9.5f);
                    float y = transform.position.y - _speed * Time.deltaTime;
                    transform.position = new Vector3(x, y, 0f);
                    break;
                }
            case EnemyType.Dodger:
                MoveDodger();
                break;
            case EnemyType.Beamer:
                // спускается до верхней части экрана и двигается из стороны в сторону
                if (transform.position.y > 3.5f)
                {
                    transform.position += Vector3.down * _speed * Time.deltaTime;
                }
                else
                {
                    float x = Mathf.Sin((Time.time - _spawnTime) * 0.8f) * 6f;
                    Vector3 target = new Vector3(x, transform.position.y - 0.15f * Time.deltaTime, 0f);
                    transform.position = Vector3.MoveTowards(transform.position, target, _speed * 1.5f * Time.deltaTime);
                }
                break;
            default:
                transform.position += _moveDirection * _speed * Time.deltaTime;
                break;
        }
    }

    void MoveDodger()
    {
        _dodgeCooldown -= Time.deltaTime;

        if (_dodgeTimer > 0f)
        {
            _dodgeTimer -= Time.deltaTime;
            Vector3 pos = transform.position + new Vector3(_dodgeDirection * 7f * Time.deltaTime, -_speed * 0.5f * Time.deltaTime, 0f);
            pos.x = Mathf.Clamp(pos.x, -9.5f, 9.5f);
            transform.position = pos;
            return;
        }

        if (_dodgeCooldown <= 0f)
        {
            GameObject shot = FindIncomingShot(3.5f, 1.3f);
            if (shot != null)
            {
                float dir = Mathf.Sign(transform.position.x - shot.transform.position.x);
                if (dir == 0f) dir = Random.value < 0.5f ? -1f : 1f;
                // не уклоняться в край экрана
                if (Mathf.Abs(transform.position.x) > 8f) dir = -Mathf.Sign(transform.position.x);
                _dodgeDirection = dir;
                _dodgeTimer = 0.35f;
                _dodgeCooldown = 0.8f;
                return;
            }
        }

        transform.position += Vector3.down * _speed * Time.deltaTime;
    }

    // Ищет лазер игрока, летящий снизу в сторону врага.
    GameObject FindIncomingShot(float range, float width)
    {
        foreach (GameObject laser in GameObject.FindGameObjectsWithTag("Laser"))
        {
            if (laser.GetComponent<SpriteRenderer>() == null)
            {
                continue; // контейнеры Triple/Radial Shot
            }
            Vector3 d = transform.position - laser.transform.position;
            if (d.y > 0f && d.y < range && Mathf.Abs(d.x) < width)
            {
                return laser;
            }
        }
        return null;
    }

    // ---------------- Оружие ----------------
    void HandleWeapons()
    {
        if (_laserPrefab == null)
        {
            return;
        }

        switch (_type)
        {
            case EnemyType.Shooter:
                ShootPickupsInFront();
                if (Time.time >= _nextFireTime)
                {
                    Fire(Vector3.down);
                    _nextFireTime = Time.time + Random.Range(1.8f, 3f);
                }
                break;

            case EnemyType.RearGunner:
                ShootPickupsInFront();
                if (player != null && Time.time >= _nextRearShot)
                {
                    Vector3 d = player.transform.position - transform.position;
                    // враг ниже игрока = позади него -> стреляет назад (вверх)
                    if (d.y > 0.5f && Mathf.Abs(d.x) < 1.5f)
                    {
                        Fire(Vector3.up);
                        _nextRearShot = Time.time + 0.6f;
                    }
                }
                if (Time.time >= _nextFireTime)
                {
                    Fire(Vector3.down);
                    _nextFireTime = Time.time + Random.Range(2.5f, 4f);
                }
                break;

            case EnemyType.Beamer:
                if (Time.time >= _nextFireTime && transform.position.y <= 3.6f)
                {
                    EnemyBeam.Spawn(_laserPrefab, transform, new Vector3(0f, -0.9f, 0f), 12f, 0.9f, 0.6f,
                                    new Color(1f, 0.3f, 0.8f, 1f));
                    _nextFireTime = Time.time + Random.Range(3.5f, 5f);
                }
                break;
        }
    }

    // Если пикап прямо по курсу — сбить его.
    void ShootPickupsInFront()
    {
        if (Time.time < _nextPickupCheck)
        {
            return;
        }
        _nextPickupCheck = Time.time + 0.25f;

        foreach (Powerup powerup in FindObjectsOfType<Powerup>())
        {
            Vector3 d = powerup.transform.position - transform.position;
            if (d.y < -0.5f && d.y > -7f && Mathf.Abs(d.x) < 0.8f)
            {
                Fire(d.normalized);
                _nextPickupCheck = Time.time + 0.9f;
                return;
            }
        }
    }

    void Fire(Vector3 direction)
    {
        Vector3 spawn = transform.position + direction.normalized * 1.1f;
        EnemyProjectile.Spawn(_laserPrefab, spawn, direction, 7f, ProjectileColor);
    }

    // ---------------- Попадания ----------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isDead)
        {
            return;
        }

        if (other.tag == "Player")
        {
            Player hitPlayer = other.transform.GetComponent<Player>();
            if (hitPlayer != null)
            {
                hitPlayer.Damage();
            }
            Die(false);
            return;
        }

        if (other.tag == "Laser" && other.GetComponent<SpriteRenderer>() != null)
        {
            Destroy(other.gameObject);

            // щит поглощает одно попадание
            if (_hasShield)
            {
                _hasShield = false;
                if (_shieldVisual != null)
                {
                    Destroy(_shieldVisual);
                }
                return;
            }
            Die(true);
        }
    }

    void Die(bool givePoints)
    {
        _isDead = true;
        if (givePoints && player != null)
        {
            player.AddScore(10);
        }
        if (_shieldVisual != null)
        {
            Destroy(_shieldVisual);
        }
        if (_anim != null)
        {
            _anim.SetTrigger("OnEnemyDeath");
        }
        _speed = 0;
        if (_collider != null)
        {
            _collider.enabled = false;
        }
        if (_audioSource != null && _audioSource.isActiveAndEnabled)
        {
            _audioSource.Play();
        }
        Destroy(this.gameObject, 2.37f);
    }
}
