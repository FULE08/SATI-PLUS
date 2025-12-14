using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    public static Player Instance {get; private set;}

    [Header("Player Inventory"), Tooltip("ของที่ผู้เล่นมี")]
    [SerializeField] private int waterDrops = 0;
    public int WaterDrops => waterDrops;
    [SerializeField] private List<SeedSlot> seeds = new();
    public SeedSlot[] Seeds => seeds.ToArray();

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

    public void AddSeed(SeedSO seedToAdd, int amount)
    {
        // 1. ค้นหาว่ามี Slot ของ Seed นี้อยู่แล้วหรือไม่
        SeedSlot existingSlot = seeds.Find(slot => slot.seed == seedToAdd);

        if (existingSlot != null)
        {
            // ถ้ามีอยู่แล้ว ให้บวกจำนวนเพิ่ม
            existingSlot.quantity += amount;
        }
        else
        {
            // ถ้ายังไม่มี ให้สร้าง Slot ใหม่แล้วใส่ List
            seeds.Add(new SeedSlot(seedToAdd, amount));
        }
    }

    public bool RemoveSeed(SeedSO seedToRemove, int amount)
    {
        // 1. ค้นหา Slot
        SeedSlot existingSlot = seeds.Find(slot => slot.seed == seedToRemove);

        // 2. เช็คว่ามีของไหม และจำนวนพอให้ลบไหม
        if (existingSlot == null || existingSlot.quantity < amount)
        {
            return false; // ลบไม่ได้
        }

        // 3. ลบจำนวนออก
        existingSlot.quantity -= amount;

        // 4. ถ้าจำนวนเหลือ 0 (หรือต่ำกว่า) ให้ลบ Slot ทิ้งไปเลยเพื่อประหยัดที่
        if (existingSlot.quantity <= 0)
        {
            seeds.Remove(existingSlot);
        }

        return true;
    }

    public int GetSeedAmount(SeedSO seed)
    {
        SeedSlot existingSlot = seeds.Find(slot => slot.seed == seed);
        return existingSlot != null ? existingSlot.quantity : 0;
    }

    public void WateringTree(Tree tree)
    {
        if (tree.IsGrowing || !RemoveWaterDrop(1))
            return;

        tree.Grow();
    }

    [System.Serializable]
    public class SeedSlot
    {
        public SeedSO seed;
        public int quantity;

        public SeedSlot(SeedSO seed, int quantity)
        {
            this.seed = seed;
            this.quantity = quantity;
        }
    }
}
