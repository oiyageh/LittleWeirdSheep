using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    private Animator animator;
    private bool isSwinging = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Detect left mouse click to swing
        if (Input.GetButtonDown("Fire1") && !isSwinging)
        {
            StartCoroutine(SwingAction());
        }
    }

    System.Collections.IEnumerator SwingAction()
    {
        isSwinging = true;
        animator.SetTrigger("Swing"); 

        // Wait for the duration of the animation (e.g., 0.5 seconds)
        yield return new WaitForSeconds(0.5f);

        isSwinging = false;
    }
}
