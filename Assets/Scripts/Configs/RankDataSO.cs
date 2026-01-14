using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RankData", menuName = "GameData/RankData")]
public class RankDataSO : ScriptableObject
{
    [System.Serializable]
    public class PlayerRankData
    {
        public string playerName;
        public int level;
        public Sprite avatarIcon;
    }

    public List<PlayerRankData> playerRanks = new List<PlayerRankData>();

    public List<PlayerRankData> GetSortedRankList()
    {
        playerRanks.Sort((a, b) => b.level.CompareTo(a.level));
        return playerRanks;
    }

}