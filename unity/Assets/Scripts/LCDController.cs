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

    private Coroutine coroutine = null;

    // Start is called before the first frame update
    void Start()
    {
        TenSecondDigit.SetFloat("TimeMultiplier", 0.01f);
        SecondDigit.SetFloat("TimeMultiplier", 0.1f);
        TenthSecondDigit.SetFloat("TimeMultiplier", 1f);
        HundredthSecondDigit.SetFloat("TimeMultiplier", 10f);
    }

    public void SetTimer(float time)
    {
        TenSecondDigit.enabled = true;
        SecondDigit.enabled = true;
        TenthSecondDigit.enabled = true;
        HundredthSecondDigit.enabled = true;
        ColonDigit.enabled = true;

        TenSecondDigit.SetFloat("OffsetTime", 1.0f - (time/100f));
        TenSecondDigit.Play(0, -1, 0);

        SecondDigit.SetFloat("OffsetTime", 1.0f - (time % 10f / 10.0f));
        SecondDigit.Play(0, -1, 0);

        TenthSecondDigit.SetFloat("OffsetTime", 1.0f - (time * 10 % 10 / 10.0f));
        TenthSecondDigit.Play(0, -1, 0);

        HundredthSecondDigit.SetFloat("OffsetTime", 1.0f - (time * 100 % 10 / 10.0f));
        HundredthSecondDigit.Play(0, -1, 0);


        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(TimerCoroutine(time));
    }

    public void StopTimer()
    {
        TenSecondDigit.enabled = false;
        TenSecondDigit.playbackTime = 0;

        SecondDigit.enabled = false;
        TenSecondDigit.playbackTime = 0;

        TenthSecondDigit.enabled = false;
        TenSecondDigit.playbackTime = 0;

        HundredthSecondDigit.enabled = false;
        TenSecondDigit.playbackTime = 0;

        ColonDigit.enabled = false;
        TenSecondDigit.playbackTime = 0;

        if(coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }

    // need to make a public function that says how much time the clock starts at, using the systemTime, or Time.time, or systemdelta, then every update, you add the delta time to the total (this will give you how much time has elapsed.
    // need the private variable lcdcontroller uses, and the function call that returns that, and in the update function, you increase the thing by the delta time
    IEnumerator TimerCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        StopTimer();
        timerExpireEvent.Invoke();
    }
}
