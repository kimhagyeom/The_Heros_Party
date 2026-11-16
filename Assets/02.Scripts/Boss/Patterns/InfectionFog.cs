using UnityEngine;

// ?? ??????? ???????? ?????? ???. ??? ??? ???? ???? ???????? ????? ??
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class InfectionFog : MonoBehaviour
{
    private const int Segments = 64;

    [Header("?? ??? ???")]
    [SerializeField] private float radiusX = 30f;
    [SerializeField] private float radiusZ = 20f;

    [Header("???")]
    [SerializeField, Range(0f, 1f)] private float startInnerRatio = 0.9f; //???? ???
    [SerializeField, Range(0f, 1f)] private float endInnerRatio = 0.7f; //?? ?????? ?? ???? ???
    [SerializeField] private float spreadTime = 10f;

    [Header("????")]
    [SerializeField] private float damage = 5f;
    [SerializeField] private float tickInterval = 1f; //???? ????

    [Header("???")]
    [SerializeField] private Color fogColor = new Color(0.45f, 0.15f, 0.7f, 0.45f);

    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private Vector3[] vertices;
    private Transform target;
    private IDamageable targetDamageable;
    private bool isActive;
    private float elapsed;
    private float stayTime;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer.sharedMaterial == null)
        {
            Material material = new Material(Shader.Find("Sprites/Default"));
            material.color = fogColor;
            meshRenderer.sharedMaterial = material;
        }
        BuildMesh();
        meshRenderer.enabled = false;
    }

    public void Begin(Transform newTarget)
    {
        if (isActive) return;

        if (!newTarget.TryGetComponent(out targetDamageable))
        {
            Debug.LogWarning("InfectionFog: ??? IDamageable?? ????", this);
            return;
        }
        target = newTarget;
        elapsed = 0f;
        stayTime = 0f;
        isActive = true;
        UpdateMesh(startInnerRatio);
        meshRenderer.enabled = true;
    }
    public void End()
    {
        isActive = false;
        target = null;
        targetDamageable = null;
        if (meshRenderer != null) meshRenderer.enabled = false;
    }
    void Update()
    {
        if (!isActive || target == null) return;

        elapsed += Time.deltaTime;
        float t = spreadTime > 0f ? Mathf.Clamp01(elapsed / spreadTime) : 1f;
        float innerRatio = Mathf.Lerp(startInnerRatio, endInnerRatio, t);
        if (t < 1f || stayTime == 0f) UpdateMesh(innerRatio);

        if (IsInFog(target.position, innerRatio))
        {
            stayTime += Time.deltaTime;
            if (stayTime >= tickInterval)
            {
                stayTime -= tickInterval;
                targetDamageable.TakeDamage(new DamageInfo { damage = damage });
            }
        }
        else
        {
            stayTime = 0f;
        }
    }
    bool IsInFog(Vector3 position, float innerRatio)
    {
        Vector3 offset = position - transform.position;
        float x = offset.x / radiusX;
        float z = offset.z / radiusZ;
        return x * x + z * z >= innerRatio * innerRatio;
    }
    void BuildMesh()
    {
        mesh = new Mesh { name = "InfectionFogRing" };
        vertices = new Vector3[(Segments + 1) * 2];
        int[] triangles = new int[Segments * 6];

        for (int i = 0; i < Segments; i++)
        {
            int inner = i * 2;
            int outer = inner + 1;
            int triangle = i * 6;
            triangles[triangle] = inner;
            triangles[triangle + 1] = inner + 2;
            triangles[triangle + 2] = outer;
            triangles[triangle + 3] = outer;
            triangles[triangle + 4] = inner + 2;
            triangles[triangle + 5] = outer + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
    void UpdateMesh(float innerRatio)
    {
        for (int i = 0; i <= Segments; i++)
        {
            float angle = Mathf.PI * 2f * i / Segments;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            vertices[i * 2] = new Vector3(cos * radiusX * innerRatio, 0f, sin * radiusZ * innerRatio);
            vertices[i * 2 + 1] = new Vector3(cos * radiusX, 0f, sin * radiusZ);
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
    }
    void OnDrawGizmosSelected()
    {
        DrawEllipse(1f, Color.white);
        DrawEllipse(startInnerRatio, Color.magenta);
        DrawEllipse(endInnerRatio, Color.red);
    }
    void DrawEllipse(float ratio, Color color)
    {
        Gizmos.color = color;
        Vector3 previous = transform.position + new Vector3(radiusX * ratio, 0f, 0f);
        for (int i = 1; i <= Segments; i++)
        {
            float angle = Mathf.PI * 2f * i / Segments;
            Vector3 next = transform.position + new Vector3(Mathf.Cos(angle) * radiusX * ratio, 0f, Mathf.Sin(angle) * radiusZ * ratio);
            Gizmos.DrawLine(previous, next);
            previous = next;
        }
    }
}