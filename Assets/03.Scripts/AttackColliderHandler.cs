using UnityEngine;

// このスクリプトは、プレイヤーのパンチ/キックコライダーにアタッチされます。
// コライダーが他のオブジェクト（Rigidbodyを持つ）に衝突した際に、
// プレイヤーコントローラーのOnAttackHitメソッドを呼び出します。
public class AttackColliderHandler : MonoBehaviour
{
    // プレイヤーのPlayerControllerスクリプトへの参照
    // PlayerController側からこの参照を設定するため、[SerializeField]は削除しprivateにします。
    // ★修正: [SerializeField] を削除
    private PlayerController playerController;

    // この攻撃コライダーが与える力の種類 (例: PunchForce or KickForce)
    // PlayerControllerから設定できるようにpublicプロパティに変更します。
    // ★修正: attackForce -> AttackForce (プロパティ化)
    public float AttackForce { get; set; } // PlayerControllerから設定される攻撃力

    // PlayerControllerからこのコライダーを制御するための参照を設定するメソッド // ★追加
    public void SetPlayerController(PlayerController controller) // ★追加
    {                                                           // ★追加
        playerController = controller;                          // ★追加
    }                                                           // ★追加

    void Awake()
    {
        // PlayerControllerがInspectorで設定されていない場合、親オブジェクトから探す
        // ★修正: このAwakeでのplayerControllerの自動取得は不要になります。
        //         PlayerController側からSetPlayerControllerで設定されるためです。
        //         ただし、念のため警告ログは残しておきます。
        if (playerController == null)
        {
            // Debug.LogError("PlayerController not found in parent for AttackColliderHandler on " + gameObject.name + ". Ensure it's set by PlayerController's Awake.", this);
            // 上記のGetComponentInParentはPlayerControllerがAwake時にセットするロジックと衝突するので削除します。
            // 代わりに警告のみにします。
            Debug.LogWarning("PlayerController is not set for AttackColliderHandler on " + gameObject.name + ". Make sure PlayerController assigns itself to this handler.", this);
        }

        // 攻撃コライダーはIs Triggerに設定されている必要があります
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning(gameObject.name + " collider is not set to Is Trigger. Please enable Is Trigger for attack colliders.");
            // 必要であればここで自動的にisTriggerをtrueにする
            // col.isTrigger = true; // 自動設定は予期せぬ挙動を招く可能性があるのでコメントアウトのまま推奨
        }
    }

    // Trigger設定されたコライダーが他のコライダーと衝突したときに呼び出される
    void OnTriggerEnter(Collider other)
    {
        // PlayerControllerが設定されていることを確認
        if (playerController == null)
        {
            Debug.LogWarning("PlayerController is not assigned to AttackColliderHandler on " + gameObject.name + ". Cannot process hit.", this);
            return;
        }

        // 自分自身（プレイヤーの一部）には反応しないようにする
        // プレイヤーのルートオブジェクトや、コライダー自身のオブジェクト、または他のプレイヤーのコライダーなど
        // ★修正: playerControllerがnullでないことを保証してからtransformにアクセス
        if (other.gameObject.CompareTag("Player") || (playerController != null && other.transform.IsChildOf(playerController.transform)))
        {
            return; // プレイヤー自身またはプレイヤーの子オブジェクトとの衝突は無視
        }

        // 相手がRigidbodyを持っているか確認
        Rigidbody otherRigidbody = other.GetComponent<Rigidbody>();
        if (otherRigidbody != null)
        {
            // PlayerControllerのOnAttackHitメソッドを呼び出し、力を加える
            // attackForceではなくAttackForceプロパティを使用
            playerController.OnAttackHit(other, AttackForce); // ★修正: attackForce -> AttackForce
        }
    }
}