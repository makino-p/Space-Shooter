using UnityEngine;

// Вражеский выстрел. Создаётся из префаба лазера игрока: перекрашивается,
// отключается обычный Laser и снимается тег "Laser", чтобы не попадать по врагам.
public class EnemyProjectile : MonoBehaviour
{
    private Vector3 _direction = Vector3.down;
    private float _speed = 7f;

    public static EnemyProjectile Spawn(GameObject laserPrefab, Vector3 position, Vector3 direction, float speed, Color color)
    {
        if (laserPrefab == null)
        {
            return null;
        }

        GameObject go = Instantiate(laserPrefab, position, Quaternion.identity);
        PrepareFromLaser(go, color);

        EnemyProjectile projectile = go.AddComponent<EnemyProjectile>();
        projectile._direction = direction.normalized;
        projectile._speed = speed;

        // повернуть спрайт по направлению полёта
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        go.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        return projectile;
    }

    // Убирает у копии лазера поведение лазера игрока.
    public static void PrepareFromLaser(GameObject go, Color color)
    {
        go.tag = "Untagged";
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
    }

    void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;

        Vector3 p = transform.position;
        if (p.y < -7f || p.y > 8f || Mathf.Abs(p.x) > 12f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.Damage();
            }
            Destroy(gameObject);
            return;
        }

        // враги стреляют по пикапам, чтобы игрок их не получил
        Powerup powerup = other.GetComponent<Powerup>();
        if (powerup != null)
        {
            powerup.DestroyByEnemy();
            Destroy(gameObject);
        }
    }
}
