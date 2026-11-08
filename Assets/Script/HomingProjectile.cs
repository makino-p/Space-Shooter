using UnityEngine;

// Самонаводящаяся ракета игрока: ищет ближайшего врага и поворачивает к нему.
// Остаётся с тегом "Laser", поэтому враги, босс и астероид обрабатывают попадание как обычно.
public class HomingProjectile : MonoBehaviour
{
    [SerializeField]
    private float _speed = 8f;
    [SerializeField]
    private float _turnSpeed = 360f; // градусов в секунду
    [SerializeField]
    private float _lifetime = 4f;
    [SerializeField]
    private float _retargetInterval = 0.2f;

    private Transform _target;
    private float _retargetTimer;

    public static HomingProjectile Spawn(GameObject laserPrefab, Vector3 position, Color color)
    {
        GameObject go = Instantiate(laserPrefab, position, Quaternion.identity);
        go.name = "Homing_Missile";
        Laser laser = go.GetComponent<Laser>();
        if (laser != null)
        {
            laser.enabled = false;
            Destroy(laser);
        }
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = color;
        }
        return go.AddComponent<HomingProjectile>();
    }

    void Start()
    {
        Destroy(gameObject, _lifetime);
        FindClosestTarget();
    }

    void Update()
    {
        _retargetTimer -= Time.deltaTime;
        if (_target == null || !IsTargetAlive(_target) || _retargetTimer <= 0f)
        {
            FindClosestTarget();
            _retargetTimer = _retargetInterval;
        }

        if (_target != null)
        {
            Vector3 toTarget = _target.position - transform.position;
            float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
        }

        transform.Translate(Vector3.up * _speed * Time.deltaTime);

        Vector3 p = transform.position;
        if (p.y > 8f || p.y < -7f || Mathf.Abs(p.x) > 12f)
        {
            Destroy(gameObject);
        }
    }

    bool IsTargetAlive(Transform t)
    {
        Collider2D col = t.GetComponent<Collider2D>();
        return col != null && col.enabled;
    }

    void FindClosestTarget()
    {
        _target = null;
        float best = float.MaxValue;
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (!IsTargetAlive(enemy.transform))
            {
                continue;
            }
            float d = (enemy.transform.position - transform.position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                _target = enemy.transform;
            }
        }
    }
}
