using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragObject : MonoBehaviour
{

    private Vector3 mOffset;
    private float mZCoord;
    private GameObject[] terminals;
    private GameObject closestTerminal;

    private void Start()
    {
        if (terminals == null)
        {
            terminals = GameObject.FindGameObjectsWithTag("Terminal");
        }
    }

    void OnMouseDown()
    {
        Debug.Log("OnMouseDown");
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;

        // Store offset = gameobject world pos - mouse world pos
        mOffset = gameObject.transform.position - GetMouseAsWorldPoint();
    }

    private Vector3 GetMouseAsWorldPoint()
    {
        // Pixel coordinates of mouse (x,y)
        Vector3 mousePoint = Input.mousePosition;

        // z coordinate of game object on screen
        mousePoint.z = mZCoord;

        // Convert it to world points
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void OnMouseDrag()
    {
        //Debug.Log("OnMouseDrag");
        transform.position = GetMouseAsWorldPoint() + mOffset;
        if (terminals.Length > 0) 
        {
            GameObject closestTerminal = null;
            foreach (GameObject terminal in terminals) 
            {
                float distance = Vector3.Distance(transform.position, terminal.transform.position);
                if (distance > 4) 
                {
                    continue;
                }

                if (closestTerminal == null) 
                {
                    closestTerminal = terminal;
                    continue;
                }

                float distanceToClosest = Vector3.Distance(transform.position, closestTerminal.transform.position);
                if (distance < distanceToClosest) 
                {
                    closestTerminal = terminal;
                }
            }

            if (closestTerminal != null) 
            {
                if (!Object.ReferenceEquals(this.closestTerminal, closestTerminal)) 
                {
                    closestTerminal.GetComponent<SpriteRenderer>().color = Color.blue;
                    if (this.closestTerminal != null) 
                    {
                        this.closestTerminal.GetComponent<SpriteRenderer>().color = Color.white;
                    }
                    this.closestTerminal = closestTerminal;
                }

            } else 
            {
                if (this.closestTerminal != null)
                {
                    this.closestTerminal.GetComponent<SpriteRenderer>().color = Color.white;
                    this.closestTerminal = null;
                }
            }
        }
    }

    private void OnMouseUp()
    {
        Debug.Log("OnMouseUp");
        if (this.closestTerminal != null) 
        {
            this.closestTerminal.GetComponent<SpriteRenderer>().color = Color.black;
            Destroy(gameObject);
        }

    }
}