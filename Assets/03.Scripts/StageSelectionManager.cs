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
            bool isUnlocked = (i <= lastClearedStageIndex);
            stageButtons[i].interactable = isUnlocked;
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

    // ▼▼▼▼▼ このメソッドを追加 ▼▼▼▼▼
    /// <summary>
    /// タイトル画面(00.TitleScene)に遷移するメソッド
    /// </summary>
    public void ReturnToTitle()
    {
        SceneManager.LoadScene("00.TitleScene");
    }
    // ▲▲▲▲▲ このメソッドを追加 ▲▲▲▲▲
}
