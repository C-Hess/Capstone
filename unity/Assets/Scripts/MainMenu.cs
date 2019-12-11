using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    /**
        * This method invokes the play game, and sets the scene to the first level
        * 
        * @param 
        */
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    /**
        * This method is called to exit the game, does not work in the browser, since it is a browser and not a standalone application
        * 
        * @param 
        */
    public void ExitGame()
    {
        Debug.Log("Exit");
        Application.Quit();
    }
}
