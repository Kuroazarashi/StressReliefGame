using UnityEngine;
using UnityEngine.InputSystem; // Input Systemを使用するために必要
using Unity.Cinemachine; // Cinemachineを使用するために必要
using System.Collections; // コルーチンを使用するために必要

public class PlayerPunchController : MonoBehaviour
{
    [SerializeField] private float punchForce = 1000f; // パンチの強さ (Inspectorで調整可能)
    [SerializeField] private float upwardForceMultiplier = 0.5f; // 上方向への力の倍率 (少し上にも飛ぶように)

    private PlayerInputActions playerInputActions; // 自動生成された Input Actions クラスのインスタンス

    // Cinemachine Virtual Cameraへの参照
    // ここを CinemachineVirtualCamera から Unity.Cinemachine.CinemachineCamera に変更
    [SerializeField] private Unity.Cinemachine.CinemachineCamera virtualCamera; // Inspectorで設定できるように

    // スローモーション関連の変数
    [SerializeField] private float slowMotionDuration = 3.0f; // スローモーションの持続時間
    [SerializeField] private float slowMotionTimeScale = 0.1f; // スローモーション時のTime.timeScale

    // Awake() メソッドを追加し、virtualCameraが設定されていない場合に自動で探す
    void Awake()
    {
        if (virtualCamera == null)
        {
            // シーン内の最初のUnity.Cinemachine.CinemachineCameraコンポーネントを自動で探して割り当てる
            virtualCamera = FindObjectOfType<Unity.Cinemachine.CinemachineCamera>();
            if (virtualCamera == null)
            {
                // もし見つからなければエラーログを出力
                Debug.LogError("PlayerPunchController: CinemachineCamera not found in scene! Please assign it in the Inspector or ensure one exists.", this);
            }
        }
    }

    void Start()
    {
        // Input Actionsのインスタンスを作成
        playerInputActions = new PlayerInputActions();

        // Gameplay Action Mapを有効にする
        playerInputActions.Gameplay.Enable();

        // PunchアクションがPerformed (実行された) 時に呼び出されるメソッドを設定
        playerInputActions.Gameplay.Punch.performed += OnPunchPerformed;
    }

    void OnPunchPerformed(InputAction.CallbackContext context)
    {
        // マウスのクリック位置を取得
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Rayを飛ばす準備
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit; // Rayが当たったオブジェクトの情報

        // Rayを飛ばし、何かに当たったかチェック
        if (Physics.Raycast(ray, out hit))
        {
            // Rayが当たったオブジェクトがRigidbodyを持っているか確認
            Rigidbody hitRigidbody = hit.collider.GetComponent<Rigidbody>();

            if (hitRigidbody != null)
            {
                // パンチの方向を計算
                Vector3 punchDirection = (hit.point - Camera.main.transform.position).normalized;

                // 少し上方向にも力を加えることで、物体を浮かせて吹き飛ばすように調整
                punchDirection.y += upwardForceMultiplier;
                punchDirection.Normalize(); // 再び正規化

                // 力を加える (Impulseモードで瞬間的に力を加える)
                hitRigidbody.AddForce(punchDirection * punchForce, ForceMode.Impulse);

                // スコアを加算
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddScore(10); // 例として10点加算
                }

                // CinemachineカメラのTracking Targetをヒットしたオブジェクトに切り替える
                if (virtualCamera != null)
                {
                    virtualCamera.LookAt = hitRigidbody.transform;
                    virtualCamera.Follow = hitRigidbody.transform;
                }
                else
                {
                    Debug.LogWarning("PlayerPunchController: virtualCamera is not assigned! Cannot change camera target.", this);
                }

                // スローモーションを開始するコルーチンを呼び出す
                StartCoroutine(SlowMotionEffect());
            }
        }
    }

    // スローモーション効果のコルーチン
    IEnumerator SlowMotionEffect()
    {
        // 現在のTime.timeScaleを保存
        float originalTimeScale = Time.timeScale;
        float originalFixedDeltaTime = Time.fixedDeltaTime;

        // スローモーションにする
        Time.timeScale = slowMotionTimeScale;
        // FixedDeltaTimeも合わせて調整 (物理演算の更新頻度を維持するため)
        Time.fixedDeltaTime = originalFixedDeltaTime * slowMotionTimeScale;

        // 指定された持続時間だけ待機
        yield return new WaitForSecondsRealtime(slowMotionDuration); // Realtimeを使用することで、スローモーション中でも正確な時間を待機

        // 通常速度に戻す
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        // カメラのTracking TargetをPlayerに戻す
        if (virtualCamera != null)
        {
            virtualCamera.LookAt = this.transform; // このスクリプトがアタッチされているオブジェクト（プレイヤー）に戻す
            virtualCamera.Follow = this.transform;
        }
        else
        {
            Debug.LogWarning("PlayerPunchController: virtualCamera is not assigned! Cannot reset camera target.", this);
        }
    }

    // スクリプトが無効になったときにInput Actionsを無効にする
    void OnDisable()
    {
        if (playerInputActions != null)
        {
            playerInputActions.Gameplay.Disable();
            // イベント購読の解除 (メモリリーク防止のため)
            playerInputActions.Gameplay.Punch.performed -= OnPunchPerformed;
        }
    }
}