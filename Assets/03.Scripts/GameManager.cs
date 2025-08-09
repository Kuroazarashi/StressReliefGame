using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Stage Settings")]
    public StageSettings stageSettings;
    [SerializeField] private int currentStageIndex;

    [Header("Score Settings")]
    [SerializeField] private TextMeshProUGUI scoreText;
    private int currentScore = 0;
    private HashSet<GameObject> scoredObjects = new HashSet<GameObject>();

    [System.Serializable]
    public class ScoreEntry
    {
        public string objectTag;
        public int scoreValue;
    }

    [Header("Object Score Values")]
    [SerializeField] private List<ScoreEntry> objectScoreValues;

    [Header("Timer Settings")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float gameDuration = 180f;
    private float currentTime;
    private bool isGameActive = false;

    [Header("Result Screen")]
    public GameObject resultUI;
    public TextMeshProUGUI resultScoreText;
    public TextMeshProUGUI resultMessageText;
    public GameObject nextStageButton;
    public float gameEndDelay = 5.0f;
    private bool isGameEnded = false;

    // ゲーム中のUIを格納する変数
    [Header("Game UI")]
    public GameObject gameUI;

    // EnemyRagdollControllerへの参照
    private EnemyRagdollController enemyRagdollController;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        if (isGameActive && !isGameEnded)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();

            if (currentTime <= 0f)
            {
                currentTime = 0f;
                UpdateTimerUI();
                isGameActive = false;
                EndGame(false);
            }
        }
    }

    public void InitializeGame()
    {
        currentScore = 0;
        scoredObjects.Clear();
        UpdateScoreText();

        currentTime = gameDuration;
        isGameActive = true;
        isGameEnded = false;
        Time.timeScale = 1.0f;

        UpdateTimerUI();

        GameObject enemyObject = GameObject.FindGameObjectWithTag("Enemy");
        if (enemyObject != null)
        {
            enemyRagdollController = enemyObject.GetComponent<EnemyRagdollController>();
            if (enemyRagdollController == null)
            {
                Debug.LogError("EnemyRagdollController not found on Enemy object with tag 'Enemy'!");
            }
            else
            {
                enemyRagdollController.SetRagdollState(false);
            }
        }
        else
        {
            Debug.LogError("Enemy object with tag 'Enemy' not found in scene!");
        }

        // ゲーム開始時にゲーム中のUIを表示
        if (gameUI != null)
        {
            gameUI.SetActive(true);
        }

        // リザルトUIを非表示にする
        if (resultUI != null)
        {
            resultUI.SetActive(false);
        }

        Debug.Log("Game Initialized!");
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void AddScore(GameObject obj, string objTag)
    {
        if (isGameEnded) return;

        if (scoredObjects.Contains(obj))
        {
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
        }

        if (objTag == "Enemy")
        {
            if (enemyRagdollController != null)
            {
                enemyRagdollController.SetRagdollState(true);

                Vector3 pushDirection = (obj.transform.up * 0.5f + Random.insideUnitSphere * 0.2f).normalized;
                float forceMagnitude = 700f;
                Vector3 hitPoint = obj.transform.position + Vector3.up * 1f;
                enemyRagdollController.ApplyForce(pushDirection * forceMagnitude, hitPoint);
            }

            EndGame(true);
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
    }

    public bool IsGameEnded()
    {
        return isGameEnded;
    }

    public void EndGame(bool isClear)
    {
        if (isGameEnded) return;

        isGameActive = false;
        isGameEnded = true;

        if (isClear)
        {
            if (stageSettings != null && stageSettings.stages.Count > currentStageIndex)
            {
                int scoreToClear = stageSettings.stages[currentStageIndex].scoreToClear;
                if (currentScore >= scoreToClear)
                {
                    int nextStageToUnlock = currentStageIndex + 1;
                    if (PlayerPrefs.GetInt("ClearedStage", 0) < nextStageToUnlock)
                    {
                        PlayerPrefs.SetInt("ClearedStage", nextStageToUnlock);
                        Debug.Log($"Stage {nextStageToUnlock} unlocked!");
                    }
                }
            }
        }

        Debug.Log($"Game Ended! IsClear: {isClear}, Final Score: {currentScore}");

        StartCoroutine(ShowResultScreenWithDelay(isClear));
    }

    private IEnumerator ShowResultScreenWithDelay(bool isClear)
    {
        float duration = 1.0f;
        float start = Time.timeScale;
        float end = 1.0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        Time.timeScale = 1.0f;

        yield return new WaitForSecondsRealtime(gameEndDelay);

        ShowResultScreen(isClear);
    }

    private void ShowResultScreen(bool isClear)
    {
        // リザルト画面が表示されるときにゲーム中のUIを非表示にする
        if (gameUI != null)
        {
            gameUI.SetActive(false);
        }

        if (resultUI != null)
        {
            resultUI.SetActive(true);

            resultScoreText.text = $"SCORE: {currentScore}";

            if (isClear)
            {
                resultMessageText.text = "Game Clear!";
                if (nextStageButton != null)
                {
                    // 次のステージが存在する場合のみボタンを有効にする
                    nextStageButton.SetActive(stageSettings != null && stageSettings.stages.Count > currentStageIndex + 1);
                }
            }
            else
            {
                resultMessageText.text = "Game Over!";
                if (nextStageButton != null)
                {
                    nextStageButton.SetActive(false);
                }
            }
        }
    }

    // 次のステージに進むためのメソッド
    public void NextStage()
    {
        Debug.Log("Next Stage button clicked!");
        if (stageSettings != null && stageSettings.stages.Count > currentStageIndex + 1)
        {
            Destroy(gameObject);
            SceneManager.LoadScene(stageSettings.stages[currentStageIndex + 1].sceneName);
        }
        else
        {
            Debug.LogWarning("Next stage not found or not configured!");
        }
    }

    // リトライボタンクリック時に呼び出されるメソッド
    public void RetryGame()
    {
        Debug.Log("Retry button clicked!");
        Destroy(gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // タイトル画面に戻るためのメソッド
    public void ReturnToTitle()
    {
        Debug.Log("Return to Title button clicked!");
        Destroy(gameObject);
        SceneManager.LoadScene("00.TitleScene");
    }

    // ステージ選択画面に戻るためのメソッド
    public void ReturnToStageSelect()
    {
        Debug.Log("Return to Stage Select button clicked!");
        Destroy(gameObject);
        SceneManager.LoadScene("02.StageSelectScene");
    }

    // 外部から現在のステージインデックスを設定するメソッド
    public void SetCurrentStage(int stageIndex)
    {
        currentStageIndex = stageIndex;
    }
}