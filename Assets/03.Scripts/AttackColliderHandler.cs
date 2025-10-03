using UnityEngine;

// このスクリプトは、プレイヤーのパンチ/キックコライダーにアタッチされます。
public class AttackColliderHandler : MonoBehaviour
{
    private PlayerController playerController;
    public float AttackForce { get; set; }
    private Rigidbody rb;

    [SerializeField]
    private LayerMask wallLayerMask; // 壁と判定するレイヤー

    public void SetPlayerController(PlayerController controller)
    {
        playerController = controller;
    }

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning(gameObject.name + " collider is not set to Is Trigger. Please enable Is Trigger for attack colliders.");
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Start()
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController is STILL not set for AttackColliderHandler on " + gameObject.name, this);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        ProcessHit(other);
    }

    void OnCollisionEnter(Collision collision)
    {
        ProcessHit(collision.collider);
    }

    /// <summary>
    /// 衝突処理を一本化するためのプライベートメソッド
    /// </summary>
    private void ProcessHit(Collider other)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded()) return;
        if (playerController == null) return;
        if (other.gameObject.CompareTag("Player") || other.transform.IsChildOf(playerController.transform)) return;

        // ▼▼▼▼▼ ここから下がRaycastによる遮蔽チェック ▼▼▼▼▼

        // プレイヤーの中心あたりから、ヒットしたオブジェクトの中心への方向と距離を計算
        Vector3 playerCenter = playerController.transform.position + Vector3.up * 1.0f; // 少し高さを調整
        Vector3 direction = other.transform.position - playerCenter;
        float distance = direction.magnitude;

        // Raycastを実行
        if (Physics.Raycast(playerCenter, direction.normalized, out RaycastHit hit, distance, wallLayerMask))
        {
            // RaycastがWallsレイヤーにヒットした場合、それは壁越しの攻撃と判断
            Debug.Log("Attack blocked by wall: " + hit.collider.name);
            return; // ヒット処理を中断
        }

        // ▲▲▲▲▲ ここまでがRaycastによる遮蔽チェック ▲▲▲▲▲


        Rigidbody otherRigidbody = other.GetComponent<Rigidbody>();
        if (otherRigidbody != null)
        {
            playerController.OnAttackHit(other, AttackForce);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(other.gameObject, other.tag);
            }
            else
            {
                Debug.LogWarning("GameManager.Instance is null. Cannot add score.");
            }
        }
    }
}
