using UnityEngine;
using System.Collections.Generic;

public class ImagePasteTool2 : MonoBehaviour
{
    [Header("Decal Settings")]
    public GameObject decalPrefab;
    public float range = 50f;
    public Color color1 = Color.red;
    public Color color2 = Color.blue;
    public Color color3 = Color.green;
    public string allowedTag = "Paintable";

    private int currentColorIndex = 0;
    private Dictionary<GameObject, (GameObject decal, int colorIndex)> objectToDecal
        = new Dictionary<GameObject, (GameObject, int)>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) Shoot();
        if (Input.GetKeyDown(KeyCode.C)) SwapColor();
    }

    void SwapColor()
    {
        currentColorIndex++;
        if (currentColorIndex > 2) currentColorIndex = 0;
        Debug.Log("Switched to color index: " + currentColorIndex);
    }

    Color GetCurrentColor()
    {
        switch (currentColorIndex)
        {
            case 0: return color1;
            case 1: return color2;
            case 2: return color3;
            default: return color1;
        }
    }

    void Shoot()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, range)) return;
        if (!hit.collider.CompareTag(allowedTag)) return;

        GameObject targetObject = hit.collider.gameObject;

        // Only allow ONE decal per object
        if (objectToDecal.ContainsKey(targetObject))
        {
            Debug.Log("Object already has a decal!");
            return;
        }

        // Place decal
        GameObject newDecal = Instantiate(
            decalPrefab,
            hit.point + hit.normal * 0.01f,
            Quaternion.LookRotation(hit.normal)
        );
        newDecal.transform.SetParent(targetObject.transform);

        Renderer r = newDecal.GetComponent<Renderer>();
        if (r != null) r.material.color = GetCurrentColor();

        objectToDecal.Add(targetObject, (newDecal, currentColorIndex));

        // Update sheep if it exists
        Sheep sheep = targetObject.GetComponent<Sheep>();
        if (sheep != null) sheep.SetColor(currentColorIndex);
    }
}