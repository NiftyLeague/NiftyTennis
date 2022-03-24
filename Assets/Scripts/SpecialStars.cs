using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialStars : MonoBehaviour
{
    public List<GameObject> starObjects;
    private Vector3 rotationSpeed;
    public Animator animator;
    [HideInInspector] public int amountOfStars;

    private void FixedUpdate()
    {
        transform.Rotate(rotationSpeed);
    }

    public void UpdateStars(int amount)
    {
        amount = Mathf.Clamp(amount, 0, 10);
        amountOfStars = amount;

        if (amount <= 0) 
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        rotationSpeed = new Vector3(0, amount * 1f, 0);

        foreach (GameObject star in starObjects)
        {
            star.SetActive(false);
        }

        for (int i = 0; i < amount; i++)
        {
            starObjects[i].SetActive(true);
        }

        if (amount >= 10)
        {
            animator.Play("filled", 0, 0);
        }
        else
        {
            animator.Play("normal", 0, 0);
        }
    }
}
