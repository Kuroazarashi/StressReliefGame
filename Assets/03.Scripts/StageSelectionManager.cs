using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectionManager : MonoBehaviour
{
    public Button[] stageButtons;

    void Start()
    {
        UpdateStageButtons();
    }

    void UpdateStageButtons()
    {
        int lastClearedStagePlusOne = PlayerPrefs.GetInt("ClearedStage", 0);
        for (int i = 0; i < stageButtons.Length; i++)
        {
            if (i <= lastClearedStagePlusOne)
            {
                stageButtons[i].interactable = true;
                Transform lockedImage = stageButtons[i].transform.Find("Locked");
                if (lockedImage != null)
                {
                    lockedImage.gameObject.SetActive(false);
                }
            }
            else
            {
                stageButtons[i].interactable = false;
                Transform lockedImage = stageButtons[i].transform.Find("Locked");
                if (lockedImage != null)
                {
                    lockedImage.gameObject.SetActive(true);
                }
            }
        }
    }

    public void SelectStage(int stageNumber)
    {
        int stageIndex = stageNumber - 1;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentStage(stageIndex);
        }

        // ▼▼▼ ここを修正しました ▼▼▼
        // シーン名を実際のファイル名 "010.Stage" + 番号 に合わせます
        SceneManager.LoadScene("010.Stage" + stageNumber);
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲
    }
}

