using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities : Singleton<Utilities>
{
    public ObjectShaker objectShaker;
    public Coroutine currentHitStopCoroutine;

    void CameraShake()
    {
        objectShaker.Begin();
    }

    public void PlayHitStop()
    {
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
        yield return new WaitForSeconds(0.01f);
        Time.timeScale = 1;
        CameraShake();
    }
}
