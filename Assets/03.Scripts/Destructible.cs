using UnityEngine;

/// <summary>
/// 破壊可能なオブジェクトに関する情報を保持するコンポーネント。
/// スコアやサウンドの種類などをオブジェクト自身に持たせる。
/// </summary>
public class Destructible : MonoBehaviour
{
    [Tooltip("このオブジェクトを破壊した際に加算されるスコア")]
    public int scoreValue = 10;

    [Tooltip("再生するヒット音の種類（SoundManagerの設定と一致させる文字列）")]
    public string soundType = "Default"; // 例: "Metal", "Wood", "Glass"
}
