using UnityEngine;
using System.Collections;
using TMPro;

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
    [SerializeField] private SpeechPhraseData[] tauntPhrases;
    [SerializeField] private float minDisplayTime = 2f;
    [SerializeField] private float maxDisplayTime = 5f;
    [SerializeField] private float displayInterval = 7f;

    [SerializeField] private Transform speechBubbleCanvasTransform;

    private GameObject currentSpeechBubble;
    private AudioSource enemyAudioSource;
    private Coroutine speechBubbleCoroutine; // 実行中のコルーチンを保持するための変数

    void Awake()
    {
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

        enemyAudioSource = GetComponent<AudioSource>();
        if (enemyAudioSource == null)
        {
            Debug.LogWarning("AudioSource component not found on Enemy object. Enemy voices will not play.", this);
        }
    }

    void Start()
    {
        // 開始したコルーチンを保持しておく
        speechBubbleCoroutine = StartCoroutine(SpeechBubbleRoutine());
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

    // ▼▼▼▼▼ エラーの原因箇所。正しい内容に復元しました ▼▼▼▼▼
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

                SpeechPhraseData selectedPhraseData = tauntPhrases[Random.Range(0, tauntPhrases.Length)];

                currentSpeechBubble = Instantiate(speechBubblePrefab, speechBubbleSpawnPoint.position, Quaternion.identity, speechBubbleCanvasTransform);
                currentSpeechBubble.transform.position = speechBubbleSpawnPoint.position;

                if (Camera.main != null)
                {
                    currentSpeechBubble.transform.LookAt(Camera.main.transform.position);
                    currentSpeechBubble.transform.forward = -currentSpeechBubble.transform.forward;
                }

                TMP_Text textComponent = currentSpeechBubble.GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = selectedPhraseData.phrase;
                }
                else
                {
                    Debug.LogWarning("TextMeshProUGUI component not found in Speech Bubble Prefab's children of " + speechBubblePrefab.name);
                }

                if (enemyAudioSource != null && selectedPhraseData.voiceClip != null)
                {
                    enemyAudioSource.PlayOneShot(selectedPhraseData.voiceClip);
                }
                else if (selectedPhraseData.voiceClip == null)
                {
                    Debug.LogWarning("No Voice Clip assigned for phrase: " + selectedPhraseData.phrase + " on Enemy Stress Visuals.", this);
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
    // ▲▲▲▲▲ ここまで ▲▲▲▲▲

    /// <summary>
    /// 全てのストレス演出（吹き出し、ボイス）を停止させる公開メソッド
    /// </summary>
    public void StopAllVisuals()
    {
        // 実行中のコルーチンがあれば停止させる
        if (speechBubbleCoroutine != null)
        {
            StopCoroutine(speechBubbleCoroutine);
            speechBubbleCoroutine = null; // 保持していたコルーチンをクリア
        }

        // 表示中の吹き出しがあれば破棄する
        if (currentSpeechBubble != null)
        {
            Destroy(currentSpeechBubble);
        }

        // ボイスが再生中であれば停止させる
        if (enemyAudioSource != null && enemyAudioSource.isPlaying)
        {
            enemyAudioSource.Stop();
        }

        // このコンポーネント自体を無効にして、以降のUpdate呼び出しなどを停止する
        this.enabled = false;
        Debug.Log("EnemyStressVisuals stopped.");
    }
}
