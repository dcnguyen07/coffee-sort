using System.Collections.Generic;
using UnityEngine;

public class UILobbyRankList : MonoBehaviour
{
    [Header("References")]
    public RankDataSO rankDataSO;
    public Transform contentParent; 
    public RankItemUI rankItemPrefab;
    public RankItemUI rankItemMe; 
    public Sprite[] medalSprites; 

    private bool isInit;

    private void Awake()
    {
        isInit = false;
    }

    public void InitUI()
    {
        LoadRankList();
    }

    public void LoadRankList()
    {
        List<RankDataSO.PlayerRankData> sortedList = rankDataSO.GetSortedRankList();

        if (!isInit)
        {
            isInit = true;
            for (int i = 0; i < sortedList.Count; i++)
            {
                RankItemUI rankItem = Instantiate(rankItemPrefab, contentParent);

                Sprite medalSprite = (i < 3) ? medalSprites[i] : null;

                rankItem.SetupRankItem(i + 1, sortedList[i], medalSprite, false);

            }

        }
        RankDataSO.PlayerRankData meData = new RankDataSO.PlayerRankData();
        meData.playerName = "Me";
        meData.level = GameManager.Instance.GetCurrentLevel();
        meData.avatarIcon = sortedList[0].avatarIcon;

        int myRank = 1;

        for (int i = 0; i < sortedList.Count; i++)
        {
            if (meData.level <= sortedList[i].level)
                myRank++;
        }


        rankItemMe.SetupRankItem(myRank, meData, null, true);
    }
}