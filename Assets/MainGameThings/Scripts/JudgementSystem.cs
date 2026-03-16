using UnityEngine;

public class JudgmentSystem : MonoBehaviour
{
    public int score;
    public int penalty = 10;

    public void JudgeEnemy(ISheepData enemy)
    {
        if (enemy.stampType == StampType.Hostile)
        {
            score += 10; // Correct
        }
        else
        {
            score -= penalty; // Wrong
        }

        Debug.Log(enemy.SheepName + " judged. Score: " + score);
    }
}