using UnityEngine;

[CreateAssetMenu(
    fileName = "BallSkin",
    menuName = "Game/Ball Skin"
)]
public class BallSkinData : ScriptableObject
{
    [Header("Identity")]
    public string ballId;
    public string displayName;

    [Header("Visuals")]
    public Sprite shopIcon;
    public Sprite gameplaySprite;

    [Header("Purchase")]
    public int price = 100;

    [Header("Starting State")]
    public bool unlockedByDefault;
}