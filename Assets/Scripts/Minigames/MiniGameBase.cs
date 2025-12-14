using UnityEngine;
using System;

// นี่คือแม่แบบ (Abstract Class)
public abstract class MiniGameBase : MonoBehaviour
{
    // Event ที่จะบอก GameManager ว่าเกมจบแล้วนะ พร้อมส่งคะแนน (Score) กลับไป
    public event Action<int> OnGameEnded;

    [SerializeField] protected GameObject gameUI; // UI ของเกมนั้นๆ (Canvas Panel)

    // ฟังก์ชันเริ่มเกม (ทุกเกมต้องมี)
    public virtual void StartGame()
    {
        gameUI.SetActive(true);
        // เขียน Logic การเริ่มเกมเพิ่มในลูกๆ
    }

    // ฟังก์ชันจบเกม
    protected virtual void EndGame(int finalScore)
    {
        gameUI.SetActive(false);
        // ส่งสัญญาณบอก GameManager ว่าจบแล้ว
        OnGameEnded?.Invoke(finalScore);
    }
}