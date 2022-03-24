using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities : Singleton<Utilities>
{
    public ObjectShaker objectShaker;
    public Coroutine currentHitStopCoroutine;
    private float shakeLength;

    void CameraShake()
    {
        objectShaker.Begin();
    }

    public void PlayHitStop(float strength)
    {
        shakeLength = strength;
        currentHitStopCoroutine = StartCoroutine(HitStop());
    }

    public void StopHitStop()
    {
        if (currentHitStopCoroutine == null)
        {
            return;
        }
        StopCoroutine(currentHitStopCoroutine);
    }

    IEnumerator HitStop()
    {
        Time.timeScale = 0.05f;
        yield return new WaitForSeconds(shakeLength / 200);
        Time.timeScale = 1;
        CameraShake();
    }
}
