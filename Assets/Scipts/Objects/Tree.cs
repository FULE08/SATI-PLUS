using UnityEngine;
using UnityEngine.UI;

public class Tree : MonoBehaviour
{
    [SerializeField] private SeedSO seed;
    [SerializeField] private int growthStage = 0;
    [SerializeField] private bool isWatered = false;
    public bool IsWatered => isWatered;

    public SeedSO Seed => seed;
    public int GrowthStage => growthStage;

    [SerializeField] private Button wateringButton;

    private void Awake()
    {
        wateringButton.onClick.AddListener(() => Player.Instance.WateringTree(this));
    }

    public void Grow()
    {
        isWatered = true;
        //ต้นไม้โตจนถึงเวลาหยุดแล้วก็มาหยดน้ำใหม่
        growthStage++; //ถ้าถึงขั้นแล้วก็จะเปลี่ยน State
    }
}
