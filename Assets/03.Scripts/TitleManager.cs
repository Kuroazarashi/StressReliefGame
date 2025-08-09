using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public void StartGame()
    {
        Debug.Log("Play button clicked!");
        SceneManager.LoadScene("02.StageSelectScene");
    }
}