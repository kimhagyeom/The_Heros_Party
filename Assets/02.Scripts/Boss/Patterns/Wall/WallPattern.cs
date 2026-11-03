using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallPattern : BossPattern
{
    [SerializeField] private Wall wallPrefab;
    [SerializeField] private int gap_Count = 2; //팔각형 중 2면만 뚫림 (랜덤)

    WallData D => (WallData)data;
    private readonly float[] angles = { 0, 45, 90, 135, 180, 225, 270, 315 };

    protected override IEnumerator Execute(BossContext ctx)
    {
        for (int i = 0; i < D.Wall_Count; i++)
        {
            SpawnWall();
            if (i < D.Wall_Count - 1)
                yield return new WaitForSeconds(D.Wall_Interval);
        }
        // 마지막 벽이 사라질 때까지 대기
        yield return new WaitForSeconds(D.Wall_Move_Speed > 0f ? D.Wall_Move_Distance / D.Wall_Move_Speed : 0f);
    }

    private void SpawnWall()
    {
        // 서로 다른 면이 gap_Count개 될 때까지 뽑기
        var gaps = new HashSet<int>();
        while (gaps.Count < Mathf.Min(gap_Count, angles.Length))
            gaps.Add(Random.Range(0, angles.Length));

        for (int i = 0; i < angles.Length; i++)
        {
            if (gaps.Contains(i)) continue;

            Wall wall = Instantiate(wallPrefab, transform.position, Quaternion.Euler(0, angles[i], 0));
            wall.Init(D.Wall_Move_Speed, D.Wall_Move_Distance);
            Track(wall.gameObject); // 패턴이 끊기면 같이 삭제
        }
    }
}