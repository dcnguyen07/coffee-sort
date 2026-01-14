using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TrayConfig", menuName = "Game/Tray Config")]
public class TrayConfigSO : ScriptableObject
{
    public enum TrayColor
    {
        Green,
        Red,
        Orangle,
        Yellow,
        Brown,
        Pink,
        Purple,
        Blue,
        White
    }

    [System.Serializable]
    public class TrayData
    {
        public TrayColor color;
        public Sprite traySprite;
        public Sprite placeSprite;
        public Sprite cupSprite;
    }

    public List<TrayData> trayConfigs;

    public Sprite GetTraySprite(TrayColor color)
    {
        var data = trayConfigs.Find(t => t.color == color);
        return data != null ? data.traySprite : null;
    }

    public Sprite GetPlaceSprite(TrayColor color)
    {
        var data = trayConfigs.Find(t => t.color == color);
        return data != null ? data.placeSprite : null;
    }

    public Sprite GetCupSprite(TrayColor color)
    {
        var data = trayConfigs.Find(t => t.color == color);
        return data != null ? data.cupSprite : null;
    }
}
