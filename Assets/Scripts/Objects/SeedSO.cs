using UnityEngine;

[CreateAssetMenu(fileName = "NewSeed", menuName = "Seed")]
public class SeedSO : ScriptableObject
{
    [SerializeField] private string seedName;
    [SerializeField] private Sprite seedSprite;
    [SerializeField] private int growthDuration;
    [SerializeField] private SeedRarity rarity;

    public string SeedName => seedName;
    public Sprite SeedSprite => seedSprite;
    public int GrowthDuration => growthDuration;
    public SeedRarity Rarity => rarity;
    
    public enum SeedRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}
