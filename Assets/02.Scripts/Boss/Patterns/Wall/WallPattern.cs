using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using System.Linq;
using System;
using System.Collections.Generic;


public class WallPattern : BossPattern
{
    [SerializeField] private float lifeTime;
    [SerializeField] private Wall wallPrefab;
    [SerializeField] private float wall_Count;
    [SerializeField] private int gap_Count;

    WallData D => (WallData)data;
    private float[] angles = {0,45,90,135,180,225,270,315,360};
    protected override IEnumerator Execute(BossContext ctx)
    {
        for(int i = 0; i < wall_Count; i++)
        {
            Spawn_Wall();
            yield return new WaitForSeconds(D.cooldown);
        }
    }
    private void Spawn_Wall()
    {
        HashSet<int> gaps = new HashSet<int>();    
        for(int j = 0; j < gap_Count; j++)
                gaps.Add(UnityEngine.Random.Range(0, 8));
        
        for(int i = 0; i < angles.Length; i++)
        {
            if (gaps.Contains(i)) continue;

            Quaternion rot = Quaternion.Euler(0, angles[i], 0);
            Wall wall = Instantiate(wallPrefab, transform.position, rot);
            wall.Init(D.push_Speed, D.push_Distance, lifeTime);
        }
    }
}
