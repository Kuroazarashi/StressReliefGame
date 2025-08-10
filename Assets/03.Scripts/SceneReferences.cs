using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SceneReferences : MonoBehaviour
{
    [Header("Game UI References")]
    public GameObject gameUI;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    [Header("Result UI References")]
    public GameObject resultUI;
    public TextMeshProUGUI resultScoreText;
    public TextMeshProUGUI resultMessageText;
    public GameObject nextStageButton;

    [Header("Character References")]
    public EnemyRagdollController enemyRagdollController;

    // Sound Referencesの項目は不要になったため削除しました
}
