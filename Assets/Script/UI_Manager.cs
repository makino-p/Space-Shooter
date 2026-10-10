using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    private GameManager _gameManager;
    [SerializedField]
    private Text _restartText;
    [SerializedField]
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
        _gameOverText.GameObject.SetActive(false);
        _gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
        
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
         _gameOverText.GameObject.SetActive(true);
         _restartText.GameObject.SetActive(true);
        StartCoroutine(GameOverFlickerRoutine());
    }
    IEnumerator GameOverFlickerRoutine()
    {
        while(true)
        {
            _gameOverText.text = "GAME OVER";
            yield return new WairForSeconds(0.5f);
            _gameOverText.text = "";
            yield return new WairForSeconds(0.5f);  
        }
    }
}
