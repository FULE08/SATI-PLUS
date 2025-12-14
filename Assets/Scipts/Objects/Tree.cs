using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq; // จำเป็นสำหรับการใช้ DateTime

public class Tree : MonoBehaviour
{
    public enum TreeState
    {
        Sapling,    // ต้นอ่อน
        Mature,     // โตเต็มวัย (ออกผลได้)
        Withered    // เหี่ยวเฉา
    }

    [Header("Settings")]
    [SerializeField] private string treeID = "Tree_01"; // *สำคัญ* ชื่อ ID สำหรับเซฟข้อมูล (ถ้ามีหลายต้นต้องตั้งไม่ซ้ำกัน)
    [SerializeField] private SeedSO seed;
    [SerializeField, Range(0, 100)] private float dropChance = 50f;

    [Header("Status")]
    [SerializeField] private TreeState currentState = TreeState.Sapling;
    private int growthStage = 0;
    [SerializeField] private bool isGrowing = false; 

    // Public Properties
    public bool IsGrowing => isGrowing; // เปลี่ยนให้คนอื่นเช็คได้ว่ากำลังโตไหม

    [Header("UI & References")]
    [SerializeField] private Button wateringButton;
    [SerializeField] private TMP_Text dropingWaterCountText;

    // Internal Variables
    private DateTime finishTime; // เวลาที่จะโตเสร็จ (เวลาจริง)
    private bool isTimerRunning = false; // ตัวคุม Update ไม่ให้ทำงานฟรีๆ
    private List<SeedSO> allSeeds = new();
    private GameObject seedPrefab;

    private void Start()
    {
        // 1. โหลดข้อมูลเก่ากลับมา (Level, State)
        LoadTreeStatus();

        // 2. ตรวจสอบเวลา (หัวใจหลักของ Offline System)
        CheckOfflineProgress();

        // Setup ปุ่ม
        if (wateringButton != null)
        {
            wateringButton.onClick.AddListener(() => 
            {
                Player.Instance.WateringTree(this);
            });
        }

        allSeeds = Resources.LoadAll<SeedSO>("Tree/Seeds").ToList();
        seedPrefab = Resources.Load<GameObject>("UIs/SeedPrefab");
    }

    private void Update()
    {
        // Logic การนับถอยหลัง (เฉพาะตอนเปิดเกมอยู่)
        if (isTimerRunning)
        {
            // คำนวณเวลาที่เหลือโดยเทียบกับเวลาปัจจุบันเสมอ (ป้องกัน Time drift)
            TimeSpan remaining = finishTime - DateTime.Now;

            if (remaining.TotalSeconds <= 0)
            {
                FinishGrowthCycle();
            }
            else
            {
                UpdateTimerUI(remaining);
            }
        }
    }

    public void Grow()
    {
        if (isGrowing || currentState == TreeState.Withered) return;

        isGrowing = true;
        
        // --- เริ่มบันทึกเวลา ---
        // กำหนดเวลาเสร็จ = เวลาปัจจุบัน + ระยะเวลาที่ต้องรอ
        finishTime = DateTime.Now.AddSeconds(seed.GrowthDuration);
        
        // เซฟลงเครื่องทันที กันเกมหลุด
        SaveTargetTime();
        
        isTimerRunning = true;
        
        if(wateringButton) wateringButton.interactable = false; 
    }

    private void CheckOfflineProgress()
    {
        // ถ้าสถานะบอกว่ากำลังโตอยู่ ให้เช็คว่าโตเสร็จตอนปิดเกมไปหรือยัง
        if (PlayerPrefs.GetInt(treeID + "_IsGrowing", 0) == 1)
        {
            string timeStr = PlayerPrefs.GetString(treeID + "_FinishTime", "");
            if (!string.IsNullOrEmpty(timeStr))
            {
                // แปลง String กลับเป็น DateTime
                finishTime = DateTime.Parse(timeStr); 
                
                // เช็คว่าปัจจุบันเลยเวลาเสร็จหรือยัง
                if (DateTime.Now >= finishTime)
                {
                    // Scenario A: เวลาผ่านไปนานแล้ว จนเสร็จไปแล้วตอนปิดเกม
                    FinishGrowthCycle(); 
                }
                else
                {
                    // Scenario B: ยังไม่เสร็จ กลับมานับถอยหลังต่อจากที่เหลือ
                    isGrowing = true;
                    isTimerRunning = true;
                    if(wateringButton) wateringButton.interactable = false; 
                }
            }
        }
        else
        {
            // ถ้าไม่ได้กำลังโต ก็แค่อัปเดต UI ปกติ
            UpdateUIState();
        }
    }

    private void FinishGrowthCycle()
    {
        isTimerRunning = false;
        isGrowing = false;
        
        // ล้างค่าการ Save เรื่องเวลาออก เพราะโตเสร็จแล้ว
        PlayerPrefs.DeleteKey(treeID + "_FinishTime");
        PlayerPrefs.SetInt(treeID + "_IsGrowing", 0);

        // เพิ่มขั้นและเซฟ
        growthStage++;
        CheckTreeState(); 
        SaveTreeStatus(); // เซฟ Level ใหม่

        if (wateringButton && currentState != TreeState.Withered) 
        {
            wateringButton.interactable = true;
        }

        UpdateUIState();
    }

    private void CheckTreeState()
    {
        if (growthStage < 2)
            currentState = TreeState.Sapling;
        else if (growthStage >= 2 && growthStage < 5)
        {
            currentState = TreeState.Mature;
            TryDropSeed();
        }
        else
        {
            currentState = TreeState.Withered;
            if(wateringButton) wateringButton.interactable = false;
        }
    }

