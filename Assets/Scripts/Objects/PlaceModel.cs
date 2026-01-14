using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Managers;
using Objects;
using UnityEngine;

public class PlaceModel : MonoBehaviour
{
    [SerializeField] private int colorIndex;
    [SerializeField] private SpriteRenderer trayBackground;
    [SerializeField] private List<SpriteRenderer> cupPositionList;

    public int ColorIndex => colorIndex;
    public SpriteRenderer TrayBackground => trayBackground;
    public List<SpriteRenderer> CupPositionList => cupPositionList;

    public bool isCovered = false;
    public List<int> childIds = new List<int>();

    public int trayLayer;

    public int parentCount = 0;
    public PlaceModel parentTray;

    private int originalSortingOrder;
    private int originalCupSortingOrder;

    private bool[] filledHoles;

    public BoardSlot currentSlot;

    public Vector3 initPos;

    public Quaternion initRot;

    [Header("Tray ID")]
    public int trayId; 

    /// <summary>
    /// Update tray and cup visuals from TrayConfigSO in GameManager
    /// </summary>
    public void UpdateTrayVisual(int colorIndex)
    {
        if (!System.Enum.IsDefined(typeof(TrayConfigSO.TrayColor), (colorIndex - 1)))
        {
            Debug.LogError($"Color index {colorIndex} không hợp lệ! / Invalid color index {colorIndex}!");
            return;
        }

        TrayConfigSO.TrayColor color = (TrayConfigSO.TrayColor)(colorIndex - 1);
        var config = GameManager.instance.trayConfigSo;

        var traySprite = config.GetTraySprite(color);
        var placeSprite = config.GetPlaceSprite(color);

        if (traySprite != null)
        {
            trayBackground.sprite = traySprite;
        }

        if (placeSprite != null)
        {
            foreach (var cup in cupPositionList)
            {
                cup.sprite = placeSprite;
            }
        }

        this.colorIndex = colorIndex;
    }

    public void UpdateSortingOrder(int layer, int indexInLayer)
    {
        int baseOrder = layer * 100; 
        int orderOffset = indexInLayer * 2;

        if (trayBackground != null)
        {
            trayBackground.sortingOrder = baseOrder + orderOffset;
        }

        foreach (var cup in cupPositionList)
        {
            if (cup != null)
            {
                cup.sortingOrder = baseOrder + orderOffset + 1;
            }
        }
    }

    public void InitSortingOrder()
    {
        int baseOrder = trayLayer * 100;
        int orderOffset = trayId * 2;

        if (trayBackground != null)
        {
            trayBackground.sortingOrder = baseOrder + orderOffset;
        }

        foreach (var cup in cupPositionList)
        {
            if (cup != null)
            {
                cup.sortingOrder = baseOrder + orderOffset + 1;
            }
        }
    }

    public void SetDimmed(bool isDimmed)
    {
        Color trayColor = trayBackground.color;
        Color cupColor = cupPositionList[0].color;

        float dimFactor = isDimmed ? 0.6f : 1f;

        trayBackground.color = new Color(trayColor.r * dimFactor, trayColor.g * dimFactor, trayColor.b * dimFactor, trayColor.a);

        foreach (var cup in cupPositionList)
        {
            cup.color = new Color(cupColor.r * dimFactor, cupColor.g * dimFactor, cupColor.b * dimFactor, cupColor.a);
        }
    }

    public void UpdateCoverState(bool covered)
    {
        isCovered = covered;
        Color newColor = covered ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white; 
        trayBackground.color = newColor;

        foreach (var cupPos in cupPositionList)
        {
            cupPos.color = newColor;
        }
    }

    /// <summary>
    /// Set sorting order to top
    /// </summary>
    public void SetSortingOrderToTop()
    {
        originalSortingOrder = trayBackground.sortingOrder; 
        originalCupSortingOrder = cupPositionList[0].sortingOrder;

        trayBackground.sortingOrder = 10001;
        foreach (var cup in cupPositionList)
        {
            cup.sortingOrder = 10002;
        }
    }

