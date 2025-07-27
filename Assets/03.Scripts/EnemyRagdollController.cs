using UnityEngine;
using System.Linq; // LINQを使用するために追加

public class EnemyRagdollController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody[] rigidbodies;
    private Collider[] colliders;
    private Collider mainCollider; // Enemyオブジェクト自身のBox Colliderへの参照を追加

    void Awake()
    {
        animator = GetComponent<Animator>();
        // 親オブジェクトを含む全ての子のRigidbodyとColliderを取得
        // ただし、Enemyオブジェクト自体のRigidbodyとColliderは除外する
        // Enemyオブジェクト自体にBoxColliderとRigidbodyが付いていることを考慮
        rigidbodies = GetComponentsInChildren<Rigidbody>().Where(rb => rb.gameObject != gameObject).ToArray();
        colliders = GetComponentsInChildren<Collider>().Where(col => col.gameObject != gameObject).ToArray();

        // Enemyオブジェクト自身のBox Colliderを取得し、別途管理する
        mainCollider = GetComponent<Collider>(); // EnemyオブジェクトのRigidbodyと同じ階層にあるCollider

        // 初期状態はアニメーション（ダンス）なので、ラグドールを無効化しておく
        SetRagdollState(false);
    }

    /// <summary>
    /// ラグドールの有効/無効を切り替えるメソッド
    /// </summary>
    /// <param name="enableRagdoll">trueでラグドール有効、falseで無効（アニメーション制御）</param>
    public void SetRagdollState(bool enableRagdoll)
    {
        // Animatorの有効/無効を切り替える
        if (animator != null)
        {
            animator.enabled = !enableRagdoll; // ラグドール有効ならAnimatorは無効、逆もまた然り
        }

        // 全てのRigidbodyのisKinematic状態を切り替える (EnemyルートのRigidbodyは含まない)
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = !enableRagdoll;
        }

        // 全てのColliderの有効/無効を切り替える (EnemyルートのBox Colliderは含まない)
        foreach (Collider col in colliders)
        {
            col.enabled = enableRagdoll;
        }

        // ★Enemyオブジェクト自身のメインコライダーは常に有効にしておく
        if (mainCollider != null)
        {
            mainCollider.enabled = true; // 常に有効にする
            // ただし、もしIs Triggerでない場合は物理衝突が発生しうるので注意
            // あなたのBox ColliderはIs Triggerになっているので問題ありません。
        }

        Debug.Log($"Ragdoll State set to: {enableRagdoll}. Animator Enabled: {animator.enabled}");
    }

    // 外部から力を加えるためのメソッド（ラグドール有効時に使用）
    public void ApplyForce(Vector3 force, Vector3 hitPoint)
    {
        // ラグドールが有効な状態でのみ力を加える
        if (animator != null && !animator.enabled) // Animatorが無効 = ラグドール有効
        {
            // ヒットした位置に最も近いRigidbodyを探して力を加える
            Rigidbody closestRigidbody = null;
            float minDistance = float.MaxValue;

            // ラグドールのRigidbodyのみを対象にする
            foreach (Rigidbody rb in rigidbodies)
            {
                float dist = Vector3.Distance(rb.position, hitPoint);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestRigidbody = rb;
                }
            }

            if (closestRigidbody != null)
            {
                closestRigidbody.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
                Debug.Log($"Applied force {force} at {hitPoint} to {closestRigidbody.name}");
            }
        }
    }
}