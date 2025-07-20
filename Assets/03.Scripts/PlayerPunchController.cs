using UnityEngine;
using UnityEngine.InputSystem; // Input Systemを使用するために必要
using UnityEngine.UI; // UIを使用するために必要 (ButtonのOnClick()設定で直接参照するため、スクリプト内での明示的な使用は少ないかもしれませんが念のため)
using Unity.Cinemachine; // Cinemachineを使用するために必要
using System.Collections; // Coroutineを使用するために必要

public class PlayerPunchController : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float punchForce = 1000f; // パンチの力
    [SerializeField] private float kickForce = 800f;   // キックの力（仮の値、後でInspectorで調整）
    [SerializeField] private float upwardForceMultiplier = 0.5f; // 上方向への力の倍率

    [Header("References")]
    [SerializeField] private Animator playerAnimator; // プレイヤーのアニメーターコンポーネント (Inspectorから設定)
    [SerializeField] private Collider punchCollider;  // 拳のコライダー (Inspectorから設定)
    [SerializeField] private Collider kickCollider;   // 足のコライダー (Inspectorから設定)

    // スローモーション関連の変数
    [Header("Slow Motion Settings")]
    [SerializeField] private float slowMotionDuration = 3.0f;
    [SerializeField] private float slowMotionTimeScale = 0.1f;
    private float originalTimeScale;
    private float originalFixedDeltaTime;

    // Cinemachine関連の変数
    [Header("Cinemachine Settings")]
    [SerializeField] private CinemachineCamera virtualCameraComponent; // 既存のVirtual Camera (Inspectorから設定)
    // 吹き飛ぶオブジェクトを追従するターゲットはOnTriggerEnterでhitRigidbody.transformを直接指定するため、別途変数不要

    // Input System関連 (マウス入力は削除し、UIボタンに置き換わるためコメントアウトまたは削除)
    // private PlayerInputActions playerInputActions;

    void Awake()
    {
        // PlayerInputActionsはUIボタンで操作するため不要
        // playerInputActions = new PlayerInputActions(); 

        // AnimatorがInspectorで設定されていなければ、ここで取得を試みる
        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }
        if (playerAnimator == null)
        {
            Debug.LogError("Animator component not found on Player or its children!");
        }
    }

    void Start()
    {
        // 初期状態でコライダーを無効化
        // Inspectorで設定されていない場合はエラーを出す
        if (punchCollider == null) Debug.LogError("Punch Collider is not assigned in Inspector!");
        if (kickCollider == null) Debug.LogError("Kick Collider is not assigned in Inspector!");

        if (punchCollider != null) punchCollider.enabled = false;
        if (kickCollider != null) kickCollider.enabled = false;

        originalTimeScale = Time.timeScale;
        originalFixedDeltaTime = Time.fixedDeltaTime;

        // CinemachineVirtualCameraが未設定の場合はエラーを出す
        if (virtualCameraComponent == null)
        {
            Debug.LogError("CinemachineVirtualCamera is not assigned in Inspector!");
        }

        // 初期状態でカメラはプレイヤーを追従
        if (virtualCameraComponent != null)
        {
            virtualCameraComponent.Follow = transform;
            virtualCameraComponent.LookAt = transform;
        }
    }

    void OnEnable()
    {
        // PlayerInputActionsはUIボタンで操作するため不要
        // playerInputActions.Enable();
        // playerInputActions.Gameplay.Punch.performed += OnPunchPerformed;
    }

    void OnDisable()
    {
        // PlayerInputActionsはUIボタンで操作するため不要
        // playerInputActions.Gameplay.Punch.performed -= OnPunchPerformed;
        // playerInputActions.Disable();
    }

    // ★★★ここからUIボタンからの入力で呼び出されるメソッド★★
    // パンチボタンが押された時に呼び出すメソッド
    public void OnPunchButtonClick()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("PunchTrigger"); // 既存のPunchTriggerを使用
        }
        Debug.Log("Punch Button Clicked! Playing Punch Animation.");
    }

    // キックボタンが押された時に呼び出すメソッド
    public void OnKickButtonClick()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("KickTrigger"); // Animatorに新しい"KickTrigger"を追加する必要あり
        }
        Debug.Log("Kick Button Clicked! Playing Kick Animation.");
    }

    // ★★★ここからアニメーションイベントから呼び出されるメソッド★★★
    // アニメーションイベントから呼び出し、拳コライダーを有効にする
    public void EnablePunchCollider()
    {
        if (punchCollider != null)
        {
            punchCollider.enabled = true;
            Debug.Log("Punch Collider Enabled!");
        }
    }

    // アニメーションイベントから呼び出し、拳コライダーを無効にする
    public void DisablePunchCollider()
    {
        if (punchCollider != null)
        {
            punchCollider.enabled = false;
            Debug.Log("Punch Collider Disabled!");
        }
    }

    // アニメーションイベントから呼び出し、足コライダーを有効にする
    public void EnableKickCollider()
    {
        if (kickCollider != null)
        {
            kickCollider.enabled = true;
            Debug.Log("Kick Collider Enabled!");
        }
    }

    // アニメーションイベントから呼び出し、足コライダーを無効にする
    public void DisableKickCollider()
    {
        if (kickCollider != null)
        {
            kickCollider.enabled = false;
            Debug.Log("Kick Collider Disabled!");
        }
    }

    // ★★★衝突検出のロジックをOnTriggerEnterで記述★★★
    // このメソッドが呼び出されるには、このスクリプトがアタッチされているGameObject（MaleSuit_A）と
    // その子オブジェクトのパンチ/キックコライダーの両方にIs Triggerが有効なColliderが必要
    // かつ、MaleSuit_AにはRigidbodyもアタッチされている必要がある（Is Kinematicをtrueに推奨）
    void OnTriggerEnter(Collider other)
    {
        // 攻撃コライダーが有効な時のみ処理を行う
        // 自分のコライダー自体が衝突を検出した場合の誤作動を防ぐ
        if (punchCollider != null && punchCollider.enabled && other == punchCollider) return;
        if (kickCollider != null && kickCollider.enabled && other == kickCollider) return;

        // 衝突した相手がRigidbodyを持っているか確認
        Rigidbody hitRigidbody = other.GetComponent<Rigidbody>();

        if (hitRigidbody != null)
        {
            // 攻撃の種類を判定し、適切な力を適用
            float currentForce = 0f;
            // どの攻撃コライダーが有効かで攻撃の種類を判断
            if (punchCollider != null && punchCollider.enabled)
            {
                currentForce = punchForce;
                DisablePunchCollider(); // ヒットしたらコライダーをすぐに無効にする
            }
            else if (kickCollider != null && kickCollider.enabled)
            {
                currentForce = kickForce;
                DisableKickCollider(); // ヒットしたらコライダーをすぐに無効にする
            }

            if (currentForce > 0)
            {
                // 力を加える方向を計算（プレイヤーからオブジェクトへ）
                Vector3 direction = (other.transform.position - transform.position).normalized;
                // 上方向への力を加える
                direction.y = upwardForceMultiplier; // 上方向への倍率を適用
                direction = direction.normalized; // 再度正規化して、合計の力が変わらないようにする

                hitRigidbody.AddForce(direction * currentForce, ForceMode.Impulse);

                // スローモーション開始
                StartCoroutine(DoSlowMotion(hitRigidbody.transform)); // 吹き飛ぶオブジェクトを渡す

                // スコア加算（ここでは仮に10点加算、後でスコアシステムを統合）
                // GameManager.Instance.AddScore(10); // GameManagerを実装後に置き換え
                Debug.Log("Hit! Object: " + other.name + ", Force: " + currentForce);
            }
        }
    }

    // スローモーションコルーチンを修正し、ターゲットを引数で受け取るようにする
    IEnumerator DoSlowMotion(Transform targetToFollow)
    {
        float originalFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = slowMotionTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;

        // カメラを吹き飛ぶオブジェクトに追従させる
        if (virtualCameraComponent != null)
        {
            virtualCameraComponent.Follow = targetToFollow;
            virtualCameraComponent.LookAt = targetToFollow;
        }

        yield return new WaitForSecondsRealtime(slowMotionDuration);

        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        // カメラをプレイヤーに戻す
        if (virtualCameraComponent != null)
        {
            virtualCameraComponent.Follow = transform; // Playerに戻す
            virtualCameraComponent.LookAt = transform;
        }
    }

    // ★★★Input SystemのOnPunchPerformedはUIボタンに置き換わるため削除★★
    // void OnPunchPerformed(InputAction.CallbackContext context)
    // {
    //    // このメソッド内の処理はOnPunchButtonClick()とOnTriggerEnter()に分割されるため不要
    // }
}