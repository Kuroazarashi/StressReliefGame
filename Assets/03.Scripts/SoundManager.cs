using System.Collections.Generic;
using UnityEngine;

// SoundManagerがサウンドの種類とクリップを紐づけるための設定クラス
[System.Serializable]
public class SoundTypeMapping
{
    public string soundType; // "Metal", "Wood"などのサウンドの種類名
    public AudioClip soundClip;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Sound Library")]
    [Tooltip("サウンドの種類とオーディオクリップを紐づけるリスト")]
    [SerializeField] private List<SoundTypeMapping> soundMappings;
    private Dictionary<string, AudioClip> soundDictionary;

    [Header("Default Sound Settings")]
    [SerializeField] private AudioClip defaultHitSound;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // リストを辞書に変換して、高速にアクセスできるようにする
        soundDictionary = new Dictionary<string, AudioClip>();
        foreach (var mapping in soundMappings)
        {
            if (!soundDictionary.ContainsKey(mapping.soundType))
            {
                soundDictionary.Add(mapping.soundType, mapping.soundClip);
            }
        }
    }

    public void PlayHitSound(GameObject hitObject)
    {
        AudioClip clipToPlay = defaultHitSound; // デフォルトの音を初期値に

        // ヒットしたオブジェクトがDestructibleコンポーネントを持っているか確認
        Destructible destructible = hitObject.GetComponent<Destructible>();
        if (destructible != null)
        {
            // soundTypeをキーにして、辞書から対応するサウンドを探す
            if (soundDictionary.ContainsKey(destructible.soundType))
            {
                clipToPlay = soundDictionary[destructible.soundType];
            }
        }

        // 見つかったサウンドを再生
        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
    }
}
