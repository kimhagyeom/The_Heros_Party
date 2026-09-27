using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkillController : MonoBehaviour
{
    public PlayerController playerController;
    public SkillData dashSkillData;
    public SkillExecutor skillExecutor;

    void Awake() 
    {
        playerController = GetComponent<PlayerController>();
        skillExecutor = GetComponent<SkillExecutor>();
    }
    void Start()
    {
        Debug.Log($"playerController: {playerController}, InputActions: {playerController.InputActions}");
        playerController.InputActions.Player.Skill_1.performed += OnSkillPerformed;
    }
    void OnDisable()
    {
        playerController.InputActions.Player.Skill_1.performed -= OnSkillPerformed;
    }
    private void OnSkillPerformed(InputAction.CallbackContext ctx)
    {
        TryUseSkill(dashSkillData); // 임시
    }

    public void TryUseSkill(SkillData data)
    {
        Debug.Log("스킬 사용하였습니다. " + data.name);
    }
}
