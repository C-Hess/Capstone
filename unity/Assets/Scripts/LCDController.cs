using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class TimerExpireEvent : UnityEvent
{

}

public class LCDController : MonoBehaviour
{
    public Animator TenSecondDigit;
    public Animator SecondDigit;
    public Animator ColonDigit;
    public Animator TenthSecondDigit;
    public Animator HundredthSecondDigit;
    public TimerExpireEvent timerExpireEvent;

    [Range(0.0f, 99.99f)]
    public float InitialTime = 60f;

    private Coroutine coroutine;

    // Start is called before the first frame update
    void Start()
    {
        TenSecondDigit.SetFloat("TimeMultiplier", 0.01f);
        SecondDigit.SetFloat("TimeMultiplier", 0.1f);
        TenthSecondDigit.SetFloat("TimeMultiplier", 1f);
        HundredthSecondDigit.SetFloat("TimeMultiplier", 10f);
        SetTimer(InitialTime);
    }

    private void SetTimer(float time)
    {
        TenSecondDigit.SetFloat("OffsetTime", 1.0f - (time /100f));
        SecondDigit.SetFloat("OffsetTime", 1.0f - (time % 10f / 10.0f));
        TenthSecondDigit.SetFloat("OffsetTime", 1.0f - (time * 10 % 10 / 10.0f));
        HundredthSecondDigit.SetFloat("OffsetTime", 1.0f - (time * 100 % 10 / 10.0f));
        coroutine = StartCoroutine(TimerCoroutine(time));
    }


    public void StopTimer()
    {
        TenSecondDigit.enabled = false;
        SecondDigit.enabled = false;
        TenthSecondDigit.enabled = false;
        HundredthSecondDigit.enabled = false;
        ColonDigit.enabled = false;
        StopCoroutine(coroutine);
    }

    IEnumerator TimerCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        StopTimer();
        timerExpireEvent.Invoke();
    }
}
