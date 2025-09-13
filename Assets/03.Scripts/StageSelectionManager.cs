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
        int clearedStageIndex = PlayerPrefs.GetInt("ClearedStage", 0);

        for (int i = 0; i < stageButtons.Length; i++)
        {
            // i はボタンのインデックス (0=Stage1, 1=Stage2...)
            // clearedStageIndex はクリア済みの最大インデックス+1 (Stage1クリア後は1になる)
            if (i <= clearedStageIndex)
            {
                // ▼▼▼ アンロック時の処理 ▼▼▼
                stageButtons[i].interactable = true;

                // Lockオブジェクトを非表示にする
                Transform lockImage = stageButtons[i].transform.Find("Lock");
                if (lockImage != null)
                {
                    lockImage.gameObject.SetActive(false);
                }

                // 【修正点】Iconオブジェクトを表示する処理を追加
                Transform iconImage = stageButtons[i].transform.Find("Icon");
                if (iconImage != null)
                {
                    iconImage.gameObject.SetActive(true);
                }
            }
            else
            {
                // ▼▼▼ ロック時の処理 ▼▼▼
                stageButtons[i].interactable = false;

                // Lockオブジェクトを表示する
                Transform lockImage = stageButtons[i].transform.Find("Lock");
                if (lockImage != null)
                {
                    lockImage.gameObject.SetActive(true);
                }

                // Iconオブジェクトを非表示にする
                Transform iconImage = stageButtons[i].transform.Find("Icon");
                if (iconImage != null)
                {
                    iconImage.gameObject.SetActive(false);
                }
            }
        }
    }

    public void SelectStage(int stageIndex)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentStage(stageIndex);
        }

        // 【修正点】新しいシーン命名規則に対応したシーン名を生成
        // Stage1 (index 0) -> "010.Stage1"
        // Stage2 (index 1) -> "011.Stage2"
        string sceneToLoad = "01" + stageIndex + ".Stage" + (stageIndex + 1);

        Debug.Log("Attempting to load scene: " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
}
