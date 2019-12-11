using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{


    // Update is called once per frame
    /**
        * This method is called, to rotate the object, signifying you can pick it up,  was never implemented in the game
        * 
        * @param 
        */
    void Update()
    {
        transform.Rotate(new Vector3(30, 0, 0) * Time.deltaTime * 2.5f);
    }
}
