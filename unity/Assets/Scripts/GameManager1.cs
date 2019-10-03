using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager1 : MonoBehaviour
{

    public float restart = 1f;
    public float newlevel = 2f;
    public double levelDiff = 1.0;
    public int levelNumber = 1;
    public int score = 0;
    public List<GameObject> wires;


    public void GameOver()
    {
        Debug.Log("Game Over");
    }

    public void LevelWon()
    {
        Debug.Log("Level Complete!");
        levelNumber++;
        Invoke("Restart", newlevel);
    }

    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        Invoke("Restart", restart);
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}