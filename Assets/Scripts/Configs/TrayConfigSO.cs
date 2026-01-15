
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
        public Color trayColorValue;
        public Color placeColorValue;
    }

    public List<TrayData> trayConfigs;

    public Color GetTrayColor(TrayColor color)
    {
        var data = trayConfigs.Find(t => t.color == color);
        return data != null ? data.trayColorValue : Color.white;
    }

    public Color GetPlaceColor(TrayColor color)
    {
        var data = trayConfigs.Find(t => t.color == color);
        return data != null ? data.placeColorValue : Color.white;;
    }

    public Color GetCupColor(TrayColor color)
    {
        return GetTrayColor(color);
    }
}