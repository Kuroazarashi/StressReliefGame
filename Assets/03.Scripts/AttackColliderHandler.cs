using UnityEngine;

// このスクリプトは、プレイヤーのパンチ/キックコライダーにアタッチされます。
// コライダーが他のオブジェクト（Rigidbodyを持つ）に衝突した際に、
// プレイヤーコントローラーのOnAttackHitメソッドを呼び出します。
public class AttackColliderHandler : MonoBehaviour
{
    // プレイヤーのPlayerControllerスクリプトへの参照
    // Inspectorで設定するか、Awake/Startで自動取得します。
    [SerializeField] private PlayerController playerController;

    // この攻撃コライダーが与える力の種類 (例: PunchForce or KickForce)
    // Inspectorで設定します。
    public float attackForce = 1000f;

    void Awake()
    {
        // PlayerControllerがInspectorで設定されていない場合、親オブジェクトから探す
        if (playerController == null)
        {
            // 通常はプレイヤーモデルのルートオブジェクトにPlayerControllerがアタッチされているはず
            playerController = GetComponentInParent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerController not found in parent for AttackColliderHandler on " + gameObject.name);
            }
        }
        // 攻撃コライダーはIs Triggerに設定されている必要があります
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning(gameObject.name + " collider is not set to Is Trigger. Please enable Is Trigger for attack colliders.");
            // 必要であればここで自動的にisTriggerをtrueにする
            // col.isTrigger = true;
        }
    }

    // Trigger設定されたコライダーが他のコライダーと衝突したときに呼び出される
    void OnTriggerEnter(Collider other)
    {
        // 自分自身（プレイヤーの一部）には反応しないようにする
        // プレイヤーのルートオブジェクトや、コライダー自身のオブジェクト、または他のプレイヤーのコライダーなど
        if (other.gameObject.CompareTag("Player") || other.transform.IsChildOf(playerController.transform))
        {
            return; // プレイヤー自身またはプレイヤーの子オブジェクトとの衝突は無視
        }

        // 相手がRigidbodyを持っているか確認
        Rigidbody otherRigidbody = other.GetComponent<Rigidbody>();
        if (otherRigidbody != null)
        {
            // PlayerControllerのOnAttackHitメソッドを呼び出し、力を加える
            if (playerController != null)
            {
                playerController.OnAttackHit(other, attackForce);
            }
        }
    }
}