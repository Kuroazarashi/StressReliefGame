using System.Collections;
using System.Collections.Generic; // 今回は不要だが念のため
using UnityEngine;
using UnityEngine.UI; // UI.Imageを使用するため追加
using TMPro; // TextMeshProを使用するため追加 // ★追加: TextMeshProを使うために必要★

// SpeechPhraseDataクラスが独立したファイルに存在する場合、
// そのファイルが同じネームスペースにあるか、またはusingディレクティブで参照できるようにしておく必要があります。
// 何もネームスペースを指定していなければ、通常はusingは不要です。

public class PlayerController : MonoBehaviour
{
    // === Animator Settings ===
    [Header("Animator Settings")]
    [SerializeField] private Animator animator; // アニメーターへの参照

    // === Attack Settings ===
    [Header("Attack Settings")]
    [SerializeField] private float punchForce = 1000f; // パンチの吹っ飛ばし力
    [SerializeField] private float kickForce = 800f; // キックの吹っ飛ばし力
    [SerializeField] private float upwardForceMultiplier = 0.5f; // 上方向への力の倍率

    [Header("References (Attack)")]
    [SerializeField] private GameObject punchColliderObject; // パンチ用コライダーのゲームオブジェクト
    [SerializeField] private GameObject kickColliderObject;  // キック用コライダーのゲームオブジェクト

    // === Slow Motion Settings ===
    [Header("Slow Motion Settings")]
    [SerializeField] private float slowMotionDuration = 0.2f; // スローモーションの持続時間
    [SerializeField] private float slowMotionTimeScale = 0.1f; // スローモーション時のタイムスケール

    // スローモーション時の物理演算の滑らかさ調整用
    private float originalFixedDeltaTime; // 元のFixed Delta Timeを保存する変数
    private Coroutine slowMotionCoroutine; // スローモーションのコルーチン管理用

    // === Movement Settings ===
    [Header("Movement Settings")] // 移動設定用のヘッダー
    [SerializeField] private Joystick joystick; // UIのJoystickスクリプトへの参照
    [SerializeField] private float moveSpeed = 5f; // 移動速度
    [SerializeField] private float rotateSpeed = 500f; // 回転速度

    // === Effects & Sounds Settings ===
    [Header("Effects & Sounds Settings")]
    [SerializeField] private GameObject hitEffectPrefab; // ヒット時に再生するパーティクルエフェクトのプレハブ
    [SerializeField] private AudioClip swingSoundClip; // 空振り時に再生するサウンドクリップ
    [SerializeField] private Image concentrationEffectImage; // 集中線エフェクトのUI Image
    [SerializeField] private float concentrationEffectDuration = 0.1f; // 集中線エフェクトの表示時間

    // ★ここから Player Speech Bubble Settings (変更なし、speechBubbleCanvasTransformは残す) ★
    [Header("Player Speech Bubble Settings")]
    [SerializeField] private GameObject playerSpeechBubblePrefab; // プレイヤー用吹き出しプレハブ (Enemyと同じものを流用可)
    [SerializeField] private Transform playerSpeechBubbleSpawnPoint; // プレイヤー用吹き出し生成位置（プレイヤーの頭上などに空のGameObjectを配置）
    [SerializeField] private SpeechPhraseData[] playerPhrases; // プレイヤーのセリフとボイスの配列
    [SerializeField] private float playerMinDisplayTime = 1.5f; // プレイヤー吹き出しの最小表示時間
    [SerializeField] private float playerMaxDisplayTime = 3f;   // プレイヤー吹き出しの最大表示時間
    [SerializeField] private float playerDisplayInterval = 10f; // プレイヤー吹き出しの表示間隔

    // このフィールドはInspectorから設定されるもので、Awake()で参照されるためコメントアウトしない
    [SerializeField] private Transform speechBubbleCanvasTransform; // SpeechBubbleCanvas (World Space Canvas) への参照
    // ★ここまで Player Speech Bubble Settings ★

    // === Private References ===
    private CharacterController characterController; // CharacterControllerへの参照
    private bool isAttacking = false; // 攻撃中かどうかのフラグ
    private bool hasHitTarget = false; // 今回の攻撃で何かにヒットしたかどうかのフラグ
    private AudioSource playerAudioSource; // 空振りSE再生用のAudioSourceを専用変数に (今回はプレイヤーボイス再生も兼ねる)

    // ★追加点: プレイヤーの吹き出し表示管理用
    private GameObject currentPlayerSpeechBubble;


