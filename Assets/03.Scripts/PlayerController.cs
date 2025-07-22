using UnityEngine;
using UnityEngine.UI; // Joystickスクリプトで使用しているため必要です
// using UnityEngine.EventSystems; // Joystickスクリプト自体がEventSystemを使用するので、PlayerControllerでは通常不要です。今回は削除します。
// using Cinemachine; // 新しいカメラワークではCinemachineVirtualCameraへの直接参照は不要になったため、コメントアウト（または削除）します。

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

    // === Movement Settings ===
    [Header("Movement Settings")] // 移動設定用のヘッダー
    [SerializeField] private Joystick joystick; // UIのJoystickスクリプトへの参照
    [SerializeField] private float moveSpeed = 5f; // 移動速度
    [SerializeField] private float rotateSpeed = 500f; // 回転速度

    // === Private References ===
    private CharacterController characterController; // CharacterControllerへの参照
    private bool isAttacking = false; // 攻撃中かどうかのフラグを追加

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
        // CharacterControllerは自身のTransform.positionを直接変更すると挙動がおかしくなる場合があるため、
        // 初期位置調整はCharacterControllerが有効になる前に行うか、CharacterController.Moveを使用する
        // または、Awake/Start時に一度だけCharacterController.Moveで設定したい目標Y座標に移動させる、などの方法があります。
        // 現在の記述はAwakeなので、これで問題ない可能性が高いですが、念のためコメントアウトしておきます。
        // transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
        // ===========================================
    }

    void Update()
    {
        // 攻撃中でない場合のみ移動処理を行う
        if (!isAttacking)
        {
            HandleMovement();
        }
        else
        {
            // 攻撃中は移動アニメーションを停止させるためにSpeedパラメーターを0に設定
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }
            // 攻撃中はキャラクターを停止させる（CharacterController.MoveにVector3.zeroを渡す）
            characterController.Move(Vector3.zero); // または CharacterController.velocity = Vector3.zero; など
        }

        // 重力は常時適用 (攻撃中も落下させるため、isAttackingの条件外で実行)
        if (!characterController.isGrounded)
        {
            characterController.Move(Vector3.up * Physics.gravity.y * Time.deltaTime);
        }

        // === パンチボタン入力処理（コメントアウト） ===
        // UIボタンからのイベントはOnPunchButtonClick()で処理されるため、ここではキー入力の例のみ
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    OnPunchButtonClick();
        //}

        // === キックボタン入力処理（コメントアウト） ===
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
        Vector2 inputDir = joystick.InputDirection; // Joystickスクリプトが持つInputDirectionプロパティを使用

        // X, Z平面での移動方向ベクトル (Y軸は無視)
        // ジョイスティックのXはワールドX、YはワールドZに直接対応
        Vector3 horizontalMoveDirection = new Vector3(inputDir.x, 0f, inputDir.y);

        // Animatorに移動速度を渡す
        // ジョイスティックが倒されている量（入力強度）を直接Speedとして使う
        float currentSpeed = horizontalMoveDirection.magnitude; // ジョイスティックの入力強度
        if (animator != null)
        {
            animator.SetFloat("Speed", currentSpeed);
        }

        // CharacterControllerによる水平移動
        // AnimatorのSpeedが0.1fを超えたらRunアニメーションが再生されるので、
        // 少なくとも0.1f以上の入力がある場合にキャラクターを移動させる
        if (currentSpeed >= 0.1f) // ある程度の入力がある場合のみ移動
        {
            // 移動 (正規化して速度を一定に保つ)
            characterController.Move(horizontalMoveDirection.normalized * moveSpeed * Time.deltaTime);

            // プレイヤーの向きを移動方向に向ける (Y軸回転のみ)
            Quaternion targetRotation = Quaternion.LookRotation(horizontalMoveDirection.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }
        // 入力がない（currentSpeedが0.1f未満）場合はキャラクターをその場に留める
        else
        {
            characterController.Move(Vector3.zero); // 移動入力を受け付けない
        }
    }


    // === パンチ関連メソッド ===

    // UIボタンから呼び出されるメソッド
    public void OnPunchButtonClick()
    {
        if (!isAttacking) // 攻撃中でない場合のみ実行
        {
            isAttacking = true; // 攻撃フラグを立てる
            Debug.Log("Punch Button Clicked! Playing Punch Animation."); // デバッグログ
            if (animator != null)
            {
                animator.SetTrigger("PunchTrigger"); // Animatorの"PunchTrigger"トリガーを設定
            }
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
        if (!isAttacking) // 攻撃中でない場合のみ実行
        {
            isAttacking = true; // 攻撃フラグを立てる
            Debug.Log("Kick Button Clicked! Playing Kick Animation."); // デバッグログ
            if (animator != null)
            {
                animator.SetTrigger("KickTrigger"); // Animatorの"KickTrigger"トリガーを設定
            }
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

    // アニメーションイベントから呼び出されるメソッド (アニメーション終了時に攻撃フラグをリセット)
    // このメソッドをPunchアニメーションとKickアニメーションの終了時にAnimatorイベントとして追加してください。
    public void ResetAttackState()
    {
        isAttacking = false;
        // 攻撃アニメーション終了時に念のためコライダーも無効化
        if (punchColliderObject != null) punchColliderObject.SetActive(false);
        if (kickColliderObject != null) kickColliderObject.SetActive(false);
    }


    // === 攻撃がヒットした際の処理 ===
    // 攻撃コライダーにアタッチされたスクリプトから呼び出されることを想定
    // ※ 現在のプロジェクトでは、AttackColliderHandler.csのような別スクリプトが
    //    このメソッドを呼び出す想定になっているため、そのスクリプト側で実装する必要があります。
    //    PlayerController単体ではOnTriggerEnterは攻撃コライダーのGameObjectにアタッチされていなければ呼び出されません。
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
    // [SerializeField] private CinemachineVirtualCamera virtualCamera; // 以前の記述ではコメントアウトされていましたが、もしインスペクターから設定するなら必要です。
    // しかし、現在はCinemachinePositionComposerでプレイヤーを直接追従するため、PlayerController側からのカメラ操作は不要です。
    //public void TrackHitObject(Transform target)
    //{
    //    if (virtualCamera != null)
    //    {
    //        virtualCamera.Follow = target;
    //        virtualCamera.LookAt = target;
    //    }
    //}
}