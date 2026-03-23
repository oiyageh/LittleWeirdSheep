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

        foreach (Sheep sheep in DecalStats.Instance.GetAllSheep())
        {
            if (sheep == null) continue;

            string colorName = sheep.GetColorName();
            string coloredText = colorName switch
            {
                "Red" => "<color=red>Red</color>",
                "Blue" => "<color=blue>Blue</color>",
                "Green" => "<color=green>Green</color>",
                _ => "<color=grey>None</color>" // unpainted sheep
            };

            string displayName = sheep.MustBeRed() ? $"<b>{sheep.GetSheepName()}</b>" : sheep.GetSheepName();
            sb.AppendLine($"{displayName} → {coloredText}");
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

        foreach (Sheep sheep in DecalStats.Instance.GetAllSheep())
        {
            if (sheep == null) continue;

            // Check required red sheep
            if (sheep.MustBeRed())
            {
                if (sheep.currentColorIndex != 0) // not red or unpainted
                {
                    SceneManager.LoadScene(loseSceneName);
                    return;
                }
            }
            else // optional sheep must be green
            {
                if (sheep.currentColorIndex != 2) // not green or unpainted
                {
                    SceneManager.LoadScene(loseSceneName);
                    return;
                }
            }
        }

        // If all checks passed
        SceneManager.LoadScene(winSceneName);
    }
}