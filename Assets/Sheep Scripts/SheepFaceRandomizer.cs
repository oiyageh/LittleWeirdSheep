using UnityEngine;
using UnityEngine.UI;
public class SheepFaceRandomizer : MonoBehaviour
{
    public Image faceImage;                // assign your FaceImage UI element
    public Sprite[] faceSprites;           // assign all your face images
    public float changeInterval = 1.5f;    // how fast to randomize
    float timer;
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = changeInterval;
            if (faceSprites.Length > 0)
            {
                faceImage.sprite = faceSprites[Random.Range(0, faceSprites.Length)];
            }
        }
    }
}