using UnityEngine;

// このスクリプトは、プレイヤーのパンチ/キックコライダーにアタッチされます。
// コライダーが他のオブジェクト（Rigidbodyを持つ）に衝突した際に、
// プレイヤーコントローラーのOnAttackHitメソッドを呼び出します。
public class AttackColliderHandler : MonoBehaviour
{
    // プレイヤーのPlayerControllerスクリプトへの参照
    private PlayerController playerController;

    // この攻撃コライダーが与える力の種類 (例: PunchForce or KickForce)
    public float AttackForce { get; set; } // PlayerControllerから設定される攻撃力

    // PlayerControllerからこのコライダーを制御するための参照を設定するメソッド
    public void SetPlayerController(PlayerController controller)
    {
        playerController = controller;
    }

    void Awake()
    {
        // ★修正: Awake()でのplayerControllerのnullチェックは削除します。
        // PlayerController.Awake()がSetPlayerControllerを呼び出す前に
        // このAttackColliderHandler.Awake()が呼ばれる可能性があるため、
        // ここでチェックすると誤った警告が出てしまいます。
        // Start()で改めてnullチェックを行います。

        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning(gameObject.name + " collider is not set to Is Trigger. Please enable Is Trigger for attack colliders.");
        }
    }

    // ★追加: Start()メソッドでplayerControllerが設定されているか最終確認を行う
    void Start()
    {
        // GameManagerのAwake()はPlayerControllerのAwake()より先に実行されることが保証されないため、
        // ここでの参照がまだ設定されていない場合がある。
        // ただし、PlayerControllerから明示的にSetPlayerControllerが呼ばれるため、
        // 通常はStart()の時点では設定されているはず。
        if (playerController == null)
        {
            Debug.LogError("PlayerController is STILL not set for AttackColliderHandler on " + gameObject.name + ". This should not happen if PlayerController's Awake() ran correctly and assigned it.", this);
        }
    }

    // Trigger設定されたコライダーが他のコライダーと衝突したときに呼び出される
    void OnTriggerEnter(Collider other)
    {
        if (playerController == null)
        {
            Debug.LogWarning("PlayerController is not assigned to AttackColliderHandler on " + gameObject.name + ". Cannot process hit.", this);
            return;
        }

        // 自分自身（プレイヤーの一部）には反応しないようにする
        if (other.gameObject.CompareTag("Player") || (playerController != null && other.transform.IsChildOf(playerController.transform)))
        {
            return;
        }

        // 相手がRigidbodyを持っているか確認
        Rigidbody otherRigidbody = other.GetComponent<Rigidbody>();
        if (otherRigidbody != null)
        {
            // PlayerControllerのOnAttackHitメソッドを呼び出し、力を加える
            playerController.OnAttackHit(other, AttackForce);

            // ★追加: GameManagerのAddScoreメソッドを呼び出してスコアを加算する
            // Hitしたオブジェクトとそのオブジェクトのタグを渡す
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
    // もしOnCollisionEnterを使っている場合はそちらも考慮する
    void OnCollisionEnter(Collision collision)
    {
        // OnTriggerEnterと同様のロジックを適用
        if (playerController == null)
        {
            Debug.LogWarning("PlayerController is not assigned to AttackColliderHandler on " + gameObject.name + ". Cannot process hit (OnCollisionEnter).", this);
            return;
        }

        if (collision.gameObject.CompareTag("Player") || (playerController != null && collision.transform.IsChildOf(playerController.transform)))
        {
            return;
        }

        Rigidbody otherRigidbody = collision.collider.GetComponent<Rigidbody>();
        if (otherRigidbody != null)
        {
            playerController.OnAttackHit(collision.collider, AttackForce);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(collision.gameObject, collision.gameObject.tag);
            }
            else
            {
                Debug.LogWarning("GameManager.Instance is null. Cannot add score (OnCollisionEnter).");
            }
        }
    }
}