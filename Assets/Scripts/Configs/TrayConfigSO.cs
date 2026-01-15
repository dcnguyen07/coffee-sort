
using System.Collections.Generic;
using UnityEngine;
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
        public Color colorValue;
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

    // Return the configured color for a TrayColor (fallback to white)
    public Color GetColor(TrayColor color)
    {
        var data = trayConfigs.Find(t => t.color == color);
        return data != null ? data.colorValue : Color.white;
    }

    // Apply color to a SpriteRenderer (for world-space sprites)
    public void ApplyColorToSpriteRenderer(TrayColor color, SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = GetColor(color);
    }

    // Apply color to a UI Image (for UI sprites)
    public void ApplyColorToImage(TrayColor color, Image image)
    {
        if (image == null) return;
        image.color = GetColor(color);
    }
}