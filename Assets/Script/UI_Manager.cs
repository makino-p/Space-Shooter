using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{ 
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
        
    }

    // Update is called once per frame
    public void  UpdateScore(int playerScore) 
    {
        _scoreText.text = "Score " + playerScore.ToString();
    }

    public void UpdateLives(int currentLives)
    {
      LivesImage.sprite = _livesSprites[currentLives];
    }
}
