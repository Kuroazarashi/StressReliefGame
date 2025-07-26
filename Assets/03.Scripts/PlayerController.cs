using System.Collections;
using System.Collections.Generic; // HashSetを使用するために追加 (今回は不要だが念のため)
using UnityEngine;
using UnityEngine.UI; // UI.Imageを使用するため追加

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

    // === Private References ===
    private CharacterController characterController; // CharacterControllerへの参照
    private bool isAttacking = false; // 攻撃中かどうかのフラグ
    private bool hasHitTarget = false; // 今回の攻撃で何かにヒットしたかどうかのフラグ
    private AudioSource playerAudioSource; // 空振りSE再生用のAudioSourceを専用変数に

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

        // 空振りSE再生用のAudioSourceを専用変数に格納
        playerAudioSource = GetComponent<AudioSource>();
        if (playerAudioSource == null)
        {
            playerAudioSource = gameObject.AddComponent<AudioSource>();
        }
        playerAudioSource.loop = false; // ループ再生はしない
        playerAudioSource.playOnAwake = false; // 自動再生しない

        // --- ★ここから修正・確認箇所★ ---
        // PlayerControllerが自身にアタッチされているパンチ・キックコライダーに
        // 自身のインスタンスと攻撃力を渡す

        SetupAttackColliderHandler(punchColliderObject, punchForce, "Punch");
        SetupAttackColliderHandler(kickColliderObject, kickForce, "Kick");
        // --- ★ここまで修正・確認箇所★ ---

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

    // --- PlayerControllerクラスのどこかに以下のヘルパーメソッドを追加してください（Awake()の後や末尾など） ---
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
                handler.AttackForce = force;       // 攻撃力をセット
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