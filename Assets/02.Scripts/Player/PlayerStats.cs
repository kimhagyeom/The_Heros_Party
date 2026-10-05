[System.Serializable]
public class PlayerStats
{
    public int Character_ID = 1001;
    public string Character_Name = "아르마";
    public float maxHealth = 100f;
    public float startHealth = 100f;
    public float Move_Speed = 7f;
    public float maxStamina = 100f;
    public float startStamina = 100f;
    public float stamina_Regen = 12.5f;
    public float stamina_Regen_Delay = 0f;
    public float max_Infection = 100f;
    public float start_Infection = 0f;
    public float hit_Invincible_Time = 0.5f;
    public float dash_Stamina_Cost = 50f;
    // 대시: dashDuration 동안 dash_Distance만큼 이동 (속도 = 거리 / 시간)
    public float dash_Distance = 4f;
    public float dashDuration = 0.25f;
    // 대시 무적: 대시 시작 후 dash_Invincible_Start초 뒤부터 dash_Invincible_Duration초 동안
    public float dash_Invincible_Start = 0f;
    public float dash_Invincible_Duration = 0.2f;
    public float base_Atk = 10f;


    public float currentStamina;
    public float currentInfection;

    public void Init()
    {
        currentStamina = maxStamina;
        currentInfection = start_Infection;
    }
}
