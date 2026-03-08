using UnityEngine;

public class ImagePasteTool : MonoBehaviour
{
    public GameObject decalPrefab;
    
    public float range = 50f;

    public Color color1 = Color.red;
    public Color color2 = Color.blue;

    public string allowedTag = "Paintable";

    private bool useFirstColor = true;

    void Update()
    {
        // Left click = paste image
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
            Debug.Log("Pasted Image");
        }

        // Press C to swap color
        if (Input.GetKeyDown(KeyCode.C))
        {
            useFirstColor = !useFirstColor;
            Debug.Log("Swapped Color!");
        }
    }

    void Shoot()
    {
    
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {

            //check tag
            if (!hit.collider.CompareTag(allowedTag))
                return;
            GameObject decal = Instantiate(decalPrefab, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));

            // attach to object hit
            decal.transform.SetParent(hit.collider.transform);

            // set color
            Renderer r = decal.GetComponent<Renderer>();
            if (r != null)
            {
                Debug.Log("Missing!!");
                r.material.color = useFirstColor ? color1 : color2;
            }
        }
    }
}