using UnityEngine;
using TMPro; // TextMeshProを使用するために必要

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // シングルトンパターン
    public TextMeshProUGUI scoreText; // スコア表示用のTextMeshProUGUI
    private int currentScore = 0;

    void Awake()
    {
        // シングルトンインスタンスの設定
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        UpdateScoreText();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
    }
}