    // AwakeはStartより前に呼ばれる
    void Awake()
    {
        // CharacterControllerとAnimatorコンポーネントを取得
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // コンポーネントがアタッチされているか確認（エラーログ用）
        if (characterController == null)
        {
            Debug.LogError("CharacterController not found on " + gameObject.name + ". Please add one.", this);
            enabled = false;
            return;
        }
        if (animator == null)
        {
            Debug.LogError("Animator not found on " + gameObject.name + ". Please assign in Inspector or add one.", this);
            enabled = false;
            return;
        }

        // Joystickが設定されていない場合の警告
        if (joystick == null)
        {
            Debug.LogWarning("Joystick is not assigned in Inspector. Movement will not work.", this);
        }

        // AudioSourceを専用変数に格納 (空振りSEとプレイヤーボイスの両方で使用)
        playerAudioSource = GetComponent<AudioSource>();
        if (playerAudioSource == null)
        {
            playerAudioSource = gameObject.AddComponent<AudioSource>();
        }
        playerAudioSource.loop = false; // ループ再生はしない
        playerAudioSource.playOnAwake = false; // 自動再生しない

        // SpeechBubbleCanvasが設定されているか確認 (プレイヤーの吹き出し用)
        // 今回の修正で吹き出しの親としては使用しませんが、Inspectorでの設定確認は残しておきます。
        if (speechBubbleCanvasTransform == null)
        {
            Debug.LogWarning("Speech Bubble Canvas Transform is not assigned for Player! Although no longer directly parenting player speech bubbles, this field might be expected for other UI configurations or for other scripts. Please ensure 'SpeechBubbleCanvas' is assigned if needed elsewhere.", this);
        }

        // --- AttackColliderHandlerの設定 (変更なし) ---
        SetupAttackColliderHandler(punchColliderObject, punchForce, "Punch");
        SetupAttackColliderHandler(kickColliderObject, kickForce, "Kick");
        // --- AttackColliderHandlerの設定 (変更なし) ---

        // 初期状態で攻撃コライダーを無効にしておく
        if (punchColliderObject != null)
        {
            punchColliderObject.SetActive(false);
        }
        if (kickColliderObject != null)
        {
            kickColliderObject.SetActive(false);
        }

        // 集中線エフェクトの初期状態を非アクティブにする
        if (concentrationEffectImage != null)
        {
            concentrationEffectImage.gameObject.SetActive(false);
        }

        // オリジナルのFixed Delta Timeを保存
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    void Start()
    {
        // ★ここから追加する処理 (PlayerSpeechBubbleRoutineの開始) ★
        // プレイヤーの吹き出しコルーチンを開始
        StartCoroutine(PlayerSpeechBubbleRoutine());
        // ★ここまで追加する処理★
    }

    void Update()
    {
        if (!isAttacking)
        {
            HandleMovement();
        }
        else
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }
            characterController.Move(Vector3.zero);
        }

