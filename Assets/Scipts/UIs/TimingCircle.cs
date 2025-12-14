using UnityEngine;
using UnityEngine.UI;
using TMPro; // ใช้สำหรับ Text แสดงคำว่า Perfect

public class TimingCircle : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private RectTransform outerRing; // วงแหวนวงนอก (สีแดงเส้นๆ)
    [SerializeField] private Button innerButton;      // ปุ่มวงใน (สีส้ม)
    [SerializeField] private TextMeshProUGUI feedbackText; // ข้อความ Perfect/Miss

    [Header("Settings")]
    [SerializeField] private float shrinkSpeed = 0.5f; // ความเร็วในการหด
    
    private float currentScale = 2.0f; // ขนาดเริ่มต้นของวงนอก (2 เท่าของวงใน)
    private bool isClicked = false;
    private System.Action<int> onScoreCalculated; // ส่งคะแนนกลับไปบอก Game Manager

    public void Setup(System.Action<int> scoreCallback)
    {
        this.onScoreCalculated = scoreCallback;
        
        // รีเซ็ตค่าต่างๆ
        currentScale = 2.0f;
        outerRing.localScale = Vector3.one * currentScale;
        feedbackText.text = ""; 
        
        innerButton.onClick.AddListener(OnClick);
    }

    private void Update()
    {
        if (isClicked) return;

        // ลดขนาดวงนอกลงเรื่อยๆ
        currentScale -= shrinkSpeed * Time.deltaTime;
        outerRing.localScale = Vector3.one * currentScale;

        // ถ้าหดจนเล็กกว่า 1 (วงใน) แปลว่า Miss (กดไม่ทัน)
        if (currentScale <= 0.8f) 
        {
            HandleClickResult("Miss", 0);
        }
    }

    private void OnClick()
    {
        if (isClicked) return;

        // คำนวณความแม่นยำ (ยิ่งใกล้ 1.0 ยิ่งแม่น)
        float diff = Mathf.Abs(currentScale - 1.0f);
        
        if (diff <= 0.15f) // แม่นมาก
        {
            HandleClickResult("Perfect!", 100);
        }
        else if (diff <= 0.4f) // พอใช้
        {
            HandleClickResult("Good", 50);
        }
        else // กดเร็วไป
        {
            HandleClickResult("Too Early", 10);
        }
    }

    private void HandleClickResult(string text, int score)
    {
        isClicked = true;
        feedbackText.text = text;
        
        // ส่งคะแนนกลับ
        onScoreCalculated?.Invoke(score);

        // ทำลายตัวเองทิ้งหลังจากแสดงผลแป๊บนึง
        Destroy(gameObject, 0.5f);
    }
}