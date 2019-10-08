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

    public void SwitchGame()
    {
        currentUIState = UIStates.GAME;
        gameScreen.SetActive(true);
        winScreen.SetActive(false);
        loseScreen.SetActive(false);
    }
    public void SwitchLose()
    {
        currentUIState = UIStates.LOSE;
        gameScreen.SetActive(false);
        winScreen.SetActive(false);
        loseScreen.SetActive(true);
    }
    public void SwitchWin()
    {
        currentUIState = UIStates.WIN;
        gameScreen.SetActive(false);
        winScreen.SetActive(true);
        loseScreen.SetActive(false);
    }



}
