using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    [SerializeField]
    private Text _ammoText;
    [SerializeField]
    private Text _outOfAmmoText;
    [SerializeField]
    private Text _shieldText;
    [SerializeField]
    private Text _restartText;
    private Game_Manager _gameManager;
    [SerializeField]
    private Text _gameOverText;
    [SerializeField]
    private Image LivesImage;
    [SerializeField]
    private Sprite[] _livesSprites;
    [SerializeField]
    private Text _scoreText;
    // Start is called before the first frame update
    void Start()
    {
        _scoreText.text = "Score " + 0;
        _gameOverText.gameObject.SetActive(false);
        _restartText.gameObject.SetActive(false);
        _gameManager = GameObject.Find("Game_Manager").GetComponent<Game_Manager>();
        
    }

    // Update is called once per frame
    public void  UpdateScore(int playerScore) 
    {
        _scoreText.text = "Score " + playerScore.ToString();
    }

    public void UpdateLives(int currentLives)
    {
    LivesImage.sprite = _livesSprites[currentLives];
    if(currentLives == 0)
    {
    GameOverSequence();

    }
    }
    void GameOverSequence()
    {
        _gameManager.GameOver();
        _gameOverText.gameObject.SetActive(true);
        _restartText.gameObject.SetActive(true);
        StartCoroutine(GameOverFlickerRoutine());
    }
    IEnumerator GameOverFlickerRoutine()
    {
        while(true)
        {
            _gameOverText.text = "GAME OVER";
            yield return new WaitForSeconds(0.5f);
            _gameOverText.text = "";
            yield return new WaitForSeconds(0.5f);  
        }
    }
    public void UpdateShield(int strength, int maxStrength)
    {
    if (_shieldText == null)
    {
        return;
    }

    _shieldText.gameObject.SetActive(strength > 0);
    _shieldText.text = "Shield " + strength + "/" + maxStrength;
    }
    public void UpdateAmmo(int ammo, int maxAmmo)
    {
    if (_ammoText != null)
    {
        _ammoText.text = "Ammo " + ammo + "/" + maxAmmo;
    }
    }

    public void ShowOutOfAmmo()
    {
    if (_outOfAmmoText != null)
    {
        StopCoroutine("OutOfAmmoRoutine");
        StartCoroutine("OutOfAmmoRoutine");
    }
    }

    // ---------- Thruster bar ----------
    [Header("Thruster Bar")]
    [SerializeField]
    private Vector2 _thrusterBarPosition = new Vector2(20f, 20f); // от левого нижнего угла экрана
    [SerializeField]
    private Vector2 _thrusterBarSize = new Vector2(200f, 16f);
    [SerializeField]
    private Color _thrusterReadyColor = new Color(0.2f, 0.9f, 1f, 1f);
    [SerializeField]
    private Color _thrusterLowColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField]
    private Color _thrusterOverheatColor = new Color(1f, 0.25f, 0.2f, 1f);

    private RectTransform _thrusterFillRect;
    private Image _thrusterFillImage;
    private Text _thrusterLabel;

    public void UpdateThruster(float normalizedCharge, bool overheated)
    {
        if (_thrusterFillRect == null)
        {
            CreateThrusterBar();
        }

        normalizedCharge = Mathf.Clamp01(normalizedCharge);
        _thrusterFillRect.anchorMax = new Vector2(normalizedCharge, 1f);

        if (overheated)
        {
            // мигание во время перегрева
            bool blinkOn = Mathf.FloorToInt(Time.time * 6f) % 2 == 0;
            _thrusterFillImage.color = _thrusterOverheatColor;
            _thrusterLabel.text = "THRUSTER OVERHEAT";
            _thrusterLabel.color = blinkOn ? _thrusterOverheatColor : Color.white;
        }
        else
        {
            _thrusterFillImage.color = normalizedCharge < 0.3f ? _thrusterLowColor : _thrusterReadyColor;
            _thrusterLabel.text = "THRUSTER";
            _thrusterLabel.color = Color.white;
        }
    }

    void CreateThrusterBar()
    {
        // Фон полоски
        GameObject bar = new GameObject("Thruster_Bar", typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(transform, false);
        RectTransform barRect = bar.GetComponent<RectTransform>();
        barRect.anchorMin = barRect.anchorMax = barRect.pivot = Vector2.zero;
        barRect.anchoredPosition = _thrusterBarPosition;
        barRect.sizeDelta = _thrusterBarSize;
        bar.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

        // Заполнение (ширина = заряд)
        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(bar.transform, false);
        _thrusterFillRect = fill.GetComponent<RectTransform>();
        _thrusterFillRect.anchorMin = Vector2.zero;
        _thrusterFillRect.anchorMax = Vector2.one;
        _thrusterFillRect.offsetMin = new Vector2(2f, 2f);
        _thrusterFillRect.offsetMax = new Vector2(-2f, -2f);
        _thrusterFillImage = fill.GetComponent<Image>();
        _thrusterFillImage.color = _thrusterReadyColor;

        // Подпись над полоской
        GameObject label = new GameObject("Label", typeof(RectTransform), typeof(Text));
        label.transform.SetParent(bar.transform, false);
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.pivot = Vector2.zero;
        labelRect.anchoredPosition = new Vector2(0f, 2f);
        labelRect.sizeDelta = new Vector2(0f, 20f);
        _thrusterLabel = label.GetComponent<Text>();
        _thrusterLabel.font = _scoreText != null ? _scoreText.font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        _thrusterLabel.fontSize = 14;
        _thrusterLabel.alignment = TextAnchor.LowerLeft;
        _thrusterLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
        _thrusterLabel.text = "THRUSTER";
    }

   IEnumerator OutOfAmmoRoutine()
   {
    _outOfAmmoText.gameObject.SetActive(true);
    yield return new WaitForSeconds(0.8f);
    _outOfAmmoText.gameObject.SetActive(false);
    }

    // ---------- Сообщения в центре экрана (волны, босс, победа) ----------
    private Text _messageText;
    private Coroutine _messageRoutine;

    public void ShowWave(int wave, bool isFinal)
    {
        string text = isFinal ? "FINAL WAVE" : "WAVE " + wave;
        ShowMessage(text, 2f, Color.white);
    }

    // duration <= 0 — сообщение остаётся на экране.
    public void ShowMessage(string text, float duration, Color color)
    {
        if (_messageText == null)
        {
            GameObject go = new GameObject("Message_Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(transform, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 60f);
            rect.sizeDelta = new Vector2(600f, 60f);
            _messageText = go.GetComponent<Text>();
            _messageText.font = _scoreText != null ? _scoreText.font : Resources.GetBuiltinResource<Font>("Arial.ttf");
            _messageText.fontSize = 36;
            _messageText.alignment = TextAnchor.MiddleCenter;
            _messageText.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        if (_messageRoutine != null)
        {
            StopCoroutine(_messageRoutine);
        }
        _messageText.text = text;
        _messageText.color = color;
        _messageText.gameObject.SetActive(true);
        if (duration > 0f)
        {
            _messageRoutine = StartCoroutine(HideMessageRoutine(duration));
        }
    }

    IEnumerator HideMessageRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        _messageText.gameObject.SetActive(false);
        _messageRoutine = null;
    }

    // ---------- Здоровье босса ----------
    private GameObject _bossBar;
    private RectTransform _bossFillRect;

    public void ShowBossHealth(float normalized)
    {
        if (_bossBar == null)
        {
            _bossBar = new GameObject("Boss_Health_Bar", typeof(RectTransform), typeof(Image));
            _bossBar.transform.SetParent(transform, false);
            RectTransform barRect = _bossBar.GetComponent<RectTransform>();
            barRect.anchorMin = barRect.anchorMax = barRect.pivot = new Vector2(0.5f, 1f);
            barRect.anchoredPosition = new Vector2(0f, -20f);
            barRect.sizeDelta = new Vector2(360f, 18f);
            _bossBar.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(_bossBar.transform, false);
            _bossFillRect = fill.GetComponent<RectTransform>();
            _bossFillRect.anchorMin = Vector2.zero;
            _bossFillRect.anchorMax = Vector2.one;
            _bossFillRect.offsetMin = new Vector2(2f, 2f);
            _bossFillRect.offsetMax = new Vector2(-2f, -2f);
            fill.GetComponent<Image>().color = new Color(1f, 0.25f, 0.25f, 1f);

            GameObject label = new GameObject("Label", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(_bossBar.transform, false);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            Text t = label.GetComponent<Text>();
            t.font = _scoreText != null ? _scoreText.font : Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = 13;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.text = "BOSS";
        }

        _bossBar.SetActive(true);
        _bossFillRect.anchorMax = new Vector2(Mathf.Clamp01(normalized), 1f);
    }

    public void HideBossHealth()
    {
        if (_bossBar != null)
        {
            _bossBar.SetActive(false);
        }
    }
}
