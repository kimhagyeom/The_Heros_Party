using System.Collections;
using UnityEngine;

public class DelayedVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private float delay = 0.2f;

    void OnEnable()
    {
        visual.enabled = false;
        StartCoroutine(ShowAfterDelay());
    }

    IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        visual.enabled = true;
    }
}