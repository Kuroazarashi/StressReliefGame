using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    // private AudioClip hitSoundClip;      // ★削除: SoundManagerで一元管理するため削除
    // private AudioSource audioSource;     // ★削除: SoundManagerで一元管理するため削除

    // === Private References ===
    private CharacterController characterController; // CharacterControllerへの参照
    private bool isAttacking = false; // 攻撃中かどうかのフラグを追加

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

        // PlayerControllerが自身にアタッチされているパンチ・キックコライダーに
        // 自身のインスタンスと攻撃力を渡す
        if (punchColliderObject != null)
        {
            AttackColliderHandler punchHandler = punchColliderObject.GetComponent<AttackColliderHandler>();
            if (punchHandler != null)
            {
                punchHandler.SetPlayerController(this);
                punchHandler.AttackForce = punchForce;
            }
            else
            {
                Debug.LogWarning("AttackColliderHandler not found on Punch Collider Object.", punchColliderObject);
            }
        }
        else
        {
            Debug.LogWarning("Punch Collider Object is not assigned in Inspector.", this);
        }

        if (kickColliderObject != null)
        {
            AttackColliderHandler kickHandler = kickColliderObject.GetComponent<AttackColliderHandler>();
            if (kickHandler != null)
            {
                kickHandler.SetPlayerController(this);
                kickHandler.AttackForce = kickForce;
            }
            else
            {
                Debug.LogWarning("AttackColliderHandler not found on Kick Collider Object.", kickColliderObject);
            }
        }
        else
        {
            Debug.LogWarning("Kick Collider Object is not assigned in Inspector.", this);
        }

        // 初期状態で攻撃コライダーを無効にしておく
        if (punchColliderObject != null)
        {
            punchColliderObject.SetActive(false);
        }
        if (kickColliderObject != null)
        {
            kickColliderObject.SetActive(false);
        }

        // ★削除: AudioSourceの取得・初期化はSoundManagerが担当するため不要
        // audioSource = GetComponent<AudioSource>();
        // if (audioSource == null)
        // {
        //     audioSource = gameObject.AddComponent<AudioSource>();
        // }
        // audioSource.loop = false;
        // audioSource.playOnAwake = false;

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
            Debug.Log("Punch Button Clicked! Playing Punch Animation.");
            if (animator != null)
            {
                animator.SetTrigger("PunchTrigger"); // ★修正: "Punch" から "PunchTrigger" へ
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
            Debug.Log("Kick Button Clicked! Playing Kick Animation.");
            if (animator != null)
            {
                animator.SetTrigger("KickTrigger"); // ★修正: "Kick" から "KickTrigger" へ
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
        isAttacking = false;
        if (punchColliderObject != null) punchColliderObject.SetActive(false);
        if (kickColliderObject != null) kickColliderObject.SetActive(false);
    }

    // === 攻撃がヒットした際の処理 ===
    public void OnAttackHit(Collider other, float force)
    {
        Rigidbody hitRigidbody = other.GetComponent<Rigidbody>();
        if (hitRigidbody != null)
        {
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
                    Destroy(effectInstance, 2f);
                }
            }

            // ★修正: SoundManager経由でヒットサウンドを再生
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayHitSound(other.gameObject); // ヒットしたオブジェクトを渡す
            }
            else
            {
                Debug.LogWarning("SoundManager.Instance is not found. Cannot play hit sound.", this);
            }

            // スローモーション演出を呼び出す
            StartSlowMotion();

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
}