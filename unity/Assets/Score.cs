using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Score : MonoBehaviour
{
    public Text text;
    public int startTime;
    public int time;
    public void StartTimer()
    {
        time = startTime;
        StartCoroutine("Timer");

    }
    public void StopTimer()
    {
        StopCoroutine("timer");
        text.text = "Score: " + time;
    }

    // Update is called once per frame
    public IEnumerator timer()
    {
        while(time > 0)
        {
            time--;
            yield return new WaitForSeconds(1f);
        }
    }
}
