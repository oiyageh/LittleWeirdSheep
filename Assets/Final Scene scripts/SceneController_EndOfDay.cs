using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SceneController_EndOfDay : MonoBehaviour
{
    [Header("Parade Settings")]
    public GameObject paradeSheepPrefab; // The prefab that walks
    public Transform startPoint;
    public Transform endPoint;
    public float timeBetweenSheep = 2f;

    [Header("UI Checklist")]
    public Transform checkboxListParent;
    public GameObject checkboxPrefab;

    [Header("Results UI")]
    public GameObject resultsPanel;
    public TextMeshProUGUI resultsText;

    private List<Toggle> toggles = new List<Toggle>();

    void Start()
    {
        // Force the cursor to be visible so you can use the checklist
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (GameManager.Instance != null && GameManager.Instance.allSheepData.Count > 0)
        {
            BuildCheckboxList();
            StartCoroutine(ParadeRoutine());
        }
    }

    void BuildCheckboxList()
    {
        foreach (SheepSaveData data in GameManager.Instance.allSheepData)
        {
            GameObject newBox = Instantiate(checkboxPrefab, checkboxListParent);
            // This finds the text child inside your toggle and sets the sheep's name
            newBox.GetComponentInChildren<TextMeshProUGUI>().text = data.name;
            toggles.Add(newBox.GetComponent<Toggle>());
        }
    }

    IEnumerator ParadeRoutine()
    {
        foreach (SheepSaveData data in GameManager.Instance.allSheepData)
        {
            GameObject walkingSheep = Instantiate(paradeSheepPrefab, startPoint.position, Quaternion.identity);

            // This line tells the sheep WHERE to go and WHO it is (Good or Bad)
            SheepParadeWalker walker = walkingSheep.GetComponent<SheepParadeWalker>();
            if (walker != null)
            {
                walker.Setup(endPoint.position, data);
            }

            yield return new WaitForSeconds(timeBetweenSheep);
        }
    }

    public void ScoreResults()
    {
        int startingGoodSheep = 0;
        int missedBadSheep = 0;

        foreach (var data in GameManager.Instance.allSheepData)
        {
            if (!data.isBad) startingGoodSheep++;
        }

        for (int i = 0; i < GameManager.Instance.allSheepData.Count; i++)
        {
            SheepSaveData data = GameManager.Instance.allSheepData[i];
            bool playerMarkedAsBad = toggles[i].isOn;

            if (data.isBad && !playerMarkedAsBad)
            {
                missedBadSheep++;
            }
        }

        int finalGoodCount = startingGoodSheep - (missedBadSheep * 2);
        if (finalGoodCount < 0) finalGoodCount = 0;

        resultsPanel.SetActive(true);
        if (finalGoodCount >= 13)
        {
            resultsText.text = "VICTORY!\nGood Sheep Remaining: " + finalGoodCount;
            resultsText.color = Color.green;
        }
        else
        {
            resultsText.text = "GAME OVER\nGood Sheep Remaining: " + finalGoodCount;
            resultsText.color = Color.red;
        }
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}