using UnityEngine;
using UnityEngine.UI;

public class SheepJudgeUI : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform listParent;
    public JudgmentSystem judgeSystem;

    void Start()
    {
        foreach (ISheepData enemy in SheepManager.Instance.stampedEnemies)
        {
            GameObject btn = Instantiate(buttonPrefab, listParent);
            btn.GetComponentInChildren<Text>().text = enemy.SheepName;
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                judgeSystem.JudgeEnemy(enemy);
            });
        }
    }
}