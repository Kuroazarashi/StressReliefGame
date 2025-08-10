using UnityEngine;
using System.Linq;

public class EnemyRagdollController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody[] rigidbodies;
    private Collider[] colliders;
    private Collider mainCollider;

    // ▼▼▼▼▼ ここから下が追加箇所 ▼▼▼▼▼
    private EnemyStressVisuals enemyStressVisuals; // EnemyStressVisualsへの参照
    // ▲▲▲▲▲ ここまでが追加箇所 ▲▲▲▲▲

    void Awake()
    {
        animator = GetComponent<Animator>();
        rigidbodies = GetComponentsInChildren<Rigidbody>().Where(rb => rb.gameObject != gameObject).ToArray();
        colliders = GetComponentsInChildren<Collider>().Where(col => col.gameObject != gameObject).ToArray();
        mainCollider = GetComponent<Collider>();

        // ▼▼▼▼▼ ここから下が追加箇所 ▼▼▼▼▼
        // 同じゲームオブジェクトにアタッチされているEnemyStressVisualsを取得
        enemyStressVisuals = GetComponent<EnemyStressVisuals>();
        // ▲▲▲▲▲ ここまでが追加箇所 ▲▲▲▲▲

        SetRagdollState(false);
    }

    public void SetRagdollState(bool enableRagdoll)
    {
        if (animator != null)
        {
            animator.enabled = !enableRagdoll;
        }

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = !enableRagdoll;
        }

        foreach (Collider col in colliders)
        {
            col.enabled = enableRagdoll;
        }

        if (mainCollider != null)
        {
            mainCollider.enabled = true;
        }

        // ▼▼▼▼▼ ここから下が追加箇所 ▼▼▼▼▼
        // ラグドールが有効になった（＝倒された）場合
        if (enableRagdoll)
        {
            // EnemyStressVisualsコンポーネントがあれば、その停止メソッドを呼び出す
            if (enemyStressVisuals != null)
            {
                enemyStressVisuals.StopAllVisuals();
            }
        }
        // ▲▲▲▲▲ ここまでが追加箇所 ▲▲▲▲▲

        Debug.Log($"Ragdoll State set to: {enableRagdoll}. Animator Enabled: {animator.enabled}");
    }

    // ...（ApplyForceメソッドは変更なし）...
    public void ApplyForce(Vector3 force, Vector3 hitPoint)
    {
        if (animator != null && !animator.enabled)
        {
            Rigidbody closestRigidbody = null;
            float minDistance = float.MaxValue;

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
