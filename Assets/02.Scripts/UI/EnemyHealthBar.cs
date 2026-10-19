using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    private Health health;
    void Start()
    {
        health = GetComponentInParent<Health>();
        Enemy enemy = GetComponentInParent<Enemy>();
        if(enemy == null || enemy.Data.enemyType != EnemyType.Normal)
        {
            gameObject.SetActive(false);
        }
    }
    void Update()
    {
        if (health == null || health.Max <= 0f) return;
        hpSlider.value = health.Current / health.Max;
    }
}
