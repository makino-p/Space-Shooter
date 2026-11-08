using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour
{  
    private CameraShake _cameraShake;
    [Header("Radial Shot")]
    [SerializeField]
    private GameObject _radialShotPrefab;
    [SerializeField]
    private float _radialShotDuration = 5f;
    private bool _isRadialShotActive = false;
    private Coroutine _radialRoutine;
    [Header("Ammo")]
    private int _maxAmmo = 15;
    private int _ammo;
    [SerializeField]
    private AudioClip _outOfAmmoClip;
    [SerializeField]
    private GameObject _tripleShotPrefab;
    [SerializeField]
    private GameObject _shieldVisualizer;

    [SerializeField]
    [Header("Shield")]
    private const int MaxShieldStrength = 3;
    private int _shieldStrength = 0;
    [SerializeField]
    private Color _shieldFullColor = new Color(0.2f, 0.85f, 1f, 1f);   // 3 попадания
    [SerializeField]
    private Color _shieldMediumColor = new Color(1f, 0.9f, 0.2f, 1f);  // 2 попадания
    [SerializeField]
    private Color _shieldWeakColor = new Color(1f, 0.2f, 0.2f, 1f);    // 1 попадание
    private SpriteRenderer _shieldRenderer; 
    private bool _isTripleShotActive = false;
    private bool _isSpeedBoostActive = false;
    private bool _isSpeedShiftActive = false;

    [SerializeField]
    private int _maxLives = 3;
    private float _canFire = -1f;
    [SerializeField]
    private float _fireRate = 0.2f;
    [SerializeField]
    private float _speed = 5.5f;
    [Header("Thrusters (Left Shift)")]
    [SerializeField]
    private float _shiftMultiplier = 1.75f;
    [SerializeField]
    private float _maxThrusterCharge = 100f;
    [SerializeField]
    private float _thrusterDrainRate = 35f;      // расход в секунду при зажатом Shift
    [SerializeField]
    private float _thrusterRechargeRate = 25f;   // восстановление в секунду
    [SerializeField]
    private float _thrusterRechargeDelay = 0.5f; // пауза перед восстановлением после отпускания Shift
    [SerializeField]
    private float _thrusterCooldown = 2f;        // перегрев: пауза, если заряд кончился полностью
    private float _thrusterCharge;
    private bool _isThrusterOverheated = false;
    private float _thrusterRechargeStartTime = 0f;
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private int _lives = 3;
    private SpawnManager _spawnManager; 
    [SerializeField]
    private float _speedMultiplier = 2.0f;
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
    _cameraShake = Camera.main.GetComponent<CameraShake>();
    transform.position = new Vector3(0, 0, 0);
    _audioSource = GetComponent<AudioSource>();
    _uiManager = GameObject.Find("Canvas").GetComponent<UI_Manager>();
    _spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();

    _ammo = _maxAmmo;
    _uiManager.UpdateAmmo(_ammo, _maxAmmo);

    if (_spawnManager == null)
    {
        Debug.LogError("The Spawn Manager is NULL");
    }
    if (_audioSource == null)
    {
        Debug.Log("Audio Source of player null");
    }
    else
    {
        _audioSource.clip = _laserSoundClip;
    }
    if (_shieldVisualizer != null)
    {
        _shieldRenderer = _shieldVisualizer.GetComponent<SpriteRenderer>();
        _shieldVisualizer.SetActive(false);
    }
    _uiManager.UpdateShield(0, MaxShieldStrength);

    _thrusterCharge = _maxThrusterCharge;
    _uiManager.UpdateThruster(1f, false);
}

    // Update is called once per frame
    void Update()
    {
        CalculateMovement();
        
        HandleThrusters();

if (Input.GetKeyDown(KeyCode.Space) && Time.time > _canFire)
{
    if (_ammo > 0)
    {
        FireLaser();
    }
    else
    {
        _canFire = Time.time + _fireRate;
        _uiManager.ShowOutOfAmmo();
        if (_outOfAmmoClip != null)
        {
            AudioSource.PlayClipAtPoint(_outOfAmmoClip, transform.position);
        }
    }
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
        _ammo--;
        _uiManager.UpdateAmmo(_ammo, _maxAmmo);
        if (_isHomingActive == true)
        {
          HomingProjectile.Spawn(_laserPrefab, transform.position + new Vector3(0, 1.2f, 0), _homingColor);
        }
        else if (_isRadialShotActive == true)
        {
          Instantiate(_radialShotPrefab, transform.position, Quaternion.identity);
        }
        else if (_isTripleShotActive == true)
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

        if (_shieldStrength > 0)
        {
           _shieldStrength--;
           UpdateShieldVisual();
           return;
        }

        _lives -= 1;                                       

        if (_cameraShake != null)
        {
         _cameraShake.Shake();
        }
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
       _shieldStrength = MaxShieldStrength;
       UpdateShieldVisual();
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

    // Заряд двигателей: тратится, пока зажат Left Shift, и восстанавливается после отпускания.
    // Если заряд кончился полностью — перегрев: ускорение заблокировано, пока заряд не восстановится до 100%.
    void HandleThrusters()
    {
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift);
        bool canBoost = shiftHeld && !_isThrusterOverheated && _thrusterCharge > 0f;

        if (canBoost)
        {
            SpeedShiftActive();
            _thrusterCharge -= _thrusterDrainRate * Time.deltaTime;

            if (_thrusterCharge <= 0f)
            {
                _thrusterCharge = 0f;
                _isThrusterOverheated = true;
                _thrusterRechargeStartTime = Time.time + _thrusterCooldown;
                SpeedShiftDeactive();
            }
            else
            {
                _thrusterRechargeStartTime = Time.time + _thrusterRechargeDelay;
            }
        }
        else
        {
            SpeedShiftDeactive();

            if (Time.time >= _thrusterRechargeStartTime && _thrusterCharge < _maxThrusterCharge)
            {
                _thrusterCharge += _thrusterRechargeRate * Time.deltaTime;
                if (_thrusterCharge >= _maxThrusterCharge)
                {
                    _thrusterCharge = _maxThrusterCharge;
                    _isThrusterOverheated = false;
                }
            }
        }

        if (_uiManager != null)
        {
            _uiManager.UpdateThruster(_thrusterCharge / _maxThrusterCharge, _isThrusterOverheated);
        }
    }
    void UpdateShieldVisual()
    {
    if (_uiManager != null)
    {
        _uiManager.UpdateShield(_shieldStrength, MaxShieldStrength);
    }


    if (_shieldVisualizer == null)
    {
        return;
    }

    if (_shieldStrength <= 0)
    {
        _shieldVisualizer.SetActive(false);
        return;
    }

    _shieldVisualizer.SetActive(true);

    if (_shieldRenderer == null)
    {
        return;
    }

    switch (_shieldStrength)
    {
        case 3:
            _shieldRenderer.color = _shieldFullColor;
            break;
        case 2:
            _shieldRenderer.color = _shieldMediumColor;
            break;
        default:
            _shieldRenderer.color = _shieldWeakColor;
            break;
    }
    }
    public void AmmoRefill()
    {
        _ammo = _maxAmmo;
        _uiManager.UpdateAmmo(_ammo, _maxAmmo);
    }
    public void Heal()
    {
    if (_lives >= _maxLives)
    {
        return;
    }

    _lives++;

    // двигатели: при 3 жизнях оба выключены, при 2 горит один, при 1 оба
    if (_lives >= 3)
    {
        rightEngine.SetActive(false);
        leftEngine.SetActive(false);
    }
    else if (_lives == 2)
    {
        leftEngine.SetActive(false);
    }

      _uiManager.UpdateLives(_lives);
    }
    public void RadialShotActive()
    {
      _isRadialShotActive = true;
    if (_radialRoutine != null)
    {
        StopCoroutine(_radialRoutine);
    }
      _radialRoutine = StartCoroutine(RadialShotPowerDownRoutine());
    }

    IEnumerator RadialShotPowerDownRoutine()
    {
      yield return new WaitForSeconds(_radialShotDuration);
      _isRadialShotActive = false;
    }

    // ---------- Homing (редкий паверап) ----------
    [Header("Homing Shot")]
    [SerializeField]
    private float _homingDuration = 5f;
    [SerializeField]
    private Color _homingColor = new Color(1f, 0.4f, 1f, 1f);
    private bool _isHomingActive = false;
    private Coroutine _homingRoutine;

    public int Ammo { get { return _ammo; } }

    public void HomingShotActive()
    {
        _isHomingActive = true;
        if (_homingRoutine != null)
        {
            StopCoroutine(_homingRoutine);
        }
        _homingRoutine = StartCoroutine(HomingPowerDownRoutine());
    }

    IEnumerator HomingPowerDownRoutine()
    {
        yield return new WaitForSeconds(_homingDuration);
        _isHomingActive = false;
    }

    // ---------- Негативный паверап: сбой двигателей ----------
    // На 4 секунды: скорость вдвое ниже, корабль мигает красным, теряет 3 патрона.
    [Header("Negative Powerup")]
    [SerializeField]
    private float _negativeDuration = 4f;
    [SerializeField]
    private float _negativeSpeedMultiplier = 0.5f;
    [SerializeField]
    private int _negativeAmmoLoss = 3;
    private bool _isNegativeActive = false;

    public void NegativeEffectActive()
    {
        _ammo = Mathf.Max(0, _ammo - _negativeAmmoLoss);
        _uiManager.UpdateAmmo(_ammo, _maxAmmo);

        if (_isNegativeActive)
        {
            return; // замедление не складывается
        }
        StartCoroutine(NegativeEffectRoutine());
    }

    IEnumerator NegativeEffectRoutine()
    {
        _isNegativeActive = true;
        _speed *= _negativeSpeedMultiplier;
        if (_uiManager != null)
        {
            _uiManager.ShowMessage("ENGINE MALFUNCTION", 1.5f, new Color(1f, 0.3f, 0.3f));
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float end = Time.time + _negativeDuration;
        while (Time.time < end)
        {
            if (sr != null)
            {
                sr.color = Mathf.FloorToInt(Time.time * 8f) % 2 == 0 ? new Color(1f, 0.4f, 0.4f) : Color.white;
            }
            yield return null;
        }
        if (sr != null)
        {
            sr.color = Color.white;
        }

        _speed /= _negativeSpeedMultiplier;
        _isNegativeActive = false;
    }
}


