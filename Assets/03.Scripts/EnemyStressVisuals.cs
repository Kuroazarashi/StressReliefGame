using UnityEngine;
using System.Collections;
using TMPro; // TextMeshProを使用しているため

public class EnemyStressVisuals : MonoBehaviour
{
    [Header("Look At Player Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    private Transform playerTransform;

    [Header("Idle Animation Settings")]
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private string idleAnimationTrigger = "Idle";

    [Header("Speech Bubble Settings")]
    [SerializeField] private GameObject speechBubblePrefab;
    [SerializeField] private Transform speechBubbleSpawnPoint;
    // ★修正点1: string[] から SpeechPhraseData[] に変更
    //           煽りセリフと対応するボイスのペアを管理するための配列になります。
    [SerializeField] private SpeechPhraseData[] tauntPhrases;
    [SerializeField] private float minDisplayTime = 2f;
    [SerializeField] private float maxDisplayTime = 5f;
    [SerializeField] private float displayInterval = 7f;

    [SerializeField] private Transform speechBubbleCanvasTransform;

    private GameObject currentSpeechBubble;
    private bool isDisplayingSpeechBubble = false;

    // ★追加点2: AudioSourceコンポーネントへの参照を保持する変数
    private AudioSource enemyAudioSource;

    void Awake()
    {
        // （変更なし）Playerオブジェクトの検索とAnimatorの取得
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("Player object with tag 'Player' not found in scene. Enemy will not look at player.");
        }

        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponent<Animator>();
        }

        if (enemyAnimator != null && !string.IsNullOrEmpty(idleAnimationTrigger))
        {
            enemyAnimator.SetTrigger(idleAnimationTrigger);
        }

        if (speechBubbleCanvasTransform == null)
        {
            Debug.LogError("Speech Bubble Canvas Transform is not assigned in the Inspector! Please drag and drop the 'SpeechBubbleCanvas' to this field.", this);
        }

        // ★追加点3: 自身のゲームオブジェクトからAudioSourceコンポーネントを取得
        enemyAudioSource = GetComponent<AudioSource>();
        if (enemyAudioSource == null)
        {
            Debug.LogWarning("AudioSource component not found on Enemy object. Enemy voices will not play.", this);
            // AudioSourceがない場合は、エラーを出して処理を中断するのではなく、
            // 単にボイス再生部分をスキップするようにします。
        }
    }

    void Start()
    {
        StartCoroutine(SpeechBubbleRoutine());
    }

    void Update()
    {
        // プレイヤーが存在し、かつAnimatorが有効な場合にのみプレイヤーの方向を向く
        if (playerTransform != null && enemyAnimator != null && enemyAnimator.enabled)
        {
            LookAtPlayer();
        }
    }

    private void LookAtPlayer()
    {
        // プレイヤーのY座標を無視して、敵のY座標と同じ高さで向き合う
        Vector3 targetPosition = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
        // 現在位置からターゲット位置への方向ベクトルを取得し、それに向くクォータニオンを計算
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
        // 現在の回転から目標の回転へ滑らかに補間
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private IEnumerator SpeechBubbleRoutine()
    {
        while (true) // ゲーム中、このコルーチンを無限ループで実行
        {
            yield return new WaitForSeconds(displayInterval); // 次の吹き出しが表示されるまでの待機時間

            // 必要な設定がInspectorで全て行われているかを確認
            if (tauntPhrases != null && tauntPhrases.Length > 0 && speechBubblePrefab != null && speechBubbleSpawnPoint != null && speechBubbleCanvasTransform != null)
            {
                // 既に吹き出しが表示中であれば、一度破棄する
                if (currentSpeechBubble != null)
                {
                    Destroy(currentSpeechBubble);
                }

                // ★修正点4: tauntPhrases配列からランダムに1つの「SpeechPhraseData」を選択
                SpeechPhraseData selectedPhraseData = tauntPhrases[Random.Range(0, tauntPhrases.Length)];

                // 吹き出しプレハブをインスタンス化し、SpeechBubbleCanvasの子として配置
                currentSpeechBubble = Instantiate(speechBubblePrefab, speechBubbleSpawnPoint.position, Quaternion.identity, speechBubbleCanvasTransform);

                // ワールド空間Canvasなので、Transform.positionを直接設定して3D空間のSpawnPointに合わせる
                currentSpeechBubble.transform.position = speechBubbleSpawnPoint.position;
                // 必要に応じてY座標を微調整するためのオフセット（コメントアウトされていますが、必要な場合に）
                // currentSpeechBubble.transform.position += new Vector3(0, 0.5f, 0); 

                // ワールド空間UIが常にカメラの方を向くように設定
                if (Camera.main != null)
                {
                    currentSpeechBubble.transform.LookAt(Camera.main.transform.position);
                    // UIは通常、カメラから見て手前が表になるように作られているため、
                    // LookAtでカメラを向くと裏返る場合がある。その場合、フォワードベクトルを反転させる。
                    currentSpeechBubble.transform.forward = -currentSpeechBubble.transform.forward;
                }

                // 吹き出し内のTextMeshProUGUIコンポーネントを取得し、テキストを設定
                TMP_Text textComponent = currentSpeechBubble.GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = selectedPhraseData.phrase; // ★修正点5: selectedPhraseDataのphraseプロパティを使用
                }
                else
                {
                    Debug.LogWarning("TextMeshProUGUI component not found in Speech Bubble Prefab's children of " + speechBubblePrefab.name);
                }

                // ★追加点6: ボイスの再生
                // enemyAudioSourceが有効で、かつ選択されたセリフデータにボイスクリップが割り当てられていれば再生
                if (enemyAudioSource != null && selectedPhraseData.voiceClip != null)
                {
                    enemyAudioSource.PlayOneShot(selectedPhraseData.voiceClip);
                }
                else if (selectedPhraseData.voiceClip == null)
                {
                    Debug.LogWarning("No Voice Clip assigned for phrase: " + selectedPhraseData.phrase + " on Enemy Stress Visuals.", this);
                }

                // 吹き出しのランダムな表示時間待機
                float displayTime = Random.Range(minDisplayTime, maxDisplayTime);
                yield return new WaitForSeconds(displayTime);

                // 吹き出しを破棄して非表示にする
                if (currentSpeechBubble != null)
                {
                    Destroy(currentSpeechBubble);
                    currentSpeechBubble = null;
                }
            }
        }
    }

    // ★追加点7: 煽りセリフとボイスのペアをInspectorで設定するための新しいクラス
    //   Unity EditorでこのクラスのプロパティをInspectorに表示させるために [System.Serializable] 属性を付けます。
    [System.Serializable]
    public class SpeechPhraseData
    {
        public string phrase;       // 吹き出しに表示するセリフのテキスト
        public AudioClip voiceClip; // そのセリフに対応するボイスのAudioClip（音声ファイル）
    }
}