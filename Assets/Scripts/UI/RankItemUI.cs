using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RankItemUI : MonoBehaviour
{
    [Header("UI Components")]
    public Text uiRank;
    public Image uiMedal;
    public Image uiAvatar;
    public Text uiName;
    public Text uiCup;
    public Image uiRewardIcon;

    /// <summary>
    /// Update UI for each Rank Item
    /// </summary>
    public void SetupRankItem(int rank, RankDataSO.PlayerRankData playerData, Sprite medalSprite, bool isPlayer)
    {
        uiRank.text = rank.ToString();
        //Debug.Log("MY RANK " + rank);

        if (playerData != null)
        {
            uiAvatar.sprite = playerData.avatarIcon;
            uiName.text = playerData.playerName;
            uiCup.text = playerData.level.ToString();
        }

        if (medalSprite != null)
            uiMedal.sprite = medalSprite;
    }
}