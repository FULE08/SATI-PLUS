using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System; // จำเป็นสำหรับ Action Callback

public class BreathingController : MonoBehaviour
{
    public static BreathingController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject breathingPanel; // ตัวหน้าต่างทั้งหมด
    [SerializeField] private Image totalTimeBar; // หลอดสีฟ้าด้านบน (นับถอยหลังรวม)
    [SerializeField] private TMP_Text timerText; // เลข 30
    [SerializeField] private TMP_Text instructionText; // คำว่า Breathe in / out

    [Header("Rhythm Visuals")]
    [SerializeField] private RectTransform circleTransform; // วงกลมตรงกลาง
    [SerializeField] private Image rhythmBar; // หลอดสีเหลือง (ตัวกำกับจังหวะ)
    [SerializeField] private Button closeButton; // ปุ่ม X

    [Header("Settings")]
    [SerializeField] private float breatheInDuration = 4f; // เวลาหายใจเข้า (วินาที)
    [SerializeField] private float breatheOutDuration = 4f; // เวลาหายใจออก (วินาที)
    [SerializeField] private float maxCircleScale = 1.5f; // ขนาดวงกลมตอนขยายสุด
    [SerializeField] private float minCircleScale = 1.0f; // ขนาดวงกลมตอนหดสุด

    private Action onBreathingComplete; // Callback เมื่อจบเวลา
    private Coroutine breathingCoroutine;

    private void Awake()
    {
        closeButton.onClick.AddListener(CancelBreathing);
        breathingPanel.SetActive(false);

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ฟังก์ชันเรียกใช้จาก GameManager
    public void StartBreathingSession(float duration, Action onComplete)
    {
        breathingPanel.SetActive(true);
        onBreathingComplete = onComplete;
        
        if (breathingCoroutine != null) StopCoroutine(breathingCoroutine);
        breathingCoroutine = StartCoroutine(BreathingRoutine(duration));
    }

    private IEnumerator BreathingRoutine(float totalDuration)
    {
        float timer = totalDuration;
        float cycleTimer = 0f; // ตัวนับเวลาสำหรับจังหวะหายใจเข้า-ออก

        while (timer > 0)
        {
            // 1. อัปเดตเวลาหลัก (หลอดฟ้าและตัวเลข)
            timer -= Time.deltaTime;
            timerText.text = Mathf.CeilToInt(timer).ToString(); // ปัดเศษขึ้นให้ดูสวย
            totalTimeBar.fillAmount = timer / totalDuration;

            // 2. คำนวณจังหวะหายใจ (Cycle)
            // คำนวณว่าตอนนี้อยู่ในช่วง หายใจเข้า หรือ ออก
            float fullCycle = breatheInDuration + breatheOutDuration;
            float currentCycleTime = cycleTimer % fullCycle;

            if (currentCycleTime < breatheInDuration)
            {
                // --- ช่วงหายใจเข้า (Breathe In) ---
                instructionText.text = "Breathe in";
                
                // คำนวณ % ความคืบหน้า (0 ถึง 1)
                float progress = currentCycleTime / breatheInDuration;
                
                // ขยายวงกลม
                float currentScale = Mathf.Lerp(minCircleScale, maxCircleScale, progress);
                circleTransform.localScale = Vector3.one * currentScale;

                // เพิ่มหลอดเหลือง
                rhythmBar.fillAmount = progress;
            }
            else
            {
                // --- ช่วงหายใจออก (Breathe Out) ---
                instructionText.text = "Breathe out";

                // คำนวณ % ความคืบหน้า (0 ถึง 1) ของช่วงหายใจออก
                float progress = (currentCycleTime - breatheInDuration) / breatheOutDuration;

                // หดวงกลม
                float currentScale = Mathf.Lerp(maxCircleScale, minCircleScale, progress);
                circleTransform.localScale = Vector3.one * currentScale;

                // ลดหลอดเหลือง
                rhythmBar.fillAmount = 1 - progress;
            }

            cycleTimer += Time.deltaTime;
            yield return null;
        }

        // จบการทำงาน
        EndSession();
    }

    private void EndSession()
    {
        breathingPanel.SetActive(false);
        // แจ้งกลับไปที่ GameManager ว่าเสร็จแล้วนะ
        onBreathingComplete?.Invoke();
    }

    private void CancelBreathing()
    {
        if (breathingCoroutine != null) StopCoroutine(breathingCoroutine);
        breathingPanel.SetActive(false);
        // กรณีปิด user อาจจะอยากกลับหน้าเมนู หรือข้ามไปเลย (ในที่นี้ให้กลับเมนูละกันครับ)
        // หรือถ้าอยากให้ข้ามไปเล่นเกมเลย ก็เรียก onBreathingComplete?.Invoke();
    }
}