    /// <summary>
    ///  Reset sorting order dựa vào layer
    /// </summary>
    public void ResetSortingOrder(int layer, int indexInLayer)
    {
        int baseOrder = layer * 100;
        int orderOffset = indexInLayer * 2;

        trayBackground.sortingOrder = baseOrder + orderOffset;
        foreach (var cup in cupPositionList)
        {
            cup.sortingOrder = baseOrder + orderOffset + 1;
        }
    }

    /// <summary>
    /// Update Sorting Order of the tray
    /// </summary>
    public void SetSortingOrder(int order)
    {
        trayBackground.sortingOrder = order;

        foreach (var hole in cupPositionList)
        {
            hole.sortingOrder = order - 1;
        }
    }

    /// <summary>
    /// Find the first available hole on the tray
    /// </summary>
    public Transform GetFirstEmptyHole()
    {
        for (int i = 0; i < cupPositionList.Count; i++)
        {
            if (!filledHoles[i])
            {
                return cupPositionList[i].transform;
            }
        }
        return null; 
    }

    /// <summary>
    /// Place cup into hole without changing hole sprite
    /// </summary>
    public void FillHole(Transform hole, CupModel cup)
    {
        int holeIndex = cupPositionList.FindIndex(c => c.transform == hole);

        if (holeIndex >= 0 && !filledHoles[holeIndex])
        {
            filledHoles[holeIndex] = true;
        }
    }

    /// <summary>
    ///  Finalize cup placement in the hole
    /// </summary>
    public void FinalizeCupPlacement(Transform hole, CupModel cup)
    {
        cup.transform.SetParent(hole);
        cup.transform.localPosition = Vector3.zero; 
    }

    /// <summary>
    /// Check if the tray is full
    /// </summary>
    public bool IsFull()
    {
        foreach (bool filled in filledHoles)
        {
            if (!filled) return false;
        }
        return true;
    }

    private void Awake()
    {
        filledHoles = new bool[cupPositionList.Count]; 
    }

    /// <summary>
    /// Check if the tray can accept a cup
    /// </summary>
    /// the cup to check</param>
    ///  True if the tray can accept the cup</returns>
    public bool CanAcceptCup(CupModel cup)
    {
        if (cup == null) return false;

        if (IsFull()) return false;

        return cup.colorIndex == this.colorIndex;
    }

