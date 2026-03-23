using System.Text;
using TMPro;
using UnityEngine;

public class SheepUI : MonoBehaviour
{
    public TMP_Text sheepText;

    void Update()
    {
        if (DecalStats.Instance == null || sheepText == null) return;

        var sheepList = DecalStats.Instance.GetAllSheep();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>Sheep Status:</b>\n");

        foreach (Sheep sheep in sheepList)
        {
            if (sheep == null) continue;

            string colorName = sheep.GetColorName();
            string coloredText = colorName;

            // TextMeshPro color formatting
            if (colorName == "Red") coloredText = "<color=red>Red</color>";
            else if (colorName == "Blue") coloredText = "<color=blue>Blue</color>";
            else if (colorName == "Green") coloredText = "<color=green>Green</color>";
            else coloredText = "None";

            string nameText = sheep.mustBeRed ? $"<b>{sheep.sheepName}</b>" : sheep.sheepName;
            sb.AppendLine($"{nameText} → {coloredText}");
        }

        sheepText.text = sb.ToString();
    }
}