using UnityEngine;

public class DPMTracker : MonoBehaviour
{
    private float totalDamage = 0f;
    private float elapsedTime = 0f;
    private bool isTracking = false; // 타이머 작동 여부
    private const float measureWindow = 60f;

    void Update()
    {
        if (isTracking && elapsedTime < measureWindow)
        {
            elapsedTime += Time.deltaTime;
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
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        seconds = seconds >= 60 ? 60 : seconds;
        GUI.Label(new Rect(10, 10, 300, 30), $"{totalDamage:F0} / {seconds:F0}초");
    }
}
