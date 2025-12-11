using UnityEngine;

[CreateAssetMenu(fileName = "NewSeed", menuName = "Seed")]
public class SeedSO : ScriptableObject
{
    [SerializeField] private string seedName;
    [SerializeField] private Sprite seedSprite;
    [SerializeField] private int growingRate;

    public string SeedName => seedName;
    public Sprite SeedSprite => seedSprite;
    public int GrowingRate => growingRate;
}
