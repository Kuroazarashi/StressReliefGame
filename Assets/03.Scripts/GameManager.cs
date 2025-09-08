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

    [Header("発狂スコア Settings")]
    private int currentHakkyouScore = 0;
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
    [SerializeField] private float gameDuration = 180f;
    private float currentTime;
    private bool isGameActive = false;

    [Header("Result Screen")]
    public float gameEndDelay = 5.0f;
    private bool isGameEnded = false;

    // シーンオブジェクトへの参照
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI timerText;
    private GameObject resultUI;
    private TextMeshProUGUI resultScoreText;
    private TextMeshProUGUI resultMessageText;
    private GameObject nextStageButton;
    private GameObject gameUI;
    private EnemyRagdollController enemyRagdollController;

    private int lastClearedStageIndex;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 01.で始まるシーンだけでなく、010.で始まるシーンでもInitializeGameを呼び出す
        if (scene.name.StartsWith("01.") || scene.name.StartsWith("010."))
        {
            InitializeGame();
        }
        else if (scene.name == "02.StageSelectScene")
        {
            lastClearedStageIndex = PlayerPrefs.GetInt("ClearedStage", 0);
        }
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
        SceneReferences sceneRefs = FindFirstObjectByType<SceneReferences>();
        if (sceneRefs == null)
        {
            Debug.LogError("SceneReferences object not found in the scene! Please add it and assign references.");
            isGameActive = false;
            return;
        }

        gameUI = sceneRefs.gameUI;
        scoreText = sceneRefs.scoreText;
        timerText = sceneRefs.timerText;
        resultUI = sceneRefs.resultUI;
        resultScoreText = sceneRefs.resultScoreText;
        resultMessageText = sceneRefs.resultMessageText;
        nextStageButton = sceneRefs.nextStageButton;
        enemyRagdollController = sceneRefs.enemyRagdollController;

        if (gameUI == null || scoreText == null || timerText == null || resultUI == null || enemyRagdollController == null)
        {
            Debug.LogError("One or more references in SceneReferences are not assigned! Please check the SceneReferences object in the scene.");
            isGameActive = false;
            return;
        }

        enemyRagdollController.SetRagdollState(false);

        currentHakkyouScore = 0;
        scoredObjects.Clear();
        UpdateScoreText();

        currentTime = gameDuration;
        isGameActive = true;
        isGameEnded = false;
        Time.timeScale = 1.0f;

        UpdateTimerUI();

        if (gameUI != null) gameUI.SetActive(true);
        if (resultUI != null) resultUI.SetActive(false);

        Debug.Log("Game Initialized with SceneReferences!");
    }

    public void AddScore(GameObject obj, string objTag)
    {
        if (isGameEnded) return;
        if (scoredObjects.Contains(obj)) return;

        int scoreToAdd = 0;

        Destructible destructible = obj.GetComponent<Destructible>();
        if (destructible != null)
        {
            scoreToAdd = destructible.scoreValue;
        }
        else
        {
            foreach (ScoreEntry entry in objectScoreValues)
            {
                if (entry.objectTag == objTag)
                {
                    scoreToAdd = entry.scoreValue;
                    break;
                }
            }
        }

        if (scoreToAdd > 0)
        {
            currentHakkyouScore += scoreToAdd;
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

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "発狂スコア <size=200%>" + currentHakkyouScore.ToString() + "</size>";
        }
    }

    private void ShowResultScreen(bool isClear)
    {
        if (gameUI != null) gameUI.SetActive(false);
        if (resultUI != null)
        {
            resultUI.SetActive(true);
            if (resultScoreText != null) resultScoreText.text = $"発狂スコア <size=200%>{currentHakkyouScore}</size>";

            if (isClear)
            {
                if (resultMessageText != null) resultMessageText.text = "スッキリしたか!?";

                // ネクストボタンの表示条件を明確にする
                // 最後にクリアしたステージが現在のステージより後の場合に、ネクストボタンを表示する
                int lastCleared = PlayerPrefs.GetInt("ClearedStage", 0);
                if (currentStageIndex < lastCleared)
                {
                    if (nextStageButton != null) nextStageButton.SetActive(true);
                }
                else
                {
                    if (nextStageButton != null) nextStageButton.SetActive(false);
                }
            }
            else
            {
                if (resultMessageText != null) resultMessageText.text = "Game Over!";
                if (nextStageButton != null) nextStageButton.SetActive(false);
            }
        }
    }

    public int GetCurrentScore() { return currentHakkyouScore; }
    public bool IsGameEnded() { return isGameEnded; }

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
                if (currentHakkyouScore >= scoreToClear)
                {
                    int nextStageToUnlock = currentStageIndex + 1;
                    int lastCleared = PlayerPrefs.GetInt("ClearedStage", 0);
                    if (lastCleared < nextStageToUnlock)
                    {
                        PlayerPrefs.SetInt("ClearedStage", nextStageToUnlock);
                        PlayerPrefs.Save(); // 追加：確実に保存する
                        Debug.Log($"Stage {nextStageToUnlock} unlocked!");
                    }
                }
            }
        }

        Debug.Log($"Game Ended! IsClear: {isClear}, Final 発狂スコア: {currentHakkyouScore}");
        StartCoroutine(ShowResultScreenWithDelay(isClear));
    }

    private IEnumerator ShowResultScreenWithDelay(bool isClear) { float duration = 1.0f; float start = Time.timeScale; float end = 1.0f; float elapsed = 0f; while (elapsed < duration) { elapsed += Time.unscaledDeltaTime; Time.timeScale = Mathf.Lerp(start, end, elapsed / duration); yield return null; } Time.timeScale = 1.0f; yield return new WaitForSecondsRealtime(gameEndDelay); ShowResultScreen(isClear); }
    public void NextStage() { Debug.Log("Next Stage button clicked!"); if (stageSettings != null && stageSettings.stages.Count > currentStageIndex + 1) { SceneManager.LoadScene(stageSettings.stages[currentStageIndex + 1].sceneName); } else { Debug.LogWarning("Next stage not found or not configured!"); } }
    public void RetryGame() { Debug.Log("Retry button clicked!"); SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void ReturnToTitle() { Debug.Log("Return to Title button clicked!"); SceneManager.LoadScene("00.TitleScene"); }
    public void ReturnToStageSelect() { Debug.Log("Return to Stage Select button clicked!"); SceneManager.LoadScene("02.StageSelectScene"); }
    public void SetCurrentStage(int stageIndex) { currentStageIndex = stageIndex; }
}
