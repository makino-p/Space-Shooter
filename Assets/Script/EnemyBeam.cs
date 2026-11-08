using UnityEngine;

// Уникальный снаряд: лазерный луч. Сначала тонкий прозрачный "прицел" (предупреждение),
// затем широкий луч, который наносит урон игроку один раз.
public class EnemyBeam : MonoBehaviour
{
    private Transform _owner;
    private Vector3 _offset;
    private float _length;
    private float _warmupTime;
    private float _activeTime;
    private float _timer;
    private bool _isActive;
    private bool _hasHit;
    private SpriteRenderer _renderer;
    private Collider2D _collider;
    private Color _color;
    private float _spriteHeight = 3f;
    private float _spriteWidth = 1f;
    private float _warnWidth = 0.03f;
    private float _activeWidth = 0.35f;

    // direction: Vector3.down для обычного врага; length в юнитах мира.
    public static EnemyBeam Spawn(GameObject laserPrefab, Transform owner, Vector3 offset, float length,
                                  float warmup, float active, Color color, float activeWidth = 0.35f)
    {
        if (laserPrefab == null || owner == null)
        {
            return null;
        }

        GameObject go = Instantiate(laserPrefab, owner.position + offset, Quaternion.identity);
        EnemyProjectile.PrepareFromLaser(go, color);
        go.name = "Enemy_Beam";

        EnemyBeam beam = go.AddComponent<EnemyBeam>();
        beam._owner = owner;
        beam._offset = offset;
        beam._length = length;
        beam._warmupTime = warmup;
        beam._activeTime = active;
        beam._color = color;
        beam._activeWidth = activeWidth;
        beam.Init();
        return beam;
    }

    void Init()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        if (_renderer != null && _renderer.sprite != null)
        {
            _spriteHeight = _renderer.sprite.bounds.size.y;
            _spriteWidth = _renderer.sprite.bounds.size.x;
        }
        if (_collider != null)
        {
            _collider.enabled = false;
        }
        SetWidth(_warnWidth, 0.35f);
        FollowOwner();
    }

    void SetWidth(float width, float alpha)
    {
        transform.localScale = new Vector3(width / _spriteWidth, _length / _spriteHeight, 1f);
        if (_renderer != null)
        {
            Color c = _color;
            c.a = alpha;
            _renderer.color = c;
        }
    }

    void FollowOwner()
    {
        if (_owner == null)
        {
            Destroy(gameObject);
            return;
        }
        // луч идёт вниз от точки выстрела
        transform.position = _owner.position + _offset + Vector3.down * (_length / 2f);
    }

    void Update()
    {
        FollowOwner();
        _timer += Time.deltaTime;

        if (!_isActive)
        {
            // мигание "прицела"
            float blink = Mathf.PingPong(Time.time * 6f, 1f);
            SetWidth(_warnWidth, 0.2f + 0.4f * blink);

            if (_timer >= _warmupTime)
            {
                _isActive = true;
                _timer = 0f;
                SetWidth(_activeWidth, 1f);
                if (_collider != null)
                {
                    _collider.enabled = true;
                }
            }
        }
        else
        {
            // лёгкая пульсация ширины активного луча
            float pulse = 1f + 0.15f * Mathf.Sin(Time.time * 40f);
            SetWidth(_activeWidth * pulse, 1f);

            if (_timer >= _activeTime)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasHit || !_isActive)
        {
            return;
        }
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                _hasHit = true;
                player.Damage();
            }
        }
    }
}
