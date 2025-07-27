using UnityEngine;
using System.Collections;
using TMPro;

public class EnemyStressVisuals : MonoBehaviour
{
    // （変更なし）
    [Header("Look At Player Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    private Transform playerTransform;

    [Header("Idle Animation Settings")]
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private string idleAnimationTrigger = "Idle";

    [Header("Speech Bubble Settings")]
    [SerializeField] private GameObject speechBubblePrefab;
    [SerializeField] private Transform speechBubbleSpawnPoint;
    [SerializeField] private string[] tauntPhrases;
    [SerializeField] private float minDisplayTime = 2f;
    [SerializeField] private float maxDisplayTime = 5f;
    [SerializeField] private float displayInterval = 7f;

    // ★修正点1: CanvasのTransformをInspectorから設定できるようにする
    // 今回はWorld Space Canvasを推奨するため、このフィールドで設定します
    [SerializeField] private Transform speechBubbleCanvasTransform; // 吹き出し専用のWorld Space CanvasのTransform

    private GameObject currentSpeechBubble;
    private bool isDisplayingSpeechBubble = false;

    void Awake()
    {
        // （変更なし）
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

        // ★修正点2: speechBubbleCanvasTransformがInspectorから設定されているか確認
        if (speechBubbleCanvasTransform == null)
        {
            Debug.LogError("Speech Bubble Canvas Transform is not assigned! Please drag and drop the 'SpeechBubbleCanvas' to this field.", this);
        }
    }

    void Start()
    {
        StartCoroutine(SpeechBubbleRoutine());
    }

    void Update()
    {
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
        while (true)
        {
            yield return new WaitForSeconds(displayInterval);

            if (tauntPhrases != null && tauntPhrases.Length > 0 && speechBubblePrefab != null && speechBubbleSpawnPoint != null && speechBubbleCanvasTransform != null)
            {
                if (currentSpeechBubble != null)
                {
                    Destroy(currentSpeechBubble);
                }

                string phrase = tauntPhrases[Random.Range(0, tauntPhrases.Length)];

                // ★修正点3: 吹き出しをWorld Space Canvasの子として生成し、SpawnPointの位置に配置
                currentSpeechBubble = Instantiate(speechBubblePrefab, speechBubbleSpawnPoint.position, Quaternion.identity, speechBubbleCanvasTransform);

                // World Space Canvasなので、Transform.positionを直接設定できる
                currentSpeechBubble.transform.position = speechBubbleSpawnPoint.position;
                // 必要に応じてオフセットを追加する
                // currentSpeechBubble.transform.position += new Vector3(0, 0.5f, 0); // 例: Y軸に0.5m上にずらす

                // ★修正点4: World Space Canvasでは、常にカメラの方を向かせる処理は有効
                // ただし、RectTransformのZ軸がカメラに正対するように調整する必要があります。
                // 吹き出しの裏表を考慮し、カメラの逆方向を向くように設定します。
                if (Camera.main != null)
                {
                    currentSpeechBubble.transform.LookAt(Camera.main.transform.position);
                    currentSpeechBubble.transform.forward = -currentSpeechBubble.transform.forward; // 通常のUIはカメラから見て手前が表なので、逆を向かせる
                }


                TMP_Text textComponent = currentSpeechBubble.GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = phrase;
                }
                else
                {
                    Debug.LogWarning("TextMeshProUGUI component not found in Speech Bubble Prefab's children of " + speechBubblePrefab.name);
                }

                float displayTime = Random.Range(minDisplayTime, maxDisplayTime);
                yield return new WaitForSeconds(displayTime);

                if (currentSpeechBubble != null)
                {
                    Destroy(currentSpeechBubble);
                    currentSpeechBubble = null;
                }
            }
        }
    }
}