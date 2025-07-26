using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score Settings")]
    [SerializeField] private TextMeshProUGUI scoreText;
    private int currentScore = 0;

    private HashSet<GameObject> scoredObjects = new HashSet<GameObject>();

    // ★重要: [System.Serializable] は class ScoreEntry の直前に記述します。
    //         [Header("Object Score Values")] はその下の List<ScoreEntry> objectScoreValues の直前に記述します。

    [System.Serializable] // この属性は、この「クラス」がInspectorでシリアライズ可能であることを示す
    public class ScoreEntry
    {
        public string objectTag;
        public int scoreValue;
    }

    [Header("Object Score Values")] // この属性は、次の「フィールド」（変数）の上にヘッダーを表示する
    [SerializeField] private List<ScoreEntry> objectScoreValues;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        UpdateScoreText();
    }
    void Start()
    {
        // UpdateScoreText(); // Awakeで呼ぶのでここでは不要
    }

    public void AddScore(GameObject obj, string objTag)
    {
        if (scoredObjects.Contains(obj))
        {
            Debug.Log($"Object {obj.name} (Tag: {objTag}) already scored. No additional points.");
            return;
        }

        int scoreToAdd = 0;
        foreach (ScoreEntry entry in objectScoreValues)
        {
            if (entry.objectTag == objTag)
            {
                scoreToAdd = entry.scoreValue;
                break;
            }
        }

        if (scoreToAdd > 0)
        {
            currentScore += scoreToAdd;
            scoredObjects.Add(obj);
            UpdateScoreText();
            Debug.Log($"Score Added! Object: {obj.name} (Tag: {objTag}), Points: {scoreToAdd}, Total Score: {currentScore}");
        }
        else
        {
            Debug.LogWarning($"No score value defined for tag: {objTag}. Object: {obj.name}");
        }
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
        else
        {
            Debug.LogWarning("Score Text UI (TextMeshProUGUI) is not assigned in GameManager.");
        }
    }

    public void GameOver()
    {
        Debug.Log("Game Over! Final Score: " + currentScore);
    }

    public void GameClear()
    {
        Debug.Log("Game Clear! Final Score: " + currentScore);
    }
}