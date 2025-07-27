using UnityEngine;
using System.Collections; // Coroutineのために必要
using TMPro;

public class EnemyStressVisuals : MonoBehaviour
{
    [Header("Look At Player Settings")]
    [SerializeField] private float rotationSpeed = 5f; // プレイヤーの方向を向く速さ
    private Transform playerTransform;

    [Header("Idle Animation Settings")]
    [SerializeField] private Animator enemyAnimator; // 敵のAnimatorコンポーネント
    [SerializeField] private string idleAnimationTrigger = "Idle"; // 任意のアニメーション再生用トリガー名 (例: AnimatorにIdle_LoopというStateを作り、Trigger: Idleで遷移するように設定)

    [Header("Speech Bubble Settings")]
    [SerializeField] private GameObject speechBubblePrefab; // 吹き出しUIのプレハブ
    [SerializeField] private Transform speechBubbleSpawnPoint; // 吹き出しを生成する位置 (敵の頭上など)
    [SerializeField] private string[] tauntPhrases; // 煽りセリフの配列
    [SerializeField] private float minDisplayTime = 2f; // 吹き出しの最小表示時間
    [SerializeField] private float maxDisplayTime = 5f; // 吹き出しの最大表示時間
    [SerializeField] private float displayInterval = 7f; // 吹き出しが表示される間隔（次の吹き出しが表示されるまでの待機時間）

    private GameObject currentSpeechBubble;
    private bool isDisplayingSpeechBubble = false;

    void Awake()
    {
        // Playerオブジェクトをタグで検索して取得
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("Player object with tag 'Player' not found in scene. Enemy will not look at player.");
        }

        // Animatorが設定されていなければ、このゲームオブジェクトから取得を試みる
        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponent<Animator>();
        }

        // 初期アイドルアニメーションを再生
        if (enemyAnimator != null && !string.IsNullOrEmpty(idleAnimationTrigger))
        {
            enemyAnimator.SetTrigger(idleAnimationTrigger);
        }
    }

    void Start()
    {
        // 吹き出し表示コルーチンを開始
        StartCoroutine(SpeechBubbleRoutine());
    }

    void Update()
    {
        // プレイヤーが存在し、かつRagdoll状態でない場合にのみプレイヤーの方向を向く
        // Animator.enabledがtrue (アニメーション再生中) の時のみ実行
        if (playerTransform != null && enemyAnimator != null && enemyAnimator.enabled)
        {
            LookAtPlayer();
        }
    }

    private void LookAtPlayer()
    {
        Vector3 targetPosition = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private IEnumerator SpeechBubbleRoutine()
    {
        while (true) // ゲーム中ずっと繰り返す
        {
            yield return new WaitForSeconds(displayInterval); // 次の吹き出しが表示されるまでの待機

            if (tauntPhrases != null && tauntPhrases.Length > 0 && speechBubblePrefab != null && speechBubbleSpawnPoint != null)
            {
                // 既に吹き出しが表示中であれば、一度破棄する
                if (currentSpeechBubble != null)
                {
                    Destroy(currentSpeechBubble);
                }

                // ランダムなセリフを選択
                string phrase = tauntPhrases[Random.Range(0, tauntPhrases.Length)];

                // 吹き出しを生成
                currentSpeechBubble = Instantiate(speechBubblePrefab, speechBubbleSpawnPoint.position, Quaternion.identity, speechBubbleSpawnPoint);
                currentSpeechBubble.transform.forward = Camera.main.transform.forward; // カメラの方を向かせる

                // TextMeshProUGUIコンポーネントを探してテキストを設定
                TMP_Text textComponent = currentSpeechBubble.GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = phrase;
                }
                else
                {
                    Debug.LogWarning("TextMeshProUGUI component not found in Speech Bubble Prefab's children.");
                }

                float displayTime = Random.Range(minDisplayTime, maxDisplayTime);
                yield return new WaitForSeconds(displayTime); // 表示時間待機

                // 吹き出しを非表示にする（破棄）
                if (currentSpeechBubble != null)
                {
                    Destroy(currentSpeechBubble);
                    currentSpeechBubble = null;
                }
            }
        }
    }
}