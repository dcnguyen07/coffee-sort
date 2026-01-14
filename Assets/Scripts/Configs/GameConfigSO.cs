using Objects;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Game Config")]
public class GameConfigSO : ScriptableObject
{
    [Header("Tray Prefabs")]
    public PlaceModel tray4Prefab;
    public PlaceModel tray6Prefab;
    public PlaceModel tray8Prefab;

    [Header("Board Slots")]
    public BoardSlot boardSlotPrefab;

    [Header("Board Settings")]
    public int mainBoardSlotCount = 7;

    [Header("Cup Prefab")]
    public CupModel cupPrefab;

    [Header("Card Board Box")]
    public GameObject cardBoardBoxPrefab;

    [Header("Special Element")]
    public SpecialElementModel specialElementPrefab;

    [Header("Confetti Effect")]
    public GameObject confettiPrefab;
}
