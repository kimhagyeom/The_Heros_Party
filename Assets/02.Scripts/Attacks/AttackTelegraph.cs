using System.Collections.Generic;
using UnityEngine;

public struct HitboxInfo
{
    public HitboxShape shape;
    public float radius, angle;
    public float width, length;
}
public class AttackTelegraph : MonoBehaviour
{
    private const int SectorSegments = 24;

    // 이미지 속 원의 반지름 (UV 기준)
    // 256px 이미지에서 원 반지름이 100px → 100 / 256 ≈ 0.39
    // 가장자리의 반투명 픽셀을 피하려고 살짝 안쪽 값 사용
    private const float ImageRadius = 0.38f;

    // 모양에 따라 UV를 다르게 붙이기 위한 구분
    private enum Kind { Sector, Circle, Box }

    [SerializeField] private LineRenderer outline;
    [SerializeField] private MeshFilter inlineFilter;
    [SerializeField] private float lineWidth = 0.05f;

    private Mesh mesh;

    public static AttackTelegraph Create(Transform parent, Color color, Material fillMaterial)
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

        // [변경] 이미지 머티리얼이 있으면 그걸 쓰고, 없으면 기존처럼 단색
        meshRenderer.sharedMaterial = fillMaterial != null
            ? fillMaterial
            : CreateMaterial(new Color(color.r, color.g, color.b, 0.35f));

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

    public void Show(EnemyAtkData d, Vector3 targetPos)
    {
        if (d.atk_Type == AttackType.Aoe)
            transform.position = targetPos + Vector3.up * 0.02f;
        else
            transform.localPosition = new Vector3(0f, 0.02f, 0f);

        // [변경] GetVertices 대신 여기서 바로 모양별로 그림 (UV 계산에 크기가 필요해서)
        if (d.atk_Type == AttackType.Projectile)
        {
            // 투사체 공격은 직선형 판정으로 표시
            Draw(GetBoxVertices(0.05f, d.atk_Range), Kind.Box, d.atk_Range);
            return;
        }

        switch (d.hitbox_Shape)
        {
            case HitboxShape.Box:
                Draw(GetBoxVertices(d.width, d.length), Kind.Box, d.length);
                break;
            case HitboxShape.Sector:
                DrawSector(d.radius, d.angle);
                break;
            default:
                Debug.LogWarning($"처리 안 된 판정 모양: {d.hitbox_Shape}");
                break;
        }
    }

    public void ShowSector(float radius, float angle)
    {
        transform.localPosition = new Vector3(0f, 0.02f, 0f);

        // 부모(플레이어) 스케일만큼 커지지 않도록 상쇄 → 월드 기준 반경과 판정이 일치
        float parentScale = transform.parent != null ? transform.parent.lossyScale.x : 1f;
        DrawSector(radius / parentScale, angle);
    }

    // 일섬 경로Box
    public void ShowBoxAt(Vector3 worldStart, Quaternion rotation, float width, float length)
    {
        transform.SetPositionAndRotation(worldStart + Vector3.up * 0.02f, rotation);

        Draw(GetBoxVertices(width, length), Kind.Box, length);
    }

    // 일섬 대상 표시: 월드 위치에 원
    public void ShowCircleAt(Vector3 worldPos, float radius)
    {
        transform.SetPositionAndRotation(worldPos + Vector3.up * 0.02f, Quaternion.identity);

        DrawSector(radius, 360f);
    }

    public void SetProgress(float t)
    {
        inlineFilter.transform.localScale = Vector3.one * Mathf.Clamp01(t);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // [추가] 부채꼴/원 공통 진입점
    private void DrawSector(float radius, float angle)
    {
        Kind kind = angle >= 360f ? Kind.Circle : Kind.Sector;
        Draw(GetSectorVertices(radius, angle, SectorSegments), kind, radius);
    }

    //(적/플레이어 공용)
    private void Draw(List<Vector3> vertices, Kind kind, float size)
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            inlineFilter.mesh = mesh;
        }
        if (vertices == null) return;

        // [변경] 원은 0번(중심점)을 빼고 둘레만 테두리로 그림
        //        (넣으면 중심에서 바깥으로 선이 하나 생김)
        int start = kind == Kind.Circle ? 1 : 0;
        outline.widthMultiplier = lineWidth;
        outline.positionCount = vertices.Count - start;
        for (int i = start; i < vertices.Count; i++)
            outline.SetPosition(i - start, vertices[i]);

        BuildMesh(vertices, GetUVs(vertices, kind, size));
        inlineFilter.transform.localScale = Vector3.zero;

        gameObject.SetActive(true);
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

    // [변경] 원이든 부채꼴이든 항상 0번 = 중심점
    //        (중심 기준으로 UV를 붙여야 그라데이션이 맞음)
    private List<Vector3> GetSectorVertices(float radius, float angle, int segments)
    {
        List<Vector3> vertices = new List<Vector3>();

        vertices.Add(Vector3.zero);

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -angle / 2f + angle * i / segments;
            vertices.Add(Quaternion.Euler(0f, currentAngle, 0f) * Vector3.forward * radius);
        }
        return vertices;
    }

    // [추가] 각 점이 원 이미지의 어디를 가져올지
    private List<Vector2> GetUVs(List<Vector3> vertices, Kind kind, float size)
    {
        if (size <= 0f) size = 1f;

        List<Vector2> uvs = new List<Vector2>(vertices.Count);
        foreach (Vector3 v in vertices)
        {
            if (kind == Kind.Box)
            {
                // 박스: 이미지의 중심 → 위쪽 끝까지 세로줄만 사용
                // → 적 쪽은 옅고 앞으로 갈수록 진해짐
                uvs.Add(new Vector2(0.5f, 0.5f + v.z / size * ImageRadius));
            }
            else
            {
                // 원/부채꼴: 점의 방향 그대로 원 이미지 위에 대응
                uvs.Add(new Vector2(0.5f + v.x / size * ImageRadius,
                                    0.5f + v.z / size * ImageRadius));
            }
        }
        return uvs;
    }

    // [변경] UV도 같이 넣음
    private void BuildMesh(List<Vector3> vertices, List<Vector2> uvs)
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
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
    }
}