    public void DestroyTrayWithEffect()
    {

        transform.SetParent(null);

     
        transform.DOMoveZ(transform.position.z - 1.0f, 0.2f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
  
            GameObject boxEffect = Instantiate(GameManager.instance.gameConfigSo.cardBoardBoxPrefab, transform.position, Quaternion.identity);
            boxEffect.transform.position = new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z + 0.2f);

            GameManager.instance.boardManager.RemoveTrayFromActiveList(this);

            Vector3 scaleFactor = Vector3.one;
            if (cupPositionList.Count == 4) scaleFactor = new Vector3(0.2f, 0.5f, 0.35f);
            else if (cupPositionList.Count == 6) scaleFactor = new Vector3(0.2f, 0.5f, 0.5f);
            else if (cupPositionList.Count == 8) scaleFactor = new Vector3(0.2f, 0.5f, 0.7f);

            boxEffect.transform.localScale = scaleFactor;

            Animator animator = boxEffect.GetComponent<Animator>();
            animator.Play("Scene");

            float animationTime = animator.GetCurrentAnimatorStateInfo(0).length;


            StartCoroutine(SpawnConfettiEffect(boxEffect, animationTime));

        });
    }

    /// <summary>
    /// Spawn Confetti Effect
    /// </summary>  
    private IEnumerator SpawnConfettiEffect(GameObject boxEffect, float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f);
        SoundManager.Instance.PlaySFX(SoundManager.Instance.trayFullClip);
        GameObject confettiEffect = Instantiate(GameManager.instance.gameConfigSo.confettiPrefab, boxEffect.transform.position, Quaternion.identity);
        confettiEffect.transform.SetParent(boxEffect.transform); 
        confettiEffect.transform.localPosition += new Vector3(0.0f, 2.5f, 0.0f);
        confettiEffect.transform.localRotation = Quaternion.Euler(0, 0, 0);

        // Play Particle System
        ParticleSystem confetti = confettiEffect.GetComponent<ParticleSystem>();
        confetti.Play();

        yield return new WaitForSeconds(confetti.main.duration); 

        StartCoroutine(MoveBoxAfterEffect(boxEffect, confettiEffect));
    }

    /// <summary>
    /// Move Box After Effect
    /// </summary>
    private IEnumerator MoveBoxAfterEffect(GameObject boxEffect, GameObject confettiEffect)
    {
       
        yield return new WaitForSeconds(0.1f);
        transform.SetParent(boxEffect.transform);
        SoundManager.Instance.PlaySFX(SoundManager.Instance.cardBoardGoClip);
      
        boxEffect.transform.DOMoveX(boxEffect.transform.position.x + 10f, 0.3f).SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                Destroy(confettiEffect); 
                Destroy(boxEffect);
            });
    }

    /// <summary>
    /// Sort all cups on the tray
    /// </summary>
    public void SortCupsOnTray()
    {
        int baseSortingOrder = 20000;

        for (int i = 0; i < cupPositionList.Count; i++)
        {
            Transform cupTransform = cupPositionList[i].transform;
            CupModel cup = cupTransform.GetComponentInChildren<CupModel>();

            if (cup != null)
            {
                int row = i / 2;
                int sortingOrder = baseSortingOrder + (row * 10);

                cup.SetSortingOrder(sortingOrder);
            }
        }
    }

    /// <summary>
    /// Check if the tray is covered by another tray
    /// </summary>
    public bool IsCovered()
    {
        return parentTray != null && parentTray.gameObject.activeSelf;
    }

    /// <summary>
    /// Update parent-child relationship when the tray moves
    /// </summary>
    public void SetParentTray(PlaceModel newParent)
    {
        parentTray = newParent;
    }

    /// <summary>
    /// Remove parent when moving out of stack
    /// </summary>
    public void ClearParent()
    {
        parentTray = null;
    }

    /// <summary>
    /// Find Special Element that this tray belongs to.
    /// </summary>
    public SpecialElementModel GetSpecialElementParent()
    {
        foreach (var specialElement in GameManager.instance.boardManager.specialElements.Values)
        {
            if (specialElement.ContainsTray(this)) 
            {
                return specialElement;
            }
        }
        return null;
    }

    /// <summary>
    /// Set parent for tray
    /// </summary>
    public void SetParent(PlaceModel parentTray)
    {
        this.parentTray = parentTray; 
        if (parentTray != null)
        {
            parentTray.childIds.Add(this.trayId);
        }
    }

    /// <summary>
    /// Check if the tray is in Special Element Stack
    /// </summary>
    public bool IsInSpecialElementStack()
    {
        return GetSpecialElementParent() != null;
    }

    /// <summary>
    /// Releases all cups from the tray and returns them to the queue
    /// </summary>
    public List<CupModel> ReleaseCups()
    {
        List<CupModel> releasedCups = new List<CupModel>();

        for (int i = 0; i < cupPositionList.Count; i++)
        {
            if (filledHoles[i])
            {
                CupModel cup = cupPositionList[i].GetComponentInChildren<CupModel>();
                if (cup != null)
                {
                    releasedCups.Add(cup);
                    cup.transform.SetParent(GameManager.instance.boardManager.cupPoolingRoot);
                    cup.transform.localPosition = Vector3.zero;
                    cup.transform.localScale = Vector3.one;
                    GameManager.instance.boardManager.cupInLevelList.Add(cup);
                    GameManager.instance.boardManager.remainCupsCount++;
                    GameManager.instance.boardManager.removedCupsCount--;
                }
                filledHoles[i] = false;
            }
        }

        return releasedCups;
    }

    /// <summary>
    /// Check if the tray is on the main board
    /// </summary>
    public bool IsOnBoard()
    {
        return currentSlot != null;
    }
}