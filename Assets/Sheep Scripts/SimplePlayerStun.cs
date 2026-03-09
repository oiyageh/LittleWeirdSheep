using UnityEngine;

public class SimplePlayerStun : MonoBehaviour
{
    public bool isStunned = false;
    public float stunTimer = 0f;

    void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer <= 0f)
            {
                stunTimer = 0f;
                isStunned = false;
            }
        }
    }

    public void ApplyStun(float duration)
    {
        isStunned = true;

        // refresh if new stun is longer
        if (duration > stunTimer)
        {
            stunTimer = duration;
        }
    }
}