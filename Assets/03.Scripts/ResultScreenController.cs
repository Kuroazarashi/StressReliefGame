using UnityEngine;

/// <summary>
/// リザルト画面のボタン機能を管理するクラス。
/// ボタンが押された時に、シングルトンのGameManagerを動的に見つけてメソッドを呼び出す。
/// </summary>
public class ResultScreenController : MonoBehaviour
{
    // このメソッドは、TitleButtonのOnClick()イベントから呼び出す
    public void OnClick_ReturnToTitle()
    {
        // GameManagerのインスタンスがnullでないことを確認してからメソッドを呼び出す
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToTitle();
        }
        else
        {
            Debug.LogError("GameManager.Instance not found!");
        }
    }

    // このメソッドは、StageSelectButtonのOnClick()イベントから呼び出す
    public void OnClick_ReturnToStageSelect()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToStageSelect();
        }
        else
        {
            Debug.LogError("GameManager.Instance not found!");
        }
    }

    // このメソッドは、RetryButtonのOnClick()イベントから呼び出す
    public void OnClick_RetryGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RetryGame();
        }
        else
        {
            Debug.LogError("GameManager.Instance not found!");
        }
    }

    // このメソッドは、NextStageButtonのOnClick()イベントから呼び出す
    public void OnClick_NextStage()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NextStage();
        }
        else
        {
            Debug.LogError("GameManager.Instance not found!");
        }
    }
}
