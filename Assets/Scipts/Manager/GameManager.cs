using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Button startMinigameButton;

    private void Awake()
    {
        startMinigameButton.onClick.AddListener(StartGame);
    }

    private void StartGame()
    {
        Player.Instance.AddWaterDrop(1);
    }
}
