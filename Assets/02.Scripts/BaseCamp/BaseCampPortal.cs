using UnityEngine;
using UnityEngine.SceneManagement;

// 베이스캠프 출발 포탈 / 플레이어 닿으면 던전 씬으로 이동
public class BaseCampPortal : MonoBehaviour
{
    [SerializeField] private string mainScene = "GameScene_HG";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        SceneManager.LoadScene(mainScene);
    }
}
