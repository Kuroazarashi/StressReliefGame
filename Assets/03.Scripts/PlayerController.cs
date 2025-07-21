using UnityEngine;
using UnityEngine.EventSystems; // Joystickスクリプトで使用しているので念のため残しておく

public class PlayerController : MonoBehaviour
{
    // === Attack Settings ===
    [Header("Attack Settings")]
    [SerializeField] private Animator animator; // アニメーターへの参照
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

    // === Cinemachine Settings (現在の追従カメラとは異なる) ===
    // ※ 新しい仕様ではプレイヤー追従カメラに変更するため、以下の設定は将来的に変更または削除されます。
    //[Header("Cinemachine Settings")]
    //[SerializeField] private CinemachineVirtualCamera virtualCamera; // シネマシン仮想カメラへの参照

    // === Movement Settings ===
    [Header("Movement Settings")] // 移動設定用のヘッダーを追加
    [SerializeField] private Joystick joystick; // UIのJoystickスクリプトへの参照
    [SerializeField] private float moveSpeed = 5f; // 移動速度
    [SerializeField] private float rotateSpeed = 500f; // 回転速度

    private CharacterController characterController; // CharacterControllerへの参照

    // AwakeはStartより前に呼ばれる
    void Awake()
    {
        // CharacterControllerとAnimatorコンポーネントを取得
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>(); // MaleSuit_AのAnimatorを取得

        // コンポーネントがアタッチされているか確認（エラーログ用）
        if (characterController == null)
        {
            Debug.LogError("CharacterController not found on " + gameObject.name);
        }
        if (animator == null)
        {
            Debug.LogError("Animator not found on " + gameObject.name);
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

        // === 修正: キャラクターの初期Y座標を固定する ===
        // 現在のtransform.position.yが意図しない値になっている可能性があるので、
        // 単純にY座標を0.1fに設定します。（地面から少し浮かせたい場合）
        // もし地面のY座標が0以外の場合は、その値に合わせてください。
        transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
        // ===========================================
    }

    void Update()
    {
        // === 移動処理 ===
        HandleMovement();

        // === パンチボタン入力処理 ===
        // UIボタンからのイベントはOnPunchButtonClick()で処理されるため、ここではキー入力の例のみ
        //if (Input.GetKeyDown(KeyCode.P)) 
        //{
        //    OnPunchButtonClick();
        //}

        // === キックボタン入力処理 ===
        // UIボタンからのイベントはOnKickButtonClick()で処理されるため、ここではキー入力の例のみ
        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    OnKickButtonClick();
        //}
    }

    // === 移動処理メソッド ===
    private void HandleMovement()
    {
        // ジョイスティックの入力方向を取得
        Vector2 inputDir = joystick.InputDirection;

        // X, Z平面での移動方向ベクトル (Y軸は無視)
        // ジョイスティックのXはワールドX、YはワールドZに直接対応
        // ★ここをワールド座標基準の移動に戻しました★
        Vector3 horizontalMoveDirection = new Vector3(inputDir.x, 0f, inputDir.y);

        // CharacterControllerによる水平移動
        if (horizontalMoveDirection.magnitude >= 0.1f) // ある程度の入力がある場合のみ移動
        {
            // 移動 (正規化して速度を一定に保つ)
            characterController.Move(horizontalMoveDirection.normalized * moveSpeed * Time.deltaTime);

            // プレイヤーの向きを移動方向に向ける (Y軸回転のみ)
            // ワールドのZ軸を「上」とした時のジョイスティックのY方向入力は、
            // キャラクターのZ方向の移動に対応するため、LookRotationに渡すベクトルもそれに合わせる
            Quaternion targetRotation = Quaternion.LookRotation(horizontalMoveDirection.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }

        // 重力は常時適用
        if (!characterController.isGrounded)
        {
            // CharacterControllerはMoveメソッドで重力の影響を別途適用する必要がある
            characterController.Move(Vector3.up * Physics.gravity.y * Time.deltaTime);
        }

        // Animatorに移動速度を渡す
        if (animator != null)
        {
            animator.SetFloat("Speed", inputDir.magnitude);
        }
    }


    // === パンチ関連メソッド ===

    // UIボタンから呼び出されるメソッド
    public void OnPunchButtonClick()
    {
        Debug.Log("Punch Button Clicked! Playing Punch Animation."); // デバッグログ
        if (animator != null)
        {
            animator.SetTrigger("PunchTrigger"); // Animatorの"PunchTrigger"トリガーを設定
        }
    }

    // アニメーションイベントから呼び出されるメソッド (パンチコライダー有効化)
    public void EnablePunchCollider()
    {
        if (punchColliderObject != null)
        {
            punchColliderObject.SetActive(true); // パンチコライダーを有効化
            Debug.Log("Punch Collider Enabled!"); // デバッグログ
        }
    }

    // アニメーションイベントから呼び出されるメソッド (パンチコライダー無効化)
    public void DisablePunchCollider()
    {
        if (punchColliderObject != null)
        {
            punchColliderObject.SetActive(false); // パンチコライダーを無効化
            Debug.Log("Punch Collider Disabled!"); // デバッグログ
        }
    }

    // === キック関連メソッド ===

    // UIボタンから呼び出されるメソッド
    public void OnKickButtonClick()
    {
        Debug.Log("Kick Button Clicked! Playing Kick Animation."); // デバッグログ
        if (animator != null)
        {
            animator.SetTrigger("KickTrigger"); // Animatorの"KickTrigger"トリガーを設定
        }
    }

    // アニメーションイベントから呼び出されるメソッド (キックコライダー有効化)
    public void EnableKickCollider()
    {
        if (kickColliderObject != null)
        {
            kickColliderObject.SetActive(true); // キックコライダーを有効化
            Debug.Log("Kick Collider Enabled!"); // デバッグログ
        }
    }

    // アニメーションイベントから呼び出されるメソッド (キックコライダー無効化)
    public void DisableKickCollider()
    {
        if (kickColliderObject != null)
        {
            kickColliderObject.SetActive(false); // キックコライダーを無効化
            Debug.Log("Kick Collider Disabled!"); // デバッグログ
        }
    }


    // === 攻撃がヒットした際の処理 ===
    // 攻撃コライダーにアタッチされたスクリプトから呼び出されることを想定
    public void OnAttackHit(Collider other, float force)
    {
        // ヒットしたオブジェクトがRigidbodyを持っているか確認
        Rigidbody hitRigidbody = other.GetComponent<Rigidbody>();
        if (hitRigidbody != null)
        {
            // 攻撃の方向（プレイヤーからヒットしたオブジェクトへの方向）
            Vector3 attackDirection = (other.transform.position - transform.position).normalized;
            // 上方向への力を少し加える
            Vector3 totalForce = (attackDirection + Vector3.up * upwardForceMultiplier).normalized * force;

            hitRigidbody.AddForce(totalForce, ForceMode.Impulse); // 力を加える

            // スローモーション演出を呼び出す
            StartSlowMotion();

            // ここにスコア加算ロジックや破壊エフェクトなどを追加する
            Debug.Log($"Hit {other.name} with force {force}!");
        }
    }

    // === スローモーション演出 ===
    private void StartSlowMotion()
    {
        Time.timeScale = slowMotionTimeScale; // タイムスケールをスローモーション用の値に変更
        Invoke("EndSlowMotion", slowMotionDuration); // 指定時間後にスローモーションを終了するメソッドを呼び出す
    }

    private void EndSlowMotion()
    {
        Time.timeScale = 1f; // タイムスケールを通常速度に戻す
    }

    // 現在のCinemachineカメラ追従はオブジェクトが吹っ飛んだとき。
    // 新しい仕様ではプレイヤーに常に追従する形になるため、この部分は変更または削除されます。
    //public void TrackHitObject(Transform target)
    //{
    //    if (virtualCamera != null)
    //    {
    //        virtualCamera.Follow = target;
    //        virtualCamera.LookAt = target;
    //    }
    //}
}