using System.Collections.Generic;
using UnityEngine;

public class EnemyRegistry : Singleton<EnemyRegistry>
{
    public List<Enemy> ActiveEnemies { get; private set; } = new List<Enemy>();

    public void Register(Enemy enemy)
    {
        ActiveEnemies.Add(enemy);
    }

    public void Unregister(Enemy enemy)
    {
        ActiveEnemies.Remove(enemy);
    }
}