using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField]
    private float _speed = 3f;

    [SerializeField]
    private int powerupID; // 0 Triple Shot, 1 Speed, 2 Shield, 3 Ammo, 4 Health, 5 Radial, 6 Negative, 7 Homing
    [SerializeField]
    private AudioClip _clip;

    [Header("Magnet (клавиша C)")]
    [SerializeField]
    private float _magnetSpeed = 9f;

    private Transform _player;

    public int ID { get { return powerupID; } }

    // Для пикапов, которые SpawnManager создаёт из других префабов (Negative, Homing).
    public void Configure(int id, Color tint)
    {
        powerupID = id;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = tint;
        }
    }

    void Start()
    {
        GameObject p = GameObject.Find("Player");
        if (p != null)
        {
            _player = p.transform;
        }
    }

    void Update()
    {
        // Зажатая C притягивает пикапы к игроку
        if (_player != null && Input.GetKey(KeyCode.C))
        {
            transform.position = Vector3.MoveTowards(transform.position, _player.position, _magnetSpeed * Time.deltaTime);
        }
        else
        {
            transform.Translate(Vector3.down * _speed * Time.deltaTime);
        }

        if (transform.position.y < -5f)
        {
            Destroy(this.gameObject);
        }
    }

    // Пикап сбит выстрелом врага.
    public void DestroyByEnemy()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            Player player = other.transform.GetComponent<Player>();
            if (_clip != null)
            {
                AudioSource.PlayClipAtPoint(_clip, transform.position);
            }
            if (player != null)
            {
                switch (powerupID)
                {
                    case 0:
                        player.TripleShotActive();
                        break;
                    case 1:
                        player.SpeedBoostActive();
                        break;
                    case 2:
                        player.ShieldActive();
                        break;
                    case 3:
                        player.AmmoRefill();
                        break;
                    case 4:
                        player.Heal();
                        break;
                    case 5:
                        player.RadialShotActive();
                        break;
                    case 6:
                        player.NegativeEffectActive();
                        break;
                    case 7:
                        player.HomingShotActive();
                        break;
                }
            }
            Destroy(this.gameObject);
        }
    }
}
