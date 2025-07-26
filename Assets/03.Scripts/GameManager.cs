using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshProを使用するために必要

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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

    // ★追加: タイマー関連の変数
    [Header("Timer Settings")]
    [SerializeField] private TextMeshProUGUI timerText; // タイマー表示用のUI Text
    [SerializeField] private float gameDuration = 180f; // ゲームの制限時間（秒）。デフォルト3分 = 180秒
    private float currentTime; // 現在の残り時間
    private bool isGameActive = false; // ゲームがアクティブ状態か（タイマーが動いているか）

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

        // ★追加: ゲーム開始時にタイマーを初期化し、ゲームをアクティブにする
        InitializeGame();
    }

    void Update()
    {
        // ★追加: ゲームがアクティブな場合のみタイマーを更新
        if (isGameActive)
        {
            currentTime -= Time.deltaTime; // デルタタイムを使って時間を減らす
            UpdateTimerUI(); // タイマーUIを更新

            if (currentTime <= 0f)
            {
                currentTime = 0f; // 時間が0を下回らないようにする
                UpdateTimerUI();
                EndGame(false); // 時間切れでゲームオーバー
            }
        }
    }

    // ★追加: ゲーム開始時の初期化処理
    public void InitializeGame()
    {
        currentScore = 0;
        scoredObjects.Clear(); // スコア済みオブジェクトリストもクリア
        UpdateScoreText();

        currentTime = gameDuration; // 制限時間をセット
        isGameActive = true; // ゲームをアクティブ状態にする
        UpdateTimerUI(); // 初期タイマー表示を更新

        Debug.Log("Game Started!");
    }

    // ★追加: タイマーUIを更新するプライベートメソッド
    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // 時間を「分:秒」形式で表示
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        else
        {
            Debug.LogWarning("Timer Text UI (TextMeshProUGUI) is not assigned in GameManager.");
        }
    }

    public void AddScore(GameObject obj, string objTag)
    {
        // 既存のAddScoreメソッド...
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

        // ★追加: 敵を吹っ飛ばした場合、その時点でゲームクリア
        // ただし、この段階では「ターゲットとなる敵」のタグが確定していないので、仮に"Enemy"とする
        if (objTag == "Enemy") // ここはターゲットとなる敵のタグに置き換える
        {
            EndGame(true); // 敵を吹っ飛ばしたのでゲームクリア
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

    // ★変更: ゲーム終了処理を一本化し、成功/失敗を引数で渡す
    public void EndGame(bool isClear)
    {
        if (!isGameActive) return; // すでにゲームが終了している場合は何もしない

        isGameActive = false; // ゲームを非アクティブにする（タイマーを停止）
        Time.timeScale = 0f; // ゲーム内の時間を停止（物理演算なども停止）

        if (isClear)
        {
            Debug.Log("Game Clear! Final Score: " + currentScore + ", Time Left: " + currentTime);
            // 今後のリザルト画面表示や、次のステージへの遷移ロジックをここに追加
        }
        else
        {
            Debug.Log("Game Over! Final Score: " + currentScore + ", Time Expired.");
            // 今後のゲームオーバー画面表示ロジックをここに追加
        }
        // TODO: ここでリザルト画面などを表示する処理を実装
    }

    // ゲームをリセットするための公開メソッド（リトライボタンなどで使用）
    public void RestartGame()
    {
        Time.timeScale = 1f; // 時間を元に戻す
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        // シーンをリロードしてゲームをリセット
        // DontDestroyOnLoadを使っているため、GameManager自体はシーンロード後も残りますが、
        // AwakeでInitializeGameが再度呼ばれるため問題ありません。
    }
}