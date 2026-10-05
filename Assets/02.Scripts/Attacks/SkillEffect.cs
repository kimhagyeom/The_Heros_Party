using UnityEngine;

public class SkillEffect : MonoBehaviour
{
    [SerializeField] private float activeTime = 1f;
    void Start()
    {
        Destroy(gameObject, activeTime);
    }

    
}
