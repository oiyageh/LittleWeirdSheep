using UnityEngine;

public class ImageColorSwap : MonoBehaviour
{
    public Texture2D sourceTexture;

    public Color colorToReplace = Color.white;
    public Color replacementColorA = Color.red;
    public Color replacementColorB = Color.blue;

    public bool useColorA = true;

    private Texture2D runtimeTexture;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();

        // Copy texture so original isn't modified
        runtimeTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
        runtimeTexture.SetPixels(sourceTexture.GetPixels());
        runtimeTexture.Apply();

        rend.material.mainTexture = runtimeTexture;

        SwapColors();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            useColorA = !useColorA;
            SwapColors();
        }
    }

    void SwapColors()
    {
        Color[] pixels = sourceTexture.GetPixels();

        Color newColor = useColorA ? replacementColorA : replacementColorB;

        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i] == colorToReplace)
            {
                pixels[i] = newColor;
            }
        }

        runtimeTexture.SetPixels(pixels);
        runtimeTexture.Apply();
    }
}
