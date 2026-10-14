using System.Collections;
using UnityEngine;

// 방 전환 시퀀스 담당
// 순서: 입력 막기 → (페이드 아웃) → 텔레포트 → (페이드 인) → 입력 풀기 → room.StartCombat()
// 페이드는 ScreenFader 작업 후 TODO 자리에 추가
public class StageManager : Singleton<StageManager>
{
    [SerializeField] private Room startRoom;          // 게임 시작 시 첫 방
    [SerializeField] private PlayerController player; // 비워두면 자동으로 찾음

    public Room CurrentRoom { get; private set; }
    private bool isTransitioning; // 전환 중 중복 요청 방지

    // GameManager.StartGame()에서 호출
    public void BeginStage()
    {
        if (startRoom == null)
        {
            Debug.LogWarning("[StageManager] startRoom이 비어 있음", this);
            return;
        }
        StartCoroutine(EnterRoomRoutine(startRoom, isFirstRoom: true));
    }

    // RoomExit(출구)에서 호출. nextRoom이 null이면 던전 끝
    public void RequestMoveTo(Room nextRoom)
    {
        if (isTransitioning) return;

        if (nextRoom == null)
        {
            Debug.Log("[StageManager] 다음 방 없음 → 던전 클리어 (의도한 게 아니면 출구의 도착 Room 연결 확인)");
            GameManager.Instance.ClearStage();
            return;
        }

        StartCoroutine(EnterRoomRoutine(nextRoom, isFirstRoom: false));
    }

    private IEnumerator EnterRoomRoutine(Room nextRoom, bool isFirstRoom)
    {
        isTransitioning = true;

        if (!TryGetPlayer())
        {
            isTransitioning = false;
            yield break;
        }

        SetPlayerInput(false);

        if (!isFirstRoom)
        {
            // TODO: ScreenFader → yield return ScreenFader.Instance.FadeOut();
        }

        CurrentRoom = nextRoom;
        player.Teleport(nextRoom.ArrivalPosition);
        yield return null; // 텔레포트 위치가 반영된 다음 프레임부터 진행

        // TODO: ScreenFader → yield return ScreenFader.Instance.FadeIn();
        // (첫 방은 검은 화면에서 시작하므로 FadeIn만)

        SetPlayerInput(true);
        isTransitioning = false;

        CurrentRoom.StartCombat();
    }

    // GameManager.Start()에서 바로 호출될 수 있어서, Start 대신 필요할 때 찾음
    private bool TryGetPlayer()
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerController>();

        if (player == null)
            Debug.LogWarning("[StageManager] PlayerController를 찾을 수 없음", this);

        return player != null;
    }

    // 나중에 PlayerController에 SetInputEnabled(bool)를 만들면 그걸로 교체
    private void SetPlayerInput(bool enabled)
    {
        if (enabled) player.InputActions.Player.Enable();
        else player.InputActions.Player.Disable();
    }
}