    private void TryDropSeed()
    {
        int rounds = 3;

        Debug.Log($"<color=cyan>--- Harvesting Start ({seed.Rarity} Tree) ---</color>");

        for (int i = 0; i < rounds; i++)
        {
            // 2.1 สุ่มจำนวนที่จะได้ในรอบนี้ (1 ถึง 3 ชิ้น)
            // Random.Range(int min, int max) ตัวหลังจะไม่ถูกรวม ดังนั้นต้องใส่ 4 เพื่อให้ได้ถึง 3
            int quantity = UnityEngine.Random.Range(1, 4); 

            // 2.2 สุ่ม "ชนิด" ของเมล็ด (ใช้ระบบถ่วงน้ำหนักตามเดิม)
            SeedSO reward = GetWeightedRandomSeed();

            if (reward != null)
            {
                Debug.Log($"Round {i + 1}: Got {quantity} x {reward.SeedName} [{reward.Rarity}]");

                Canvas canvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None).FirstOrDefault();
                
                for (int j = 0; j < quantity; j++)
                {
                    GameObject seed = Instantiate(seedPrefab, canvas.transform);
                    seed.GetComponent<Seed>().Init(reward);
                }
            }
        }
        
        Debug.Log("<color=cyan>--- Harvesting End ---</color>");
    }

    private SeedSO GetWeightedRandomSeed()
    {
        int totalWeight = 0;
        List<int> weights = new List<int>();

        // แปลง Rarity ของต้นไม้เรา เป็นตัวเลข (0=Common ... 4=Legendary)
        int myTier = (int)seed.Rarity; 

        foreach (var candidateSeed in allSeeds)
        {
            // แปลง Rarity ของเมล็ดเป้าหมาย เป็นตัวเลข
            int targetTier = (int)candidateSeed.Rarity;
            
            // --- สูตรคำนวณครอบคลุม 5 ระดับ ---

            // 1. Base Weight (คะแนนพื้นฐาน): 80 คะแนน
            int weight = 80;

            // 2. Distance Penalty (ยิ่งห่างชั้น ยิ่งโอกาสน้อย):
            // ห่าง 1 ขั้น ลบ 20 คะแนน (เช่น ต้น Common(0) จะดรอป Rare(2) -> ห่าง 2 -> โดนลบ 40)
            int diff = Mathf.Abs(myTier - targetTier);
            weight -= (diff * 20);

            // 3. High Tier Bonus (ยิ่งต้นไม้เทพ ยิ่งบวกคะแนนเพิ่ม):
            // สูตร: เอา level ต้นไม้ * 15 คะแนน
            // Common(0) = +0
            // Uncommon(1) = +15
            // ...
            // Legendary(4) = +60 !! (ทำให้โอกาสได้ของดีพุ่งกระฉูด)
            
            // เงื่อนไข: จะได้โบนัสนี้เฉพาะเมื่อ เมล็ดเป้าหมาย level สูงพอๆ กับต้นไม้เรา (กันไม่ให้ต้นเทพไปดรอปของกากเยอะเกิน)
            if (targetTier >= myTier - 1) 
            {
                weight += (myTier * 15);
            }

            // 4. กันค่าติดลบ (ต่ำสุดให้ 1 คือยังมีโอกาสออก 1% ดีกว่าไม่ออกเลย)
            if (weight <= 0) weight = 1;

            weights.Add(weight);
            totalWeight += weight;
        }

        // --- สุ่มตามน้ำหนัก (เหมือนเดิม) ---
        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int currentSum = 0;

        for (int i = 0; i < allSeeds.Count; i++)
        {
            currentSum += weights[i];
            if (randomValue < currentSum)
            {
                return allSeeds[i];
            }
        }

        return null;
    }

    private void UpdateTimerUI(TimeSpan remainingTime)
    {
        if (dropingWaterCountText != null)
        {
            dropingWaterCountText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", 
                remainingTime.Hours, 
                remainingTime.Minutes, 
                remainingTime.Seconds);
        }
    }

    private void UpdateUIState()
    {
        if (dropingWaterCountText != null && !isGrowing)
        {
            if (currentState == TreeState.Withered)
                dropingWaterCountText.text = "Withered";
            else
                dropingWaterCountText.text = "Need Water";
        }
    }

    // --- Save / Load System ---

    private void SaveTargetTime()
    {
        // บันทึกเวลาที่จะเสร็จ เป็น String
        PlayerPrefs.SetString(treeID + "_FinishTime", finishTime.ToString());
        PlayerPrefs.SetInt(treeID + "_IsGrowing", 1);
        PlayerPrefs.Save();
    }

    private void SaveTreeStatus()
    {
        PlayerPrefs.SetInt(treeID + "_Stage", growthStage);
        PlayerPrefs.SetInt(treeID + "_State", (int)currentState);
        PlayerPrefs.Save();
    }

    private void LoadTreeStatus()
    {
        growthStage = PlayerPrefs.GetInt(treeID + "_Stage", 0);
        currentState = (TreeState)PlayerPrefs.GetInt(treeID + "_State", 0);
    }
    
    // เผื่อต้องการ Reset (ใช้ตอน Test)
    [ContextMenu("Reset Save Data")]
    public void ResetData()
    {
        PlayerPrefs.DeleteKey(treeID + "_FinishTime");
        PlayerPrefs.DeleteKey(treeID + "_IsGrowing");
        PlayerPrefs.DeleteKey(treeID + "_Stage");
        PlayerPrefs.DeleteKey(treeID + "_State");
        Debug.Log("Data Reset");
    }
}