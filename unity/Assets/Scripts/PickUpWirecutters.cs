using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpWirecutters : MonoBehaviour
{
    private Vector3 mOffset;

    private float mZCoord;
    /**
    * This method  checks to see if the mouse is down, if it is, it allows you to drag the object
    * 
    * @param 
    */
    void OnMouseDown()
    {
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;

        mOffset = gameObject.transform.position - GetMouseWorldPos();

    }
    /**
    * This method checks the world position of the mouse
    * 
    * @param 
    */
    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;

        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
    /**
        * This method is called when the object should be moved 
        * 
        * @param 
        */
    private void OnMouseDrag()
    {
        transform.position = GetMouseWorldPos() + mOffset; 
    }
}
