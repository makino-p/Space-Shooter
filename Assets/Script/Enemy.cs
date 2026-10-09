using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float _speed = 1.5f;
    private Player player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);

        if (transform.position.y < -5f)
        {
        float randomX = Random.Range(-7f, 7f);
        transform.position = new Vector3(randomX, 7, 0);
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
            Destroy(this.gameObject);
        }
        
        if (other.tag == "Laser")
        {
            Destroy(other.gameObject);
            if(player != null){
                player.AddScore(10);
            }
            Destroy(this.gameObject);
        }
    }
}
