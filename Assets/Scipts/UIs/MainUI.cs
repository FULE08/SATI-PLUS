using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainUI : MonoBehaviour
{
    [SerializeField] private TMP_Text waterDropText;

    private PlayerData player {get => PlayerData.Instance;}

    private void Awake()
    {
        FetchWaterDropText();
    }

    private void Start()
    {
        player.onWaterDropChanged.AddListener(FetchWaterDropText);
    }

    private void FetchWaterDropText()
    {
        waterDropText.text = player.WaterDrops.ToString();
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.onWaterDropChanged.RemoveListener(FetchWaterDropText);
        }
    }
}