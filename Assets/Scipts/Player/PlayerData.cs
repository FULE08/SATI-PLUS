using UnityEngine;
using UnityEngine.Events;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance {get; private set;}

    [SerializeField] private int waterDrops = 0;
    public int WaterDrops => waterDrops;

    [HideInInspector] public UnityEvent onWaterDropChanged;
    public void AddWaterDrop(int amount)
    {
        waterDrops += amount;
        onWaterDropChanged?.Invoke();
    }
    public void RemoveWaterDrop(int amount)
    {
        waterDrops -= amount;
        onWaterDropChanged?.Invoke();
    }
}
