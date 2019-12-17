using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed = 0.5f;
    public float zoomSpeed = 4.0f;
    public float focalMax = 25.0f;
    public float focalMin = 15.0f;
    public int scrollMargin = 30;
    public float minX = 0;
    public float maxX = 100f;
    public float minZ = 0;
    public float maxZ = 100f;

    private float zoomGoal = 0;
    private Vector3 posDir = Vector3.zero;

    /**
    * This method sets the zoom goal to the camera focal length at the start
    * 
    * @param localPosition
    */
    void Start()
    {
        zoomGoal = Camera.main.focalLength;
    }

    // Update is called once per frame
    /**
    * This method updates every frame and changes the camera position based on where the user decides using their mouse position
    * 
    * @param 
    */
    void Update()
    {
        if (Input.mouseScrollDelta.magnitude > 0.0001f)
        {
            zoomGoal = Mathf.Clamp(Camera.main.focalLength + Input.mouseScrollDelta.y, focalMin, focalMax);
        }


        if(Mathf.Abs(zoomGoal - Camera.main.focalLength) > 0.01)
        {
            Camera.main.focalLength = Mathf.Lerp(Camera.main.focalLength, zoomGoal, zoomSpeed * Time.deltaTime);
        }

        var camPosX = Camera.main.transform.position.x;
        var camPosZ = Camera.main.transform.position.z;

        posDir = Vector3.zero;
        if(camPosX < maxX && Input.mousePosition.x > Camera.main.pixelWidth -  scrollMargin)
        {
            var diff = Input.mousePosition.x - Camera.main.pixelWidth + scrollMargin;
            posDir += new Vector3(moveSpeed * Time.deltaTime * diff/scrollMargin, 0, 0);
        }
        else if(camPosX > minX && Input.mousePosition.x < scrollMargin)
        {
            var diff = scrollMargin - Input.mousePosition.x;
            posDir -= new Vector3(moveSpeed * Time.deltaTime * diff / scrollMargin, 0, 0);
        }

        if (camPosZ < maxZ && Input.mousePosition.y > Camera.main.pixelHeight - scrollMargin)
        {
            var diff = Input.mousePosition.y - Camera.main.pixelHeight + scrollMargin;
            posDir += new Vector3(0, 0, moveSpeed * Time.deltaTime * diff/scrollMargin);
        }
        else if (camPosZ > minZ && Input.mousePosition.y < scrollMargin)
        {
            var diff = scrollMargin - Input.mousePosition.y;
            posDir -= new Vector3(0, 0, moveSpeed * Time.deltaTime * diff / scrollMargin);
        }

        Camera.main.transform.position += posDir;
    }
}