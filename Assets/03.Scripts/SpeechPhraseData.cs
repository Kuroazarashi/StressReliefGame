using UnityEngine;

// Unity EditorのInspectorでこのクラスのデータを表示・編集できるようにするために必要
[System.Serializable]
public class SpeechPhraseData
{
    public string phrase;       // 吹き出しに表示するセリフのテキスト
    public AudioClip voiceClip; // そのセリフに対応するボイスのAudioClip（音声ファイル）
}