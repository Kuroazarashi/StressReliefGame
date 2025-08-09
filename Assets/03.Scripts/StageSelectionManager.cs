using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class StageSelectionManager : MonoBehaviour
{
    [Header("Stage Settings")]
    public StageSettings stageSettings;
    public Button[] stageButtons;
    public GameObject[] lockIcons;
    public Image[] stageIcons; // ★ステージアイコン用のImageコンポーネントを追加

    private int lastClearedStageIndex = 0;

    void Start()
    {
        // PlayerPrefsから最後にクリアしたステージの情報を読み込む
        // "ClearedStage"キーが存在しない場合は0を返す（ステージ1は最初から解放されているため）
        lastClearedStageIndex = PlayerPrefs.GetInt("ClearedStage", 0);

        UpdateStageButtons();
    }

    void UpdateStageButtons()
    {
        for (int i = 0; i < stageButtons.Length; i++)
        {
            // 現在のステージインデックスが、クリアしたステージインデックス以下であれば解放
            bool isUnlocked = (i <= lastClearedStageIndex);

            // ボタンの有効/無効を切り替える
            stageButtons[i].interactable = isUnlocked;

            // ロックアイコンとステージアイコンの表示/非表示を切り替える
            if (lockIcons.Length > i && lockIcons[i] != null)
            {
                lockIcons[i].SetActive(!isUnlocked);
            }
            if (stageIcons.Length > i && stageIcons[i] != null)
            {
                stageIcons[i].gameObject.SetActive(isUnlocked);
            }
        }
    }

    public void OnStageSelected(int stageIndex)
    {
        // 選択されたステージのインデックスが、解放済みのステージインデックス以下であることを確認
        if (stageIndex <= lastClearedStageIndex)
        {
            if (stageSettings != null && stageSettings.stages.Count > stageIndex)
            {
                // GameManagerのインスタンスがあれば、ステージインデックスをセット
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetCurrentStage(stageIndex);
                }

                // 対応するシーンをロードする
                SceneManager.LoadScene(stageSettings.stages[stageIndex].sceneName);
            }
        }
        else
        {
            Debug.Log("このステージはまだ解放されていません。");
        }
    }
}