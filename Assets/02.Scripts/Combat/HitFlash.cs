using UnityEngine;
using System.Collections;


[RequireComponent(typeof(SpriteRenderer))]
public class HitFlash : MonoBehaviour
{
    [Header("플레이어 깜빡임")]
    [SerializeField] private float blinkInterval = 0.1f;
    [SerializeField, Range(0f,1f)] private float blinkAlpha = 0.3f;

    [Header("적 피격 틴트")]
    [SerializeField] private Color tintColor = Color.white;
    [SerializeField] private float tintDuration = 0.1f;

    [Header("자동 연결 (적용)")]
    [SerializeField] private Health health;
    [SerializeField] private bool tintOnDamaged;

    private SpriteRenderer sr;
    private Color originColor;
    private Coroutine currentRoutine;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originColor = sr.color;
    }
    void OnEnable()
    {
        if (health != null) health.OnDamaged += HandleDamaged;
    }

    void HandleDamaged(DamageInfo info)
    {
        if (tintOnDamaged) PlayTint();
    }
    // 플레이어용 : duration동안 깜박임
    public void PlayBlink(float duration)
    {
        Restart(BlinkRoutine(duration));
    }
    public void PlayTint()
    {
        Restart(TintRoutine());
    }
    void Restart(IEnumerator newRoutine)
    {
        if(currentRoutine != null) StopCoroutine(currentRoutine);
        sr.color = originColor;
        currentRoutine = StartCoroutine(newRoutine);
    }

    IEnumerator BlinkRoutine(float duration)
    {
        Color dim = originColor;
        dim.a = blinkAlpha;

        float elapsed = 0f;
        bool isDim = true;

        while (elapsed < duration)
        {
            sr.color = isDim ? dim : originColor;
            isDim = !isDim;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        sr.color = originColor;
        currentRoutine = null;
    }

    IEnumerator TintRoutine()
    {
        sr.color = tintColor;
        yield return new WaitForSeconds(tintDuration);
        sr.color = originColor;
        currentRoutine = null;
    }

    void OnDisable()
    {
        if (health != null) health.OnDamaged -= HandleDamaged;

        currentRoutine = null;
        if (sr != null) sr.color = originColor;
    }
}
