using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float _speed = 1.5f;
    private Player player;
    private Animator _anim;
    private AudioSource _audioSource;
    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        GameObject p = GameObject.Find("Player");
        if (p != null)
        {
            player = p.GetComponent<Player>();
        }
        _anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);

        if (transform.position.y < -5f)
        {
        Destroy(this.gameObject);
        }
        
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
       // Debug.Log("Hit: " + other.transform.name);
    if (other.tag == "Player")
        {
            Player player = other.transform.GetComponent<Player>();

            if (player != null)
            {
                player.Damage();
            }
            // other.transform.GetComponent<Player>().Damage();
            _anim.SetTrigger("OnEnemyDeath");
            _speed = 0;
            GetComponent<Collider2D>().enabled = false;
            _audioSource.Play();
            Destroy(this.gameObject, 2.8f);
        }
        
    if (other.tag == "Laser")
        {
            Destroy(other.gameObject);
            if(player != null){
                player.AddScore(10);
            }
            _anim.SetTrigger("OnEnemyDeath");
            _speed = 0;
            GetComponent<Collider2D>().enabled = false;
            _audioSource.Play();
            Destroy(this.gameObject, 2.8f);
        }
    }
}
