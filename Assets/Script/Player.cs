using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour
{   
    [SerializeField]
    private GameObject _tripleShotPrefab;
    [SerializeField]
    private GameObject _shieldVisualizer;


    [SerializeField]
    private bool isShieldActive = false;
    private bool _isTripleShotActive = false;
    private bool _isSpeedBoostActive = false;
    private bool _isSpeedShiftActive = false;


    private float _canFire = -1f;
    [SerializeField]
    private float _fireRate = 0.2f;
    [SerializeField]
    private float _speed = 5.5f;
    private float _shiftMultiplier = 5.5f;
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private int _lives = 3;
    private SpawnManager _spawnManager; 
    [SerializeField]
    private float _speedMultiplier = 2.0f;
    private float currentSpeed;
    [SerializeField]
    private int _score; 
    private UI_Manager _uiManager;

    [SerializeField]
    private GameObject rightEngine, leftEngine;

    [SerializeField]
    private AudioClip _laserSoundClip;

    private AudioSource _audioSource;

    // Start is called before the first frame update
    void Start()
    {
        transform.position  = new Vector3(0,0,0);
        _audioSource = GetComponent<AudioSource>();
        _uiManager = GameObject.Find("Canvas").GetComponent<UI_Manager>();
        _spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
        if (_spawnManager == null)
        {
            Debug.LogError("The Spawn Manager is NULL");
        }
        if(_audioSource == null)
        {
            Debug.Log("Audio Source of player null");
        }else
        {
            _audioSource.clip = _laserSoundClip;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CalculateMovement();
        
    if (Input.GetKeyDown(KeyCode.LeftShift))
    {
        SpeedShiftActive();
    }
    if (Input.GetKeyUp(KeyCode.LeftShift))
    {
        SpeedShiftDeactive();
    }

    if (Input.GetKeyDown(KeyCode.Space) && Time.time > _canFire)
    {
        FireLaser();
    }
    }
    void CalculateMovement()
    { 
    float horizontalInput = Input.GetAxis("Horizontal");
    float verticalInput = Input.GetAxis("Vertical");
    Vector3 direction = new Vector3(horizontalInput, verticalInput, 0);

    transform.Translate(_speed * Time.deltaTime * direction);

    transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, -3.8f, 0), 0);

    if (transform.position.x > 11)
    {
        transform.position = new Vector3(-11, transform.position.y, 0);
    }
    else if (transform.position.x < -11)
    {
        transform.position = new Vector3(11, transform.position.y, 0);
    }
    }


    void FireLaser()
    {
        _canFire = Time.time + _fireRate;

        if (_isTripleShotActive == true)
        {
            Instantiate(_tripleShotPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Instantiate(_laserPrefab, transform.position + new Vector3(0, 1.8f, 0), Quaternion.identity);
        }
        _audioSource.Play();
    }
    
    public void Damage()

    {

        if (isShieldActive == true)
        {
            isShieldActive = false;
            _shieldVisualizer.SetActive(false);
            return;
        }

        _lives -= 1;

        if (_lives == 2)
        {
            rightEngine.SetActive(true);
        }
        else if(_lives == 1)
        {
            leftEngine.SetActive(true);
        }
        if (_lives < 1)
        {
            _lives = 0;
            _spawnManager.OnPlayerDeath();
        }

        _uiManager.UpdateLives(_lives);

        if (_lives == 0)
        {
            Destroy(this.gameObject);
        }
    }
    
    public void TripleShotActive()
    {
        _isTripleShotActive = true;
        StartCoroutine(TripleShotPowerDownRoutine());
    }

    IEnumerator TripleShotPowerDownRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        _isTripleShotActive = false;
    }

    public void ShieldActive()
    {
        isShieldActive = true;
        _shieldVisualizer.SetActive(true);
    }
public void SpeedBoostActive()
{
    if (_isSpeedBoostActive == true)
    {
        return;
    }

    _isSpeedBoostActive = true;
    _speed *= _speedMultiplier;
    StartCoroutine(SpeedBoostPowerDownRoutine());
}
    IEnumerator SpeedBoostPowerDownRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        _isSpeedBoostActive = false;
        _speed /= _speedMultiplier;
    }

    public void AddScore(int points)
    {
        _score += points; 
        _uiManager.UpdateScore(_score);
    }


public void SpeedShiftActive()
{
    if (_isSpeedShiftActive == true)
    {
        return;
    }

    _isSpeedShiftActive = true;
    _speed *= _shiftMultiplier;
}

public void SpeedShiftDeactive()
{
    if (_isSpeedShiftActive == false)
    {
        return;
    }

    _isSpeedShiftActive = false;
    _speed /= _shiftMultiplier;
}
}


