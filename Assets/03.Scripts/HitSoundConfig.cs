// HitSoundConfig.cs
using UnityEngine;
using System; // [Serializable]のために必要

[Serializable] // Inspectorで表示できるようにするために必要
public class HitSoundConfig
{
    public GameObject targetObject; // ヒット対象のオブジェクト（プレハブでもシーン内のインスタンスでも可）
    public AudioClip hitSoundClip; // そのオブジェクトがヒットした時に再生するサウンド
}