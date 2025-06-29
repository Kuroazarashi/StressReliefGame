using UnityEngine;
using UnityEngine.InputSystem; // 追加: Input Systemを使用するために必要

public class PlayerPunchController : MonoBehaviour
{
    [SerializeField] private float punchForce = 1000f; // パンチの強さ（Inspectorで調整可能）
    [SerializeField] private float upwardForceMultiplier = 0.5f; // 上方向への力の倍率（少し上にも飛ばすため）

    private PlayerInputActions playerInputActions; // 自動生成されたInput Actionsクラスのインスタンス

    // Start関数は、スクリプトが有効になった最初のフレームで一度だけ呼び出されます。
    void Start()
    {
        // Input Actionsのインスタンスを作成
        playerInputActions = new PlayerInputActions();

        // Gameplay Action Mapを有効にする
        playerInputActions.Gameplay.Enable();

        // PunchアクションがPerformed（実行された）時に呼び出されるメソッドを設定
        playerInputActions.Gameplay.Punch.performed += OnPunchPerformed;
    }

    // OnPunchPerformedは、Punchアクションが実行されたときに呼び出されます。
    private void OnPunchPerformed(InputAction.CallbackContext context)
    {
        // マウスのクリック位置を取得
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Rayを飛ばす準備
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit; // Rayが当たったオブジェクトの情報

        // Rayを飛ばし、何かに当たったかどうかをチェック
        if (Physics.Raycast(ray, out hit))
        {
            // Rayが当たったオブジェクトにRigidbodyがあるか確認
            Rigidbody hitRigidbody = hit.collider.GetComponent<Rigidbody>();

            if (hitRigidbody != null)
            {
                // パンチの方向を計算
                Vector3 punchDirection = (hit.point - Camera.main.transform.position).normalized;

                // 少し上方向に力を加えることで、放物線を描いて飛ぶようにする
                punchDirection.y += upwardForceMultiplier;
                punchDirection = punchDirection.normalized; // 再度正規化

                // 力を加える（Impulseモードで瞬間的な力を加える）
                hitRigidbody.AddForce(punchDirection * punchForce, ForceMode.Impulse);

                Debug.Log("パンチ！ オブジェクト: " + hit.collider.name + "に力を加えました。");
            }
        }
    }

    // OnDisable関数は、スクリプトが無効になったり、ゲームオブジェクトが破棄されたりするときに呼び出されます。
    // イベントの購読解除は、メモリリークを防ぐために重要です。
    void OnDisable()
    {
        playerInputActions.Gameplay.Punch.performed -= OnPunchPerformed;
        playerInputActions.Gameplay.Disable();
        playerInputActions.Dispose(); // インスタンスを破棄
    }
}