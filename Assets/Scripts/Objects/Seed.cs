using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class Seed : MonoBehaviour
{
    private Button collectButton;
    public void Init(SeedSO data)
    {
        collectButton = GetComponent<Button>();
        collectButton.onClick.AddListener(() =>
        {
            Player.Instance.AddSeed(data, 1);
            Destroy(gameObject);
        });
    }
}
