using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField]
    private float _speed = 10.0f;
    // Start is called before the first frame update

    
    // Update is called once per frame


    void Update()
    {
        transform.Translate(Vector3.up * _speed * Time.deltaTime);

        if (transform.position.y > 8f || transform.position.y < -6f || Mathf.Abs(transform.position.x) > 12f)
        {
          if (transform.parent != null && transform.parent.childCount <= 1)
            {
               Destroy(transform.parent.gameObject);
            }
          Destroy(this.gameObject); 
        } 
    }
}



