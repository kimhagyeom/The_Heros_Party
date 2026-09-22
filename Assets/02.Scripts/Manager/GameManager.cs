using System;
using UnityEngine;

public enum GameState { Ready, Playing, Cleared, GameOver }

public class GameManager : Singleton<GameManager>
{
    public GameState CurrentState { get; private set; } = GameState.Ready;
    public event Action<GameState> OnGameStateChanged;

    // 테스트용 자동 시작. 메인 메뉴/스테이지 선택 붙이면 이 호출은 지우고 메뉴 쪽에서 StartGame()을 직접 호출할 것
    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        SetState(GameState.Playing);
        SpawnManager.Instance.StartSpawning();
    }

    public void ClearStage()
    {
        // Playing 상태에서만 클리어 처리 (중복 호출이나 GameOver 이후 호출로 상태가 꼬이는 것 방지)
        if (CurrentState != GameState.Playing) return;
        SetState(GameState.Cleared);
    }

    public void GameOver()
    {
        if (CurrentState == GameState.GameOver) return;
        SetState(GameState.GameOver);
    }

    private void SetState(GameState state)
    {
        CurrentState = state;
        Debug.Log($"[GameManager] State -> {state}");
        OnGameStateChanged?.Invoke(state);
    }
}