        // 地面にいるかどうかのチェック (CharacterControllerはPhysics.gravityを自動で適用しないため)
        if (!characterController.isGrounded)
        {
            characterController.Move(Vector3.up * Physics.gravity.y * Time.deltaTime);
        }
    }

    // === 移動処理メソッド ===
    private void HandleMovement()
    {
        // ジョイスティックの入力方向を取得
        Vector2 inputDir = joystick.InputDirection;

        Vector3 horizontalMoveDirection = new Vector3(inputDir.x, 0f, inputDir.y);

        float currentSpeed = horizontalMoveDirection.magnitude;
        if (animator != null)
        {
            animator.SetFloat("Speed", currentSpeed);
        }

        if (currentSpeed >= 0.1f)
        {
            characterController.Move(horizontalMoveDirection.normalized * moveSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(horizontalMoveDirection.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }
        else
        {
            characterController.Move(Vector3.zero);
        }
    }

    // === パンチ関連メソッド ===
    public void OnPunchButtonClick()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            hasHitTarget = false; // 攻撃開始時にヒットフラグをリセット
            Debug.Log("Punch Button Clicked! Playing Punch Animation.");

            if (animator != null)
            {
                animator.SetTrigger("PunchTrigger");
            }
        }
    }

    public void EnablePunchCollider()
    {
        if (punchColliderObject != null)
        {
            punchColliderObject.SetActive(true);
            Debug.Log("Punch Collider Enabled!");
        }
    }

    public void DisablePunchCollider()
    {
        if (punchColliderObject != null)
        {
            punchColliderObject.SetActive(false);
            Debug.Log("Punch Collider Disabled!");
        }
    }

    // === キック関連メソッド ===
    public void OnKickButtonClick()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            hasHitTarget = false; // 攻撃開始時にヒットフラグをリセット
            Debug.Log("Kick Button Clicked! Playing Kick Animation.");

            if (animator != null)
            {
                animator.SetTrigger("KickTrigger");
            }
        }
    }

    public void EnableKickCollider()
    {
        if (kickColliderObject != null)
        {
            kickColliderObject.SetActive(true);
            Debug.Log("Kick Collider Enabled!");
        }
    }

    public void DisableKickCollider()
    {
        if (kickColliderObject != null)
        {
            kickColliderObject.SetActive(false);
            Debug.Log("Kick Collider Disabled!");
        }
    }

    public void ResetAttackState()
    {
        // 攻撃がヒットしなかった場合にのみ空振りSEを再生
        if (!hasHitTarget && swingSoundClip != null && playerAudioSource != null)
        {
            playerAudioSource.PlayOneShot(swingSoundClip);
            Debug.Log("Swing sound played (miss)!");
        }

        isAttacking = false;
        hasHitTarget = false; // フラグをリセット
        if (punchColliderObject != null) punchColliderObject.SetActive(false);
        if (kickColliderObject != null) kickColliderObject.SetActive(false);
    }

    // === 攻撃がヒットした際の処理 ===
    public void OnAttackHit(Collider other, float force)
    {
        Rigidbody hitRigidbody = other.GetComponent<Rigidbody>();
        if (hitRigidbody != null)
        {
            hasHitTarget = true; // ヒットしたことを記録

            if (hitRigidbody.isKinematic)
            {
                hitRigidbody.isKinematic = false;
            }

            Vector3 attackDirection = (other.transform.position - transform.position).normalized;
            Vector3 totalForce = (attackDirection + Vector3.up * upwardForceMultiplier).normalized * force;

            hitRigidbody.AddForce(totalForce, ForceMode.Impulse);

            // ヒットエフェクトの再生
            if (hitEffectPrefab != null)
            {
                GameObject effectInstance = Instantiate(hitEffectPrefab, other.transform.position, Quaternion.identity);
                ParticleSystem ps = effectInstance.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    Destroy(effectInstance, ps.main.duration + ps.main.startLifetime.constantMax);
                }
                else
                {
                    Destroy(effectInstance, 2f); // ParticleSystemがない場合の安全策
                }
            }

            // SoundManager経由でヒットサウンドを再生
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayHitSound(other.gameObject);
            }
            else
            {
                Debug.LogWarning("SoundManager.Instance is not found. Cannot play hit sound.", this);
            }

            // スローモーション演出を呼び出す
            StartSlowMotion();

            // 集中線エフェクトの表示
            if (concentrationEffectImage != null)
            {
                StartCoroutine(ShowConcentrationEffect());
            }

            Debug.Log($"Hit {other.name} with force {force}!");
        }
    }

    // === スローモーション演出 ===
    private void StartSlowMotion()
    {
        if (slowMotionCoroutine != null)
        {
            StopCoroutine(slowMotionCoroutine);
        }
        slowMotionCoroutine = StartCoroutine(DoSlowMotion());
    }

    private IEnumerator DoSlowMotion()
    {
        Time.timeScale = slowMotionTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;

        yield return new WaitForSecondsRealtime(slowMotionDuration);

        EndSlowMotion();
    }

    private void EndSlowMotion()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
    }

    // 集中線エフェクト表示用のコルーチン
    private IEnumerator ShowConcentrationEffect()
    {
        if (concentrationEffectImage != null)
        {
            concentrationEffectImage.gameObject.SetActive(true); // 画像をアクティブにする
            yield return new WaitForSeconds(concentrationEffectDuration); // 指定時間待つ
            concentrationEffectImage.gameObject.SetActive(false); // 画像を非アクティブにする
        }
    }

    // ★ここから修正するコルーチン (PlayerSpeechBubbleRoutine) ★
    // プレイヤーの吹き出しとボイスを管理するコルーチン
    private IEnumerator PlayerSpeechBubbleRoutine()
    {
        while (true)
        {
            // まずは設定された間隔だけ待機
            yield return new WaitForSeconds(playerDisplayInterval);

            // 必要な設定がInspectorで全て行われているかを確認
            // speechBubbleCanvasTransform のチェックは引き続き行いますが、
            // 吹き出しの親としては使用しないため、Debug.LogWarning のメッセージを調整しています。
            if (playerPhrases != null && playerPhrases.Length > 0 && playerSpeechBubblePrefab != null && playerSpeechBubbleSpawnPoint != null)
            {
                // 既に吹き出しが表示中であれば、一度破棄する
                if (currentPlayerSpeechBubble != null)
                {
                    Destroy(currentPlayerSpeechBubble);
                }

                // ランダムなSpeechPhraseDataを選択
                SpeechPhraseData selectedPhraseData = playerPhrases[Random.Range(0, playerPhrases.Length)];

                // 【ここを修正】: 吹き出しをplayerSpeechBubbleSpawnPointの子として生成
                // Instantiateのオーバーロードを使い、第2引数にTransformを指定することで親を設定します
                currentPlayerSpeechBubble = Instantiate(playerSpeechBubblePrefab, playerSpeechBubbleSpawnPoint);

                // 親のTransformに対してローカル座標、回転、スケールをリセットし、吹き出しが正確な位置に表示されるようにします。
                currentPlayerSpeechBubble.transform.localPosition = Vector3.zero;
                currentPlayerSpeechBubble.transform.localRotation = Quaternion.identity;
                currentPlayerSpeechBubble.transform.localScale = Vector3.one; // プレハブの元のスケールを維持したい場合

                // 吹き出しの裏表を考慮し、カメラの逆方向を向かせる
                // 親がplayerSpeechBubbleSpawnPointになったので、ワールド座標でのLookAtはそのままでOK
                if (Camera.main != null)
                {
                    currentPlayerSpeechBubble.transform.LookAt(Camera.main.transform.position);
                    currentPlayerSpeechBubble.transform.forward = -currentPlayerSpeechBubble.transform.forward;
                }

                // 吹き出し内のTextMeshProUGUIコンポーネントを取得し、テキストを設定
                TMP_Text textComponent = currentPlayerSpeechBubble.GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = selectedPhraseData.phrase;
                }
                else
                {
                    Debug.LogWarning("TextMeshProUGUI component not found in Player Speech Bubble Prefab's children of " + playerSpeechBubblePrefab.name);
                }

                // ボイスの再生
                if (playerAudioSource != null && selectedPhraseData.voiceClip != null)
                {
                    playerAudioSource.PlayOneShot(selectedPhraseData.voiceClip);
                }
                else if (selectedPhraseData.voiceClip == null)
                {
                    Debug.LogWarning("No Voice Clip assigned for player phrase: " + selectedPhraseData.phrase + " on PlayerController.", this);
                }

                // 吹き出しのランダムな表示時間待機
                float displayTime = Random.Range(playerMinDisplayTime, playerMaxDisplayTime);
                yield return new WaitForSeconds(displayTime);

                // 吹き出しを破棄して非表示にする
                if (currentPlayerSpeechBubble != null)
                {
                    Destroy(currentPlayerSpeechBubble);
                    currentPlayerSpeechBubble = null;
                }
            }
            else
            {
                // 設定が不完全な場合でも無限ループが停止しないように、短い時間待機
                yield return new WaitForSeconds(1f);
            }
        }
    }
    // ★ここまで修正するコルーチン★

    /// <summary>
    /// 指定されたコライダーオブジェクトのAttackColliderHandlerを初期設定します。
    /// </summary>
    /// <param name="colliderObject">AttackColliderHandlerがアタッチされているゲームオブジェクト。</param>
    /// <param name="force">この攻撃で適用される力。</param>
    /// <param name="colliderName">ログ表示用のコライダーの名前（例: "Punch", "Kick"）。</param>
    private void SetupAttackColliderHandler(GameObject colliderObject, float force, string colliderName)
    {
        if (colliderObject != null)
        {
            AttackColliderHandler handler = colliderObject.GetComponent<AttackColliderHandler>();
            if (handler != null)
            {
                handler.SetPlayerController(this); // PlayerController自身をセット
                handler.AttackForce = force;        // 攻撃力をセット
                Debug.Log($"{colliderName} Collider Handler setup complete for {colliderObject.name}.");
            }
            else
            {
                // AttackColliderHandlerがコライダーオブジェクトに見つからない場合の警告
                Debug.LogWarning($"AttackColliderHandler not found on {colliderName} Collider Object: {colliderObject.name}. Please add it.", colliderObject);
            }
        }
        else
        {
            // コライダーオブジェクトがInspectorにアサインされていない場合の警告
            Debug.LogWarning($"{colliderName} Collider Object is not assigned in PlayerController's Inspector. Please assign it.", this);
        }
    }
}