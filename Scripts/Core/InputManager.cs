using UnityEngine;

public class InputManager : MonoBehaviour
{
    Camera cam;
    bool isDragging;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            DigAtMouse();
        }
    }

    void DigAtMouse()
    {
        // Disabled direct digging — let the ants do the work!
        // (DigCommandManager handles assigning ants to clicked tiles)
        
        /*
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null)
            {
                tile.Dig();
            }
        }
        */
    }
}