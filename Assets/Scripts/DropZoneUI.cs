using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZoneUI : MonoBehaviour, IDropHandler
{
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDropZone");
        if (eventData.pointerDrag != null)
        {
            Destroy(eventData.pointerDrag);
            image.color = Color.black;
        }

    }

}
