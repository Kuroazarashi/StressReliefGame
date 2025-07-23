// SoundManager.cs
using System.Collections.Generic; // List<T>のために必要
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; } // シングルトンインスタンス

    [Header("Hit Sounds Configuration")]
    [SerializeField] private List<HitSoundConfig> hitSoundConfigs = new List<HitSoundConfig>(); // オブジェクトとサウンドの紐付けリスト

    [Header("Default Sound Settings")]
    [SerializeField] private AudioClip defaultHitSound; // 紐付けがない場合のデフォルトサウンド

    private AudioSource audioSource; // サウンド再生用のAudioSource

    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいで存在させる
        }
        else
        {
            Destroy(gameObject); // 既にインスタンスがあれば自身を破棄
            return;
        }

        // AudioSourceコンポーネントを取得。なければ追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = false; // エフェクト音のためループ再生はしない設定に
        audioSource.playOnAwake = false; // シーン開始時に自動再生しない
    }

    // オブジェクトのタグに基づいてヒットサウンドを取得して再生するメソッド
    public void PlayHitSound(GameObject hitObject)
    {
        AudioClip clipToPlay = null;

        // ヒットしたオブジェクトのタグと一致する設定を探す
        // より厳密にするなら、tagではなくGameObjectそのものや特定のコンポーネントで判断することも可能
        foreach (var config in hitSoundConfigs)
        {
            // ここではGameObjectのReferenceEqualityComparer（インスタンスの一致）を使用
            // または、タグで比較する (config.targetObject.CompareTag(hitObject.tag))
            // もしくは、config.targetObject.name == hitObject.name とか
            // ヒットしたオブジェクトが設定リストのGameObjectそのもの、またはそのプレハブインスタンスであるかをチェック
            // ただし、シーン内に配置されているオブジェクト（インスタンス）とInspectorで設定したプレハブを
            // 正しく比較するには、それぞれの名前やタグを使用する方が確実なことが多い。
            // ここでは簡易的にNameで比較してみます。より厳密にはTagの利用を推奨。
            if (config.targetObject != null && hitObject.name.Contains(config.targetObject.name)) // 名前に含まれるかで簡易比較
            {
                clipToPlay = config.hitSoundClip;
                break;
            }
        }

        // ヒットしたオブジェクトの特定のコンポーネントを持っているかで判断する、というのもアリ
        // 例：DestructibleObjectコンポーネントがある場合はその情報を優先、なければデフォルト
        // 今回は、SoundManagerで一元管理するため、DestructibleObjectは不要になる方向で進めます。

        if (clipToPlay == null) // 設定が見つからない場合、またはサウンドが設定されていない場合
        {
            clipToPlay = defaultHitSound; // デフォルトサウンドを再生
        }

        // サウンドを再生
        if (clipToPlay != null && audioSource != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
        else
        {
            Debug.LogWarning("No hit sound clip found for " + hitObject.name + " and no default hit sound is set.", this);
        }
    }
}