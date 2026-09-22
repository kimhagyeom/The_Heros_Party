using UnityEngine;

public class DPMTracker : MonoBehaviour
{
    private GUIStyle labelStyle;

    private float totalDamage = 0f;
    private float elapsedTime = 0f;
    private bool isTracking = false; // 타이머 작동 여부
    public const float measureWindow = 1f;//60f

    void Update()
    {
        if (isTracking)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= measureWindow)
            {
                Debug.Log($"1초간 데미지: {totalDamage}");
                elapsedTime = 0f;
                totalDamage = 0f;
            }
        }
    }

    public void RecordDamage(float amount)
    {
        if (!isTracking)
        {
            isTracking = true; // 첫 데미지가 들어온 순간 타이머 시작
        }

        if (elapsedTime < measureWindow) // 60초 안 지났으면 계속 누적
        {
            totalDamage += amount;
        }
    }
    public void ResetTracker()
    {
        totalDamage = 0f;
        elapsedTime = 0f;
        isTracking = false;
    }
    void OnGUI()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 36;
            labelStyle.normal.textColor = Color.white;
        }

        GUI.Label(new Rect(10, 10, 300, 40), $"{totalDamage:F0} / {measureWindow:F0}초", labelStyle);
    }
}
