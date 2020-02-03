using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour
{
    public const string DRAGGABLE_TAG = "UIDraggable";

    private Transform objectToDrag;
    private bool dragging = false;

    List<RaycastResult> hitObjects = new List<RaycastResult>();


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Down");
            objectToDrag = GetDraggableTransformUnderMouse();

            if (objectToDrag != null)
            {
                dragging = true;
            }
        }
        if (dragging)
        {
            Debug.Log("Dragging");
            objectToDrag.position = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0)) 
        {
            Debug.Log("Up");
            dragging = false;
            objectToDrag = null;
        }
    }

    private GameObject GetObjectUnderMouse()
    {
        var pointer = new PointerEventData(EventSystem.current);

        pointer.position = Input.mousePosition;

        EventSystem.current.RaycastAll(pointer, hitObjects);

        if (hitObjects.Count <= 0) return null;

        return hitObjects.First().gameObject;
    }

    private Transform GetDraggableTransformUnderMouse()
    {
        var clickedObject = GetObjectUnderMouse();
        Debug.Log("Clicked Object: " + ((clickedObject==null)?"NULL":"not null"));

        // get top level object hit
        if (clickedObject != null && clickedObject.tag == DRAGGABLE_TAG)
        {
            return clickedObject.transform;
        }

        return null;
    }
}
