using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScore : MonoBehaviour
{
    public Text scoreText;
    public static int score;
    // Update is called once per frame
    void Update()
    {
        string currentScore = scoreText.text;
        int intScore = 0;
        int.TryParse(currentScore, out intScore);

        scoreText.text = (intScore).ToString();
    }

    public int getScore()
    {
        return score;
    }

    public void setScore(int updatedScore)
    {
        score = updatedScore;
    }
}
