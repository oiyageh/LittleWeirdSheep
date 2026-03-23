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
                _ => "None"
            };

            string displayName = sheep.MustBeRed() ? $"<b>{sheep.GetSheepName()}</b>" : sheep.GetSheepName();
            sb.AppendLine($"{displayName} → {coloredText}");
        }

        summaryText.text = sb.ToString();
    }

    // Call this with a button click
    public void OnCheckWinLose()
    {
        if (DecalStats.Instance == null) return;

        bool allCorrect = true;

        foreach (Sheep sheep in DecalStats.Instance.GetAllSheep())
        {
            if (sheep == null) continue;

            // Required sheep must be red
            if (sheep.MustBeRed() && sheep.currentColorIndex != 0)
                allCorrect = false;

            // Optional sheep must be green
            if (!sheep.MustBeRed() && sheep.currentColorIndex != 2)
                allCorrect = false;
        }

        if (allCorrect)
            SceneManager.LoadScene(winSceneName);
        else
            SceneManager.LoadScene(loseSceneName);
    }
}