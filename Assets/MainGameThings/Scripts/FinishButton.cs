using UnityEngine;

public class FinishButton : MonoBehaviour
{
    public string winSceneName = "WinScene";
    public string loseSceneName = "LoseScene";

    public void OnFinishClicked()
    {
        if (DecalStats.Instance != null)
        {
            DecalStats.Instance.EvaluateWinLose(winSceneName, loseSceneName);
        }
    }
}