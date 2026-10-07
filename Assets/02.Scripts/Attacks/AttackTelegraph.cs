using System.Collections.Generic;
using UnityEngine;

public class AttackTelegraph : MonoBehaviour
{
    private const int SectorSegments = 24;
 
    [SerializeField] private LineRenderer outline;
    [SerializeField] private MeshFilter inlineFilter;
    [SerializeField] private float lineWidth = 0.05f;
 
    private Mesh mesh;

    public static AttackTelegraph Create(Transform parent, Color color)
    {
        GameObject root = new GameObject("SkillTelegraph");
        root.transform.SetParent(parent, false);
        root.SetActive(false);

        LineRenderer line = root.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.sharedMaterial = CreateMaterial(new Color(color.r, color.g, color.b, 0.9f));
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;

        GameObject inline = new GameObject("Inline");
        inline.transform.SetParent(root.transform, false);
        MeshFilter filter = inline.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = inline.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = CreateMaterial(new Color(color.r, color.g, color.b, 0.35f));
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        AttackTelegraph telegraph = root.AddComponent<AttackTelegraph>();
        telegraph.outline = line;
        telegraph.inlineFilter = filter;
        return telegraph;
    }

    private static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Unlit");

        Material material = new Material(shader);
        material.color = color;
        return material;
    }

    public void Show(EnemyAtkData d,Vector3 targetPos)
    {
        if (d.atk_Type == AttackType.Aoe)
            transform.position = targetPos + Vector3.up * 0.02f;
        else
            transform.localPosition = new Vector3(0f, 0.02f, 0f);

        Draw(GetVertices(d));
    }

    public void ShowSector(float radius, float angle)
    {
        transform.localPosition = new Vector3(0f, 0.02f, 0f);

        Draw(GetSectorVertices(radius, angle, SectorSegments));
    }

    // 일섬 경로Box
    public void ShowBoxAt(Vector3 worldStart, Quaternion rotation, float width, float length)
    {
        transform.SetPositionAndRotation(worldStart + Vector3.up * 0.02f, rotation);

        Draw(GetBoxVertices(width, length));
    }

    // 일섬 대상 표시: 월드 위치에 원 테두리
    public void ShowCircleAt(Vector3 worldPos, float radius)
    {
        transform.SetPositionAndRotation(worldPos + Vector3.up * 0.02f, Quaternion.identity);

        Draw(GetSectorVertices(radius, 360f, SectorSegments));
    }

    //(적/플레이어 공용)
    private void Draw(List<Vector3> vertices)
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            inlineFilter.mesh = mesh;
        }
        if (vertices == null) return;

        outline.widthMultiplier = lineWidth;
        outline.positionCount = vertices.Count;
        outline.SetPositions(vertices.ToArray());
 
        BuildMesh(vertices);
        inlineFilter.transform.localScale = Vector3.zero;
 
        gameObject.SetActive(true);
    }
 
    public void SetProgress(float t)
    {
        inlineFilter.transform.localScale = Vector3.one * Mathf.Clamp01(t);
    }
 
    public void Hide()
    {
        gameObject.SetActive(false);
    }
 
    private List<Vector3> GetVertices(EnemyAtkData d)
    {
        if (d.atk_Type == AttackType.Projectile)
            return GetBoxVertices(0.05f, d.atk_Range); // 투사체 공격은 직선형 판정으로 표시
 
        switch (d.hitbox_Shape)
        {
            case HitboxShape.Box:
                return GetBoxVertices(d.width, d.length);
            case HitboxShape.Sector:
                return GetSectorVertices(d.radius, d.angle, SectorSegments);
            default:
                Debug.LogWarning($"처리 안 된 판정 모양: {d.hitbox_Shape}");
                return null;
        }
    }
 
    private List<Vector3> GetBoxVertices(float width, float length)
    {
        float halfWidth = width / 2f;
 
        return new List<Vector3>
        {
            new Vector3(-halfWidth, 0f, 0f),
            new Vector3(-halfWidth, 0f, length),
            new Vector3( halfWidth, 0f, length),
            new Vector3( halfWidth, 0f, 0f),
        };
    }
 
    // 적 위치를 꼭짓점으로 하는 부채꼴 (360도면 꼭짓점 없이 원)
    private List<Vector3> GetSectorVertices(float radius, float angle, int segments)
    {
        List<Vector3> vertices = new List<Vector3>();
 
        if (angle < 360f)
            vertices.Add(Vector3.zero);
 
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -angle / 2f + angle * i / segments;
            vertices.Add(Quaternion.Euler(0f, currentAngle, 0f) * Vector3.forward * radius);
        }
        return vertices;
    }
 
    private void BuildMesh(List<Vector3> vertices)
    {
        List<int> triangles = new List<int>();
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }
 
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
    }
}
 