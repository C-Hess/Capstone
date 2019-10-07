using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpWirecutters : MonoBehaviour
{
    private Vector3 mOffset;

    private float mZCoord; 
    void OnMouseDown()
    {
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;

        mOffset = gameObject.transform.position - GetMouseWorldPos();

    }
    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;

        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
    private void OnMouseDrag()
    {
        transform.position = GetMouseWorldPos() + mOffset; 
    }
}
