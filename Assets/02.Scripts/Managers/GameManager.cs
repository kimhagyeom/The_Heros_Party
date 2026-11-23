using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Ready, Playing, Cleared, GameOver }

public class GameManager : Singleton<GameManager>
{
    public GameState CurrentState { get; private set; } = GameState.Ready;
    public event Action<GameState> OnGameStateChanged;

    [SerializeField] private string baseCampScene = "BaseCamp";
    [SerializeField] private float returnDelay = 1f; // 사망 연출 보여주는 시간

    // 테스트용 자동 시작. 메인 메뉴/스테이지 선택 붙이면 이 호출은 지우고 메뉴 쪽에서 StartGame()을 직접 호출할 것
    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        RunSaveManager.Instance.StartRun();
        SetState(GameState.Playing);
        StageManager.Instance.BeginStage();
    }

    public void ClearStage()
    {
        // Playing 상태에서만 클리어 처리 (중복 호출이나 GameOver 이후 호출로 상태가 꼬이는 것 방지)
        if (CurrentState != GameState.Playing) return;
        SetState(GameState.Cleared);
        RunSaveManager.Instance.Delete();

        // 보스 죽는 모습 보여주고 베이스캠프로
        Invoke(nameof(GoToBaseCamp), returnDelay);
    }

    public void GameOver()
    {
        if (CurrentState != GameState.Playing) return;
        SetState(GameState.GameOver);
        RunSaveManager.Instance.Delete();

        // 바로 넘어가면 어색해서 조금 기다렸다가 베이스캠프로
        Invoke(nameof(GoToBaseCamp), returnDelay);
    }

    private void GoToBaseCamp()
    {
        SceneManager.LoadScene(baseCampScene);
    }
    public void CompleteRestart()
    {
        if (CurrentState != GameState.GameOver) return;
        SetState(GameState.Playing);
    }

    private void SetState(GameState state)
    {
        CurrentState = state;
        Debug.Log($"[GameManager] State -> {state}");
        OnGameStateChanged?.Invoke(state);
    }
}
