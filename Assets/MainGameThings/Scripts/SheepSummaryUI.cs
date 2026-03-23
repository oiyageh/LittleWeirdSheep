using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.SceneManagement;

public class SheepSummaryUI : MonoBehaviour
{
    public TMP_Text summaryText;
    public string winSceneName = "WinScene";
    public string loseSceneName = "LoseScene";

    void Start()
    {
        ShowSheepSummary();
    }

    void ShowSheepSummary()
    {
        if (DecalStats.Instance == null || summaryText == null) return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>Sheep Summary:</b>\n");

        foreach (var sheep in DecalStats.Instance.GetAllSheep())
        {
            string colorName = sheep.colorIndex switch
            {
                0 => "<color=red>Red</color>",
                1 => "<color=blue>Blue</color>",
                2 => "<color=green>Green</color>",
                _ => "<color=grey>None</color>"
            };

            string displayName = sheep.mustBeRed
                ? $"<b>{sheep.sheepName}</b>"
                : sheep.sheepName;

            sb.AppendLine($"{displayName} → {colorName}");
        }

        summaryText.text = sb.ToString();
    }

    // Call this on Button Click
    public void OnCheckWinLose()
    {
        if (DecalStats.Instance == null)
        {
            Debug.LogError("DecalStats instance not found!");
            return;
        }

        foreach(var sheep in DecalStats.Instance.GetAllSheep())
{
            bool isCorrect =
                (sheep.mustBeRed && sheep.colorIndex == 0) ||
                (!sheep.mustBeRed && sheep.colorIndex == 2);

            if (!isCorrect)
            {
                SceneManager.LoadScene(loseSceneName);
                return;
            }
        }

        // If all checks passed
        SceneManager.LoadScene(winSceneName);
    }
}