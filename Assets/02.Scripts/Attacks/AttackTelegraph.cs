using UnityEngine;

public class AttackTelegraph : MonoBehaviour
{
    [SerializeField] private LineRenderer outline;  
    [SerializeField] private Transform inline;
    private Vector3 fullScale;

    public void Show(EnemyAtkData d)
    {
        transform.localPosition = new Vector3(0f, 0.02f, d.length / 2f);

        float w = d.width / 2f;
        float l = d.length / 2f;
        outline.positionCount = 4;
        outline.SetPosition(0, new Vector3(-w, 0f, -l));
        outline.SetPosition(1, new Vector3(-w, 0f,  l));
        outline.SetPosition(2, new Vector3( w, 0f,  l));
        outline.SetPosition(3, new Vector3( w, 0f, -l));
        outline.widthMultiplier = 0.05f;

        fullScale = new Vector3(d.width, d.length, 1f);
        inline.localScale = Vector3.zero;

        gameObject.SetActive(true);
    }

    public void SetProgress(float t)
    {
        inline.localScale = fullScale * Mathf.Clamp01(t);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
