
using Managers;
using UnityEngine;

/// <summary>
/// Manage the cup on the tray
/// </summary>
public class CupModel : MonoBehaviour
{
    public SpriteRenderer cupSpriteRenderer;
    public int colorIndex { get; private set; }

    /// <summary>
    /// Update cup color based on color index
    /// </summary>
    /// <param name="colorIndex">Color index from config</param>
    public void SetCupColor(int colorIndex)
    {
        this.colorIndex = colorIndex;

        if (cupSpriteRenderer == null)
        {
            Debug.LogError("cupSpriteRenderer is null on CupModel");
            return;
        }

        TrayConfigSO config = GameManager.instance.trayConfigSo;
        if (config == null)
        {
            Debug.LogError("trayConfigSo is null on GameManager");
            return;
        }

        TrayConfigSO.TrayColor color = (TrayConfigSO.TrayColor)(this.colorIndex - 1);
        var cupSprite = config.GetCupSprite(color);

        if (cupSprite != null)
        {
            cupSpriteRenderer.sprite = cupSprite;
            // Apply configured tint from TrayConfigSO (colorValue)
            config.ApplyColorToSpriteRenderer(color, cupSpriteRenderer);
        }
        else
        {
            Debug.LogError($"Không tìm thấy sprite cốc cho màu {colorIndex}! / Cup sprite for color {colorIndex} not found!");
        }
    }

    public void SetSortingOrder(int order)
    {
        cupSpriteRenderer.sortingOrder = order;

        SpriteRenderer shadow = transform.Find("Shadow")?.GetComponent<SpriteRenderer>();
        if (shadow != null)
        {
            shadow.sortingOrder = order - 1;
        }
    }

    public void SetSortingOrderInJump()
    {
        cupSpriteRenderer.sortingOrder = 30000;

        SpriteRenderer shadow = transform.Find("Shadow")?.GetComponent<SpriteRenderer>();
        if (shadow != null)
        {
            shadow.sortingOrder = cupSpriteRenderer.sortingOrder - 1;
        }
    }
}