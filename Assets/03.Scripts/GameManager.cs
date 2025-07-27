using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshProを使用するために必要
using UnityEngine.SceneManagement; // ★この行は正しいです

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

    // ★追加：EnemyRagdollControllerへの参照
    private EnemyRagdollController enemyRagdollController;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // GameManagerはシーン遷移しても破壊されないようにする

        UpdateScoreText();

        // ★追加: ゲーム開始時にタイマーを初期化し、ゲームをアクティブにする
        InitializeGame();

        // ★追加：シーン内のEnemyオブジェクトを探して、EnemyRagdollControllerの参照を取得
        // Enemyオブジェクトに"Enemy"タグが付いていることを前提とします
        GameObject enemyObject = GameObject.FindGameObjectWithTag("Enemy");
        if (enemyObject != null)
        {
            enemyRagdollController = enemyObject.GetComponent<EnemyRagdollController>();
            if (enemyRagdollController == null)
            {
                Debug.LogError("EnemyRagdollController not found on Enemy object with tag 'Enemy'!");
            }
        }
        else
        {
            Debug.LogError("Enemy object with tag 'Enemy' not found in scene!");
        }
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

        // ★ラグドールを無効化（アニメーション状態に戻す）
        if (enemyRagdollController != null)
        {
            enemyRagdollController.SetRagdollState(false);
        }

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
        // 既にスコア加算済みのオブジェクトか確認
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
            scoredObjects.Add(obj); // スコア加算済みリストに追加
            UpdateScoreText();
            Debug.Log($"Score Added! Object: {obj.name} (Tag: {objTag}), Points: {scoreToAdd}, Total Score: {currentScore}");
        }
        else
        {
            Debug.LogWarning($"No score value defined for tag: {objTag}. Object: {obj.name}");
        }

        // 敵を吹っ飛ばした場合、その時点でゲームクリア
        if (objTag == "Enemy")
        {
            if (enemyRagdollController != null)
            {
                enemyRagdollController.SetRagdollState(true); // ラグドールを有効化

                // 吹っ飛ぶ力を加える
                // 現在のAddScoreにはヒットした位置情報がないため、プレイヤーの位置を基準に仮の力を設定します
                // プレイヤーの参照をGameManagerに持つか、AttackColliderHandlerからhitPointとforceDirectionを渡す方がより正確です
                // 今回は仮に、敵の少し上方向から、敵の中心から外側へ押す力を加えます
                Vector3 forceDirection = (obj.transform.position - transform.position).normalized; // GameManagerから敵への方向
                // GameManagerの位置が原点付近だと、方向が適切でなくなる可能性があります。
                // 理想的にはプレイヤーのパンチ/キックが発生した位置からの方向を取得すべきです。
                // ここでは仮に上方向と少しランダムな横方向の力を混ぜてみます
                Vector3 pushDirection = (obj.transform.up * 0.5f + Random.insideUnitSphere * 0.2f).normalized; // 上方向+ランダムな横方向
                float forceMagnitude = 700f; // 吹っ飛ぶ力の大きさ。調整してください。
                Vector3 hitPoint = obj.transform.position + Vector3.up * 1f; // 敵の中心より少し上

                enemyRagdollController.ApplyForce(pushDirection * forceMagnitude, hitPoint);
            }
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

    // ゲーム終了処理を一本化し、成功/失敗を引数で渡す
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

        Debug.Log($"Game Ended. Current Time.timeScale: {Time.timeScale}"); // この行で値を確認できます

        // TODO: ここでリザルト画面などを表示する処理を実装
    }

    // ゲームをリセットするための公開メソッド（リトライボタンなどで使用）
    public void RestartGame()
    {
        Time.timeScale = 1f; // 時間を元に戻す
        // ★ここを修正しました！
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // シーンをリロードしてゲームをリセット
        // DontDestroyOnLoadを使っているため、GameManager自体はシーンロード後も残りますが、
        // AwakeでInitializeGameが再度呼ばれるため問題ありません。
    }
}