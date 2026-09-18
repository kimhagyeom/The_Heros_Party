using UnityEngine;

namespace WitchGardenDemo
{
    public class Health : MonoBehaviour, IDamageable
    {
        public int maxHealth = 100;

        private int currentHealth;

        void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            currentHealth = Mathf.Max(0, currentHealth - amount);
            Debug.Log($"{gameObject.name} took {amount} damage. ({currentHealth}/{maxHealth})");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"{gameObject.name} died.");
            gameObject.SetActive(false);
        }
    }
}
