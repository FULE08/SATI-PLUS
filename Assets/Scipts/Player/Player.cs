using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    public static Player Instance {get; private set;}

    [SerializeField] private int waterDrops = 0;
    public int WaterDrops => waterDrops;

    [HideInInspector] public UnityEvent onWaterDropChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void AddWaterDrop(int amount)
    {
        waterDrops += amount;
        onWaterDropChanged?.Invoke();
    }
    public bool RemoveWaterDrop(int amount)
    {
        if (waterDrops < amount)
            return false;

        waterDrops -= amount;
        onWaterDropChanged?.Invoke();
        return true;
    }

    public void WateringTree(Tree tree)
    {
        if (tree.IsGrowing || !RemoveWaterDrop(1))
            return;

        tree.Grow();
    }
}
