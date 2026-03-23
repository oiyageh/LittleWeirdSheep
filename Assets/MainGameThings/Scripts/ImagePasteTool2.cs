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
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            SwapColor();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveLookedAtDecal();
        }
    }

    void SwapColor()
    {
        currentColorIndex++;
        if (currentColorIndex > 2)
            currentColorIndex = 0;

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
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            if (!hit.collider.CompareTag(allowedTag)) return;
            GameObject targetObject = hit.collider.gameObject;

            // Remove old decal if it exists
            if (objectToDecal.ContainsKey(targetObject))
            {
                var oldData = objectToDecal[targetObject];
                DecalStats.Instance.RemoveColor(oldData.colorIndex);
                Destroy(oldData.decal);
                objectToDecal.Remove(targetObject);
            }

            // Place new decal
            GameObject newDecal = Instantiate(
                decalPrefab,
                hit.point + hit.normal * 0.01f,
                Quaternion.LookRotation(hit.normal)
            );
            newDecal.transform.SetParent(targetObject.transform);

            Renderer r = newDecal.GetComponent<Renderer>();
            if (r != null) r.material.color = GetCurrentColor();

            objectToDecal.Add(targetObject, (newDecal, currentColorIndex));
            DecalStats.Instance.AddColor(currentColorIndex);

            // Update sheep
            Sheep sheep = targetObject.GetComponent<Sheep>();
            if (sheep != null) sheep.SetColor(currentColorIndex);
        }
    }

    void RemoveLookedAtDecal()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            GameObject objectToRemove = null;

            foreach (var pair in objectToDecal)
            {
                if (pair.Value.decal == hit.collider.gameObject)
                {
                    objectToRemove = pair.Key;

                    // Update color counts
                    DecalStats.Instance.RemoveColor(pair.Value.colorIndex);

                    // Update sheep
                    Sheep sheep = objectToRemove.GetComponent<Sheep>();
                    if (sheep != null) sheep.RemoveColor();

                    Destroy(pair.Value.decal);
                    break;
                }
            }

            if (objectToRemove != null)
            {
                objectToDecal.Remove(objectToRemove);
            }
        }
    }
}