using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // ต้องมี namespace นี้
using TMPro;

public class MainUI : MonoBehaviour
{
    [Header("Existing Data")]
    [SerializeField] private TMP_Text waterDropText;

    [Header("Tab System")]
    [SerializeField] private GameObject[] contentPages;
    [SerializeField] private Button[] tabButtons;
    [SerializeField] private Image[] tabIcons;
    [SerializeField] private TMP_Text[] tabTexts;
    [SerializeField] private GameObject[] tabUnderlines;

    [Header("Tab Appearance")]
    [SerializeField] private Color activeColor = new Color(0.12f, 0.63f, 0.35f);
    [SerializeField] private Color inactiveColor = Color.gray;

    [Header("Swipe Settings")]
    [SerializeField] private float minDistanceForSwipe = 50f;
    [SerializeField] private float maxTimeForSwipe = 1f; // เวลานานสุดที่จะนับว่าเป็นการปัด (กันการลากค้าง)

    // --- Input System Variables ---
    private InputAction pressAction;
    private InputAction positionAction;
    
    private Vector2 startPos;
    private float startTime;
    private int currentTabIndex = 1;

    private PlayerData player { get => PlayerData.Instance; }

    private void Awake()
    {
        SetupInputActions();
        FetchWaterDropText();
    }

    private void OnEnable()
    {
        // เปิดใช้งาน Input
        pressAction.Enable();
        positionAction.Enable();
    }

    private void OnDisable()
    {
        // ปิดใช้งาน Input
        pressAction.Disable();
        positionAction.Disable();
    }

    private void Start()
    {
        if (player != null)
        {
            player.onWaterDropChanged.AddListener(FetchWaterDropText);
        }

        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => SwitchTab(index));
        }

        SwitchTab(currentTabIndex);
    }

    // --- Input System Setup ---
    private void SetupInputActions()
    {
        // 1. สร้าง Action สำหรับการ "กด" (รองรับทั้ง Mouse Left Click และ Touch Press)
        pressAction = new InputAction(type: InputActionType.Button);
        pressAction.AddBinding("<Mouse>/leftButton");
        pressAction.AddBinding("<Touchscreen>/primaryTouch/press");

        // 2. สร้าง Action สำหรับ "ตำแหน่ง" (Mouse Position และ Touch Position)
        positionAction = new InputAction(type: InputActionType.Value, expectedControlType: "Vector2");
        positionAction.AddBinding("<Mouse>/position");
        positionAction.AddBinding("<Touchscreen>/primaryTouch/position");

        // 3. ผูก Event
        pressAction.started += OnSwipeStart;   // เมื่อเริ่มกด
        pressAction.canceled += OnSwipeEnd;    // เมื่อปล่อยมือ
    }

    // --- Swipe Logic ---
    private void OnSwipeStart(InputAction.CallbackContext context)
    {
        // อ่านค่าตำแหน่งปัจจุบันจาก Input System
        startPos = positionAction.ReadValue<Vector2>();
        startTime = Time.time;
    }

    private void OnSwipeEnd(InputAction.CallbackContext context)
    {
        // คำนวณเวลาที่ใช้ไป
        float timeSwipe = Time.time - startTime;
        if (timeSwipe > maxTimeForSwipe) return; // ถ้าลากนิ้วนานเกินไป ไม่นับว่าเป็นการปัด (อาจจะเป็นการ Drag ของ)

        Vector2 endPos = positionAction.ReadValue<Vector2>();
        Vector2 swipeVector = endPos - startPos;
        float distance = swipeVector.magnitude;

        if (distance > minDistanceForSwipe)
        {
            // เช็คว่าเป็นเเนวนอนมากกว่าแนวตั้ง
            if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
            {
                if (swipeVector.x > 0)
                {
                    OnSwipeRight(); // ปัดขวา (นิ้วไปทางขวา) -> ย้อนกลับ
                }
                else
                {
                    OnSwipeLeft(); // ปัดซ้าย (นิ้วไปทางซ้าย) -> ไปหน้าถัดไป
                }
            }
        }
    }

    private void OnSwipeLeft()
    {
        if (currentTabIndex < contentPages.Length - 1)
        {
            SwitchTab(currentTabIndex + 1);
        }
    }

    private void OnSwipeRight()
    {
        if (currentTabIndex > 0)
        {
            SwitchTab(currentTabIndex - 1);
        }
    }

    // --- Tab Switching Logic (เหมือนเดิม) ---
    public void SwitchTab(int index)
    {
        // ป้องกัน index หลุดขอบเขต
        if (index < 0 || index >= contentPages.Length) return;

        currentTabIndex = index;

        for (int i = 0; i < contentPages.Length; i++)
        {
            bool isActive = (i == currentTabIndex);

            // 1. จัดการ Content Page
            if (i < contentPages.Length && contentPages[i] != null) 
                contentPages[i].SetActive(isActive);

            // 2. จัดการ Icon (เพิ่มการเช็ค i < tabIcons.Length)
            if (i < tabIcons.Length && tabIcons[i] != null) 
                tabIcons[i].color = isActive ? activeColor : inactiveColor;

            // 3. จัดการ Text (เพิ่มการเช็ค i < tabTexts.Length)
            if (i < tabTexts.Length && tabTexts[i] != null) 
                tabTexts[i].color = isActive ? activeColor : inactiveColor;

            // 4. จัดการ Underline (เพิ่มการเช็ค i < tabUnderlines.Length)
            // ถ้าใน Inspector เป็น 0 โค้ดส่วนนี้จะข้ามไปเอง ไม่ Error
            if (i < tabUnderlines.Length && tabUnderlines[i] != null) 
                tabUnderlines[i].SetActive(isActive);
        }
    }

    private void FetchWaterDropText()
    {
        if (waterDropText != null && player != null)
            waterDropText.text = player.WaterDrops.ToString();
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.onWaterDropChanged.RemoveListener(FetchWaterDropText);
        }
        
        // อย่าลืม Dispose Action เมื่อ Object ถูกทำลาย
        pressAction?.Dispose();
        positionAction?.Dispose();
    }
}