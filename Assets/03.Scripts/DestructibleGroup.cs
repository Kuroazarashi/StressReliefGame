using UnityEngine;

// 複数のRigidbodyを持つオブジェクトを一つのグループとして扱い、
// いずれか一つが攻撃されたらグループ全体を破壊状態にするスクリプト
public class DestructibleGroup : MonoBehaviour
{
    // グループに所属するすべてのRigidbodyを持つパーツをここに登録する
    [SerializeField] private Rigidbody[] groupParts;

    private bool isDestroyed = false;

    // このメソッドが、外部（AttackColliderHandler）から呼び出される
    public void ShatterGroup()
    {
        // すでに破壊処理が実行済みの場合は、何もしない
        if (isDestroyed)
        {
            return;
        }

        // 破壊フラグを立てて、二重実行を防ぐ
        isDestroyed = true;
        
        Debug.Log(gameObject.name + " group is shattering!");

        // 登録されているすべてのパーツに対してループ処理を行う
        foreach (Rigidbody part in groupParts)
        {
            if (part != null)
            {
                // 各パーツのIs Kinematicのチェックを外し、物理演算を開始させる
                part.isKinematic = false;
            }
        }
    }
}
