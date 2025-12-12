using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // จำเป็นสำหรับการใช้ DateTime

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
    [SerializeField] private float growthDuration = 10f; // เวลาเติบโต (วินาที)
    [SerializeField, Range(0, 100)] private float dropChance = 50f;

    [Header("Status")]
    [SerializeField] private TreeState currentState = TreeState.Sapling;
    [SerializeField] private int growthStage = 0;
    [SerializeField] private bool isGrowing = false; 

    // Public Properties
    public bool IsGrowing => isGrowing; // เปลี่ยนให้คนอื่นเช็คได้ว่ากำลังโตไหม

    [Header("UI & References")]
    [SerializeField] private Button wateringButton;
    [SerializeField] private TMP_Text dropingWaterCountText;

    // Internal Variables
    private DateTime finishTime; // เวลาที่จะโตเสร็จ (เวลาจริง)
    private bool isTimerRunning = false; // ตัวคุม Update ไม่ให้ทำงานฟรีๆ

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
                if(!isGrowing && currentState != TreeState.Withered)
                {
                    Player.Instance.WateringTree(this);
                }
            });
        }
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
        finishTime = DateTime.Now.AddSeconds(growthDuration);
        
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
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        if (randomValue <= dropChance)
        {
            Debug.Log($"Offline/Online Reward: Got Seed from {seed.name}");
        }
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