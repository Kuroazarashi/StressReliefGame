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

    // ▼▼▼ ここを修正しました ▼▼▼
    /// <summary>
    /// ステージ選択ボタンから呼び出されるメソッド。
    /// 引数にはステージのインデックス（Stage1なら0, Stage2なら1...）を渡してください。
    /// </summary>
    /// <param name="stageIndex">選択されたステージのインデックス（0始まり）</param>
    public void SelectStage(int stageIndex)
    {
        // GameManagerには、受け取ったインデックスをそのまま渡します。
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentStage(stageIndex);
        }

        // シーン名はインデックスに1を足して生成します (index 0 -> Stage1)
        int stageNumber = stageIndex + 1;
        SceneManager.LoadScene("010.Stage" + stageNumber);
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲
}

