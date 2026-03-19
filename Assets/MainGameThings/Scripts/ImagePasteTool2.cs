using UnityEngine;

public class ImagePasteTool2 : MonoBehaviour
{
    [Header("Decal Settings")]
    public GameObject decalPrefab;
    public float range = 50f;

    public Color color1 = Color.red;
    public Color color2 = Color.blue;
    public Color color3 = Color.green;

    public string allowedTag = "Paintable";

    private int currentColorIndex = 0; // 0, 1, 2
    private GameObject currentDecal;

    void Update()
    {
        // Left click = paste image
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }

        // Press C = cycle colors
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwapColor();
        }

        // Press R = remove decal
        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveDecal();
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
        // Only one decal at a time
        if (currentDecal != null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            if (!hit.collider.CompareTag(allowedTag))
                return;

            currentDecal = Instantiate(
                decalPrefab,
                hit.point + hit.normal * 0.01f,
                Quaternion.LookRotation(hit.normal)
            );

            currentDecal.transform.SetParent(hit.collider.transform);

            // Apply selected color
            Renderer r = currentDecal.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = GetCurrentColor();
            }

            // Save enemy data
            SheepDataComponent enemyComp = hit.collider.GetComponent<SheepDataComponent>();
            if (enemyComp != null && enemyComp.enemyData != null)
            {
                SheepManager.Instance.AddEnemy(enemyComp.enemyData);
                Debug.Log("Stamped and saved: " + enemyComp.enemyData.SheepName);
            }
        }
    }

    void RemoveDecal()
    {
        if (currentDecal != null)
        {
            Destroy(currentDecal);
            currentDecal = null;
            Debug.Log("Decal Removed");
        }
    }
}