using System.Collections.Generic;
using UnityEngine;

public class AttackTelegraph : MonoBehaviour
{
    private const int SectorSegments = 24;
 
    [SerializeField] private LineRenderer outline;
    [SerializeField] private MeshFilter inlineFilter;
    [SerializeField] private float lineWidth = 0.05f;
 
    private Mesh mesh;
 
    public void Show(EnemyAtkData d)
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            inlineFilter.mesh = mesh;
        }
 
        transform.localPosition = new Vector3(0f, 0.02f, 0f);
 
        List<Vector3> vertices = GetVertices(d);
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
 