using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // ถ้าใช้ TextMeshPro

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startMinigameButton;
    [SerializeField] private GameObject resultPanel; // หน้าต่างสรุปผล
    [SerializeField] private TextMeshProUGUI resultText; // ข้อความสรุปผล

    [Header("Mini Games")]
    // ลากเกมทั้งหมด (PopUpGame, WhackAMole, MatchCard) มาใส่ใน List นี้ใน Inspector
    [SerializeField] private List<MiniGameBase> miniGamesList;

    private void Awake()
    {
        startMinigameButton.onClick.AddListener(OnStartButtonClicked);
        resultPanel.SetActive(false);
    }

    private void OnStartButtonClicked()
    {
        // 1. ซ่อนปุ่มเริ่ม
        startMinigameButton.gameObject.SetActive(false);
        resultPanel.SetActive(false);

        // 2. สุ่มเกม
        PlayRandomGame();
    }

    private void PlayRandomGame()
    {
        if (miniGamesList.Count == 0) return;

        // สุ่ม Index
        int randomIndex = Random.Range(0, miniGamesList.Count);
        MiniGameBase selectedGame = miniGamesList[randomIndex];

        // ผูก Event รอรับตอนเกมจบ
        selectedGame.OnGameEnded += HandleGameEnded;

        // เริ่มเกม
        selectedGame.StartGame();
    }

    // ฟังก์ชันนี้จะถูกเรียกเมื่อ MiniGame จบลง
    private void HandleGameEnded(int score)
    {
        // 1. คำนวณ Reward
        int reward = CalculateReward(score);

        // 2. แสดงผล
        ShowResult(score, reward);

        // 3. เอา Event ออก (ป้องกัน Memory Leak)
        // ต้อง cast กลับไปหาตัวที่เพิ่งเล่นจบ (ในที่นี้ทำแบบง่ายคือ Loop เอาออกให้หมดก็ได้ หรือเก็บตัวแปร currentActiveGame ไว้)
        foreach(var game in miniGamesList)
        {
            game.OnGameEnded -= HandleGameEnded;
        }
        
        // 4. เปิดปุ่มให้เล่นใหม่ได้
        startMinigameButton.gameObject.SetActive(true);
    }

    private int CalculateReward(int score)
    {
        // ตัวอย่างสูตร: 10 คะแนน ได้ 1 เหรียญ
        return score / 10;
    }

    private void ShowResult(int score, int reward)
    {
        resultPanel.SetActive(true);
        resultText.text = $"Game Over!\nScore: {score}";
        resultText.text += $"\nYou earned {reward} Water Drops!";

        Player.Instance.AddWaterDrop(reward);
    }
}