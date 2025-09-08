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
    public Image[] stageIcons;

    private int lastClearedStageIndex = 0;

    void Start()
    {
        lastClearedStageIndex = PlayerPrefs.GetInt("ClearedStage", 0);
        UpdateStageButtons();
    }

    void UpdateStageButtons()
    {
        for (int i = 0; i < stageButtons.Length; i++)
        {
            // ステージが解放されているかどうかの判定
            bool isUnlocked = (i <= lastClearedStageIndex);

            // ボタンのインタラクティブな状態を設定
            if (stageButtons[i] != null)
            {
                stageButtons[i].interactable = isUnlocked;
            }

            // ロックアイコンとステージアイコンの表示・非表示を切り替える
            if (lockIcons.Length > i && lockIcons[i] != null)
            {
                lockIcons[i].SetActive(!isUnlocked);
            }

            if (stageIcons.Length > i && stageIcons[i] != null)
            {
                // ロックされたステージではアイコンを非表示にする
                stageIcons[i].gameObject.SetActive(isUnlocked);
            }
        }
    }

    public void OnStageSelected(int stageIndex)
    {
        // ステージが解放されている場合のみシーン遷移
        if (stageIndex <= lastClearedStageIndex)
        {
            if (stageSettings != null && stageSettings.stages.Count > stageIndex)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetCurrentStage(stageIndex);
                }
                SceneManager.LoadScene(stageSettings.stages[stageIndex].sceneName);
            }
        }
        else
        {
            Debug.Log("このステージはまだ解放されていません。");
        }
    }

    public void ReturnToTitle()
    {
        SceneManager.LoadScene("00.TitleScene");
    }
}
