using System.Collections;
using UnityEngine;

public class TimingGameManager : MiniGameBase
{
    [Header("Game Settings")]
    [SerializeField] private GameObject timingCirclePrefab; // ลาก Prefab ที่มีสคริปต์ TimingCircle มาใส่
    [SerializeField] private RectTransform spawnArea;       // พื้นที่ที่จะให้ปุ่มเกิด
    [SerializeField] private float gameDuration = 20f;      // เวลาเล่นทั้งหมด
    [SerializeField] private float spawnRate = 1.0f;        // เกิดทุกๆ 1 วินาที

    private int totalScore = 0;
    private float timeRemaining;

    public override void StartGame()
    {
        base.StartGame();
        totalScore = 0;
        timeRemaining = gameDuration;
        
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        float spawnTimer = 0;

        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnRate)
            {
                SpawnCircle();
                spawnTimer = 0;
                // อาจจะเพิ่มความยากโดยลด spawnRate ลงเรื่อยๆ ก็ได้
            }

            yield return null;
        }

        // จบเกม รอแป๊บนึงให้ปุ่มที่ค้างอยู่หายไป (ถ้าต้องการ) แล้วส่งคะแนน
        yield return new WaitForSeconds(1f);
        EndGame(totalScore);
    }

    private void SpawnCircle()
    {
        // สร้างปุ่มขึ้นมา
        GameObject obj = Instantiate(timingCirclePrefab, spawnArea);
        
        // สุ่มตำแหน่ง
        float w = spawnArea.rect.width;
        float h = spawnArea.rect.height;
        float x = Random.Range(-w / 2 + 50, w / 2 - 50); // +50 เพื่อไม่ให้ชิดขอบเกินไป
        float y = Random.Range(-h / 2 + 50, h / 2 - 50);
        
        obj.transform.localPosition = new Vector3(x, y, 0);

        // ตั้งค่าปุ่ม และผูกฟังก์ชันรับคะแนน
        TimingCircle circle = obj.GetComponent<TimingCircle>();
        if (circle != null)
        {
            circle.Setup((score) => {
                totalScore += score;
            });
        }
    }
}