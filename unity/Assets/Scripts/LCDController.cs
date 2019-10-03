using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LCDController : MonoBehaviour
{
    public Animator TenSecondDigit;
    public Animator SecondDigit;
    public Animator ColonDigit;
    public Animator TenthSecondDigit;
    public Animator HundredthSecondDigit;

    [Range(0.0f, 99.99f)]
    public float InitialTime = 60f;

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
        StartCoroutine(TimerCoroutine(time));
    }

    IEnumerator TimerCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        TenSecondDigit.enabled = false;
        SecondDigit.enabled = false;
        TenthSecondDigit.enabled = false;
        HundredthSecondDigit.enabled = false;
        ColonDigit.enabled = false;
    }
}
