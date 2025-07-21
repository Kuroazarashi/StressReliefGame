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
        // 単純にY座標を0に設定します。
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

        // 移動方向ベクトルを作成 (Y軸は無視し、X, Z平面で移動)
        // ジョイスティックのY軸入力は、キャラクターのZ軸方向の移動に対応させる
        Vector3 moveDirection = new Vector3(inputDir.x, 0f, inputDir.y).normalized;

        // 地面に対して重力を適用 (CharacterControllerが地面にいない場合)
        // CharacterController.Moveは重力を自動で適用しないため、手動で加える
        if (!characterController.isGrounded)
        {
            moveDirection.y += Physics.gravity.y * Time.deltaTime;
        }

        // CharacterControllerを使ってプレイヤーを移動
        // Time.deltaTimeを乗算してフレームレートに依存しない移動にする
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        // プレイヤーの向きを移動方向に向ける (ジョイスティックに一定以上の入力がある場合のみ)
        if (moveDirection.magnitude >= 0.1f) // 微妙な入力で回転しないように閾値を設ける
        {
            // Y軸回転のみを考慮し、キャラクターの正面を移動方向に向ける
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveDirection.x, 0, moveDirection.z));
            // 徐々に目標の向きに回転させることで、滑らかな動きを実現
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }

        // Animatorに移動速度を渡す (アニメーション制御用)
        // "Speed"パラメーターは、Animator Controllerで歩行/走行アニメーションのブレンドツリーに使用する予定
        if (animator != null)
        {
            // ジョイスティックの入力の大きさをAnimatorの"Speed"パラメーターに渡す
            // magnitudeはベクトルの長さ（強さ）を表し、0〜1の値になる
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