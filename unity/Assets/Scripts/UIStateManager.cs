using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStateManager : MonoBehaviour
{
    public enum UIStates
    {
        GAME,
        WIN,
        LOSE
    }

    public UIStates currentUIState = UIStates.GAME;

    public GameObject winScreen;
    public GameObject loseScreen;
    public GameObject gameScreen;


    // Start is called before the first frame update
    void Start()
    {
        switch(currentUIState)
        {
            case UIStates.GAME:
                SwitchGame();
                break;
            case UIStates.LOSE:
                SwitchLose();
                break;
            case UIStates.WIN:
                SwitchWin();
                break;
        }
    }
    /**
        * This method is called to signify what state the game is currently in such as the win state, lose state, or game state
        * 
        * @param 
        */
    public UIStates GetState()
    {
        return currentUIState;
    }
    /**
        * This method switches the gamesceen to active and sets the win and lose screen to false so they do not appear
        * 
        * @param 
        */
    public void SwitchGame()
    {
        currentUIState = UIStates.GAME;
        gameScreen.SetActive(true);
        winScreen.SetActive(false);
        loseScreen.SetActive(false);
    }
    /**
        * This method switches the lose screen to true so that it does appear, and sets the gamescreen and win screen to false so that they do not appear
        * 
        * @param 
        */
    public void SwitchLose()
    {
        currentUIState = UIStates.LOSE;
        gameScreen.SetActive(false);
        winScreen.SetActive(false);
        loseScreen.SetActive(true);
    }
    /**
        * This method switches the win screen to true so that it does appear, and it sets the game screen and lose screen to false so they do not appear
        * 
        * @param 
        */
    public void SwitchWin()
    {
        currentUIState = UIStates.WIN;
        gameScreen.SetActive(false);
        winScreen.SetActive(true);
        loseScreen.SetActive(false);
    }



}
