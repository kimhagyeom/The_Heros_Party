using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// GameScene의 Ground / Pillars 오브젝트가 Built-in RP(Standard 셰이더) 재질로 만들어져 있어
// URP 프로젝트에서 셰이더가 깨져 보이는 문제를 고친다.
// 기존과 동일한 위치/회전/크기로 오브젝트를 새로 만들고, URP 호환 재질을 새로 입힌다.
// (Ground / Pillars 만 건드리며, Player/Enemy/Camera 등 다른 오브젝트는 손대지 않는다.)
public static class RebuildGroundAndPillars
{
    private const string ScenePath = "Assets/01.Scenes/GameScene.unity";

    private struct Placement
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;
    }

    [MenuItem("Tools/Witch Garden/Rebuild Ground And Pillars (URP)")]
    public static void Rebuild()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            Debug.LogError("Universal Render Pipeline/Lit 셰이더를 찾을 수 없습니다. URP 패키지가 설치되어 있는지 확인하세요.");
            return;
        }

        RebuildGround(urpLit);
        RebuildPillars(urpLit);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Ground / Pillars를 URP 호환 재질로 다시 만들었습니다.");
    }

    private static void RebuildGround(Shader urpLit)
    {
        GameObject old = GameObject.Find("Ground");
        if (old == null)
        {
            Debug.LogWarning("Ground 오브젝트를 찾지 못해 건너뜁니다.");
            return;
        }

        Placement placement = CapturePlacement(old.transform);
        Object.DestroyImmediate(old);

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ApplyPlacement(ground.transform, placement);
        SetUrpColor(ground, urpLit, new Color(0.35f, 0.32f, 0.3f));
    }

    private static void RebuildPillars(Shader urpLit)
    {
        GameObject oldRoot = GameObject.Find("Pillars");
        if (oldRoot == null)
        {
            Debug.LogWarning("Pillars 오브젝트를 찾지 못해 건너뜁니다.");
            return;
        }

        Placement rootPlacement = CapturePlacement(oldRoot.transform);

        Placement[] children = new Placement[oldRoot.transform.childCount];
        for (int i = 0; i < oldRoot.transform.childCount; i++)
        {
            children[i] = CapturePlacement(oldRoot.transform.GetChild(i));
        }

        Object.DestroyImmediate(oldRoot);

        GameObject newRoot = new GameObject("Pillars");
        ApplyPlacement(newRoot.transform, rootPlacement);

        foreach (Placement child in children)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = child.name;
            pillar.transform.SetParent(newRoot.transform, false);
            ApplyLocalPlacement(pillar.transform, child);
            SetUrpColor(pillar, urpLit, new Color(0.55f, 0.53f, 0.5f));
        }
    }

    private static Placement CapturePlacement(Transform t)
    {
        return new Placement
        {
            name = t.name,
            position = t.position,
            rotation = t.rotation,
            localScale = t.localScale,
        };
    }

    private static void ApplyPlacement(Transform t, Placement placement)
    {
        t.position = placement.position;
        t.rotation = placement.rotation;
        t.localScale = placement.localScale;
    }

    private static void ApplyLocalPlacement(Transform t, Placement placement)
    {
        // Pillars 자식들은 world position/rotation을 그대로 복원한다 (부모가 동일 위치에 재생성되므로 결과적으로 동일).
        t.position = placement.position;
        t.rotation = placement.rotation;
        t.localScale = placement.localScale;
    }

    private static void SetUrpColor(GameObject go, Shader shader, Color color)
    {
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Material material = new Material(shader);
        material.color = color;
        renderer.sharedMaterial = material;
    }
}
