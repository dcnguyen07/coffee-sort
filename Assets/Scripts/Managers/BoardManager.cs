using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Objects;
using UnityEngine;
using Components;

namespace Managers
{
    public class BoardManager : MonoBehaviour
    {
        private LevelDataSO currentLevel;
        public Transform waitingAreaParent;
        public Transform specialItemAreaParent;
        public Transform deleteAreaParent;
        public Transform cupPoolingRoot;
        private float waitingAreaOffset;

        public List<BoardSlot> boardSlots = new List<BoardSlot>();
        private PlaceModel selectedTray;

        private Dictionary<int, PlaceModel> spawnedTrays = new Dictionary<int, PlaceModel>();
        public Dictionary<int, SpecialElementModel> specialElements = new Dictionary<int, SpecialElementModel>();
        private List<PlaceModel> animPlaceList = new List<PlaceModel>();

        public Transform cupParent;
        private List<Transform> cupSpawnPoints = new List<Transform>();

        public List<CupModel> cupQueue = new List<CupModel>();
        public bool moveQueueCompleted = true;

        private int maxSlots = 7;
        private int availableSlots = 4;
        public int totalSlots = 7;
        public int unlockedSlots = 4;

        private readonly List<PlaceModel> activeTrays = new List<PlaceModel>();

        public TextMesh cupNumText;
        public TextMesh cupNumDesText;
        public Transform checkmarkImage;

        public Transform waitingArea;

        public LayerMask trayMask;

        public enum BoardState
        {
            Idle,
            Playing
        }
        public BoardState currentBoardState = BoardState.Idle;
        public BoosterButton orderBooster;
        public BoosterButton undoBooster;

        private void UpdateBoosterButtonStates()
        {
            if (currentBoardState == BoardState.Playing)
            {
                if (orderBooster != null) orderBooster.DisableBtn();
                if (undoBooster != null) undoBooster.DisableBtn();
            }
            else
            {
                if (orderBooster != null) orderBooster.EnableBtn();
                if (undoBooster != null) undoBooster.EnableBtn();
            }
        }

        /// <summary>
        /// Handle dragging tray from waiting area to main board
        /// </summary>
        private void HandleTrayDrag()
        {
            if (GameManager.instance.currentState != GameState.Playing)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                if (Camera.main != null)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit[] hits = Physics.RaycastAll(ray, 100.0f, trayMask);

                    PlaceModel tray = null;
                    PlaceModel topTray = null;
                    int highestLayer = -1;

                    if (hits.Length > 0)
                    {
                        foreach (RaycastHit hit in hits)
                        {
                            tray = hit.collider.GetComponent<PlaceModel>();
                            if (tray != null && !tray.isCovered)
                            {
                                if (tray.trayLayer > highestLayer && tray.currentSlot == null)
                                {
                                    topTray = tray;
                                    highestLayer = tray.trayLayer;
                                }
                            }

                        }
                        if (topTray != null)
                        {
                            topTray.SetSortingOrderToTop();
                            TryPlaceTrayInSlot(topTray);
                        }

                    }
                }
            }
        }

        /// <summary>
        ///  Generate the waiting area (Layers)
        /// </summary>
        private void GenerateWaitingArea()
        {
            animPlaceList.Clear();
            waitingAreaOffset = 0.5f;
            if (currentLevel == null)
            {
                Debug.LogError("Chưa gán LevelDataSO cho BoardManager! / LevelDataSO is not assigned!");
                return;
            }

            int indexInLayer = 0;
            foreach (var layerGroup in currentLevel.layers)
            {
                //Debug.LogError("Total tray " + layerGroup.trays.Count);
                foreach (var trayData in layerGroup.trays)
                {
                    PlaceModel trayPrefab = null;
                    switch (trayData.cupIds.Count)
                    {
                        case 4:
                            trayPrefab = GameManager.instance.gameConfigSo.tray4Prefab;
                            break;
                        case 6:
                            trayPrefab = GameManager.instance.gameConfigSo.tray6Prefab;
                            break;
                        case 8:
                            trayPrefab = GameManager.instance.gameConfigSo.tray8Prefab;
                            break;
                    }

                    if (trayPrefab == null)
                    {
                        Debug.LogError($"Không tìm thấy prefab khay phù hợp với số lỗ: {trayData.cupIds.Count} / Tray prefab not found for hole count: {trayData.cupIds.Count}");
                        continue;
                    }

                    Vector3 spawnPosition = new Vector3((trayData.posX - 0.5f * currentLevel.maxCol) * waitingAreaOffset, (trayData.posY - 0.5f * currentLevel.maxRow) * waitingAreaOffset, trayData.layer * 0.1f);
                    PlaceModel trayInstance = Instantiate(trayPrefab, spawnPosition, Quaternion.identity, waitingAreaParent);
                    trayInstance.transform.localPosition = spawnPosition;
                    trayInstance.initPos = spawnPosition;

                    if (trayData.angle == 0)
                        trayInstance.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                    else if (trayData.angle == 90)
                        trayInstance.transform.localRotation = Quaternion.Euler(0, 90, -90);
                    trayInstance.initRot = trayInstance.transform.localRotation;
                    trayInstance.trayId = trayData.id;
                    trayInstance.UpdateTrayVisual(trayData.cupIds[0]);
                    trayInstance.UpdateSortingOrder(trayData.layer, indexInLayer);
                    trayInstance.trayLayer = trayData.layer;

                    if (trayData.parentIds != null && trayData.parentIds.Count > 0)
                    {
                        trayInstance.SetDimmed(true);
                    }
                    spawnedTrays[trayData.id] = trayInstance;
                    animPlaceList.Add(trayInstance);
                    indexInLayer++;
                }
            }

            foreach (var layerGroup in currentLevel.layers)
            {
                foreach (var trayData in layerGroup.trays)
                {
                    if (trayData.parentIds != null)
                    {
                        foreach (var parentId in trayData.parentIds)
                        {
                            if (spawnedTrays.ContainsKey(parentId))
                            {
                                spawnedTrays[parentId].childIds.Add(trayData.id);
                                spawnedTrays[trayData.id].parentCount++;
                            }
                        }
                    }
                }
            }
            foreach (var trayData in currentLevel.layers.SelectMany(layer => layer.trays))
            {
                if (spawnedTrays.TryGetValue(trayData.id, out PlaceModel trayInstance))
                {
                    bool isCovered = trayData.parentIds != null && trayData.parentIds.Count > 0;
                    trayInstance.UpdateCoverState(isCovered);
                }
            }
        }

        /// <summary>
        /// Generate the main board slots
        /// </summary>
        private void GenerateMainBoardSlots()
        {
            boardSlots.Clear();
            int slotCount = GameManager.instance.gameConfigSo.mainBoardSlotCount;

            float slotSpacing = 1.15f;
            float startX = -(slotCount - 1) * slotSpacing / 2f;

            for (int i = 0; i < slotCount; i++)
            {
                Vector3 spawnPosition = new Vector3(startX + i * slotSpacing, 0, 0);
                BoardSlot slotInstance = Instantiate(GameManager.instance.gameConfigSo.boardSlotPrefab, spawnPosition, Quaternion.identity);
                slotInstance.transform.SetParent(deleteAreaParent);
                slotInstance.transform.localPosition = spawnPosition;
                slotInstance.transform.localRotation = Quaternion.Euler(0, 0, 0);
                slotInstance.ReleaseTray(true);
                slotInstance.SetSlotState(i < unlockedSlots);
                boardSlots.Add(slotInstance);
            }
        }

        /// <summary>
        /// Generate the complete level
        /// </summary>
        public void GenerateLevel()
        {
            currentBoardState = BoardState.Idle;
            maxSlots = 7;
            availableSlots = 4;
            totalSlots = 7;
            unlockedSlots = 4;
            processCupDone = true;
            moveQueueCompleted = true;
            removedCupsCount = 0;
            remainCupsCount = currentLevel.cups.Count;
            GenerateWaitingArea();
            GenerateMainBoardSlots();
            LoadCupSpawnPoints();
            LoadCupQueue();
            GenerateSpecialElements();
            UpdateCupLeftDisplay();
            PrepareGame();
            UpdateBoardState();
            UpdateBoosterButtonStates();
        }

        public void SetLevelData(LevelDataSO levelData)
        {
            if (levelData == null)
            {
                return;
            }
            currentLevel = levelData;
        }
        
        void Update()
        {
            HandleTrayDrag();
        }

        /// <summary>
        /// Find the first available slot and place the tray
        /// </summary>
        private void TryPlaceTrayInSlot(PlaceModel tray)
        {
            foreach (BoardSlot slot in boardSlots)
            {
                if (slot.IsAvailable())
                {
                    GameManager.instance.SaveActionForUndo(new UndoAction(tray, null, slot));
                    if (tray.IsInSpecialElementStack())
                    {
                        SpecialElementModel specialElement = tray.GetSpecialElementParent();
                        if (specialElement != null)
                        {
                            specialElement.ReleaseTopTray();
                        }
                    }
                    slot.LockSlotInTable();
                    tray.transform.DOKill();
                
                    // Set state to PLAYING when starting movement
                    currentBoardState = BoardState.Playing;
                    UpdateBoosterButtonStates();
                
                    tray.transform.DOLocalRotate(new Vector3(-90, 0, 0), 0.15f);
                    tray.transform.DOMove(slot.transform.position, 0.3f).SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            slot.SetOccupied(true, tray);
                            tray.currentSlot = slot;
                            tray.currentSlot.ReleaseTray(false);
                            UpdateTrayCoverState(tray);
                            activeTrays.Add(tray);
                        
                            // Set state to IDLE after movement completes
                            currentBoardState = BoardState.Idle;
                            UpdateBoosterButtonStates();
                        
                            if (moveQueueCompleted && processCupDone)
                            {
                                StartCoroutine(ProcessCupQueue());
                            }
                            slot.UnlockSlotInTable();
                            SoundManager.Instance.PlaySFX(SoundManager.Instance.trayPlacedClip);
                        });

                    return;
                }
            }
        }

        private void UpdateTrayCoverState(PlaceModel movedTray)
        {
            foreach (int childId in movedTray.childIds)
            {
                if (spawnedTrays.ContainsKey(childId))
                {
                    PlaceModel childTray = spawnedTrays[childId];
                    childTray.parentCount--;
                    if (childTray.parentCount == 0)
                    {
                        childTray.UpdateCoverState(false);
                    }
                }
            }
        }

        private void LoadCupSpawnPoints()
        {
            cupSpawnPoints.Clear();
            Transform pathParent = cupParent.Find("Path");

            if (pathParent != null)
            {
                foreach (Transform child in pathParent)
                {
                    cupSpawnPoints.Add(child);
                }
            }
            else
            {
                Debug.LogError("Path not found under Panel!");
            }
        }

        public List<CupModel> cupInLevelList = new List<CupModel>();

        /// <summary>
        /// Generate the initial cup queue
        /// </summary>
        private void LoadCupQueue()
        {
            cupInLevelList.Clear();
            // Debug.Log("TOTAL CUP " + currentLevel.cups.Count);
            //load cup form level
            for (int i = 0; i < currentLevel.cups.Count; i++)
            {
                int colorIndex = currentLevel.cups[i];
                CupModel newCup = Instantiate(GameManager.instance.gameConfigSo.cupPrefab);
                newCup.transform.localRotation = Quaternion.Euler(0, 0, 0);
                newCup.SetCupColor(colorIndex);
                newCup.transform.SetParent(cupPoolingRoot);
                newCup.transform.localPosition = Vector3.zero;
                cupInLevelList.Add(newCup);
            }

            cupQueue.Clear();
            Transform pathParent = cupParent.Find("Path");
            if (pathParent == null)
            {
                Debug.LogError("Không tìm thấy Path trong CupParent! / Path not found in CupParent!");
                return;
            }

            List<Transform> cupSpawnPoints = new List<Transform>();
            for (int i = 0; i < pathParent.childCount; i++)
            {
                cupSpawnPoints.Add(pathParent.GetChild(i));
            }

            int cupCount = Mathf.Min(23, currentLevel.cups.Count);

            for (int i = 0; i < cupCount; i++)
            {
                cupInLevelList[0].transform.SetParent(cupParent);
                cupInLevelList[0].transform.position = cupSpawnPoints[cupCount - 1].position;
                cupInLevelList[0].transform.localRotation = Quaternion.Euler(0, 0, 0);
                cupQueue.Add(cupInLevelList[0]);
                cupInLevelList.RemoveAt(0);
            }
        }

        [HideInInspector]
        public int removedCupsCount = 0;
        [HideInInspector]
        public int remainCupsCount = 0;
        private bool processCupDone;

        public IEnumerator ProcessCupQueue()
        {
            processCupDone = false;
            CupModel cup = cupQueue[0];

            BoardSlot targetSlot = FindSlotForCup(cup);
            if (targetSlot == null)
            {
                processCupDone = true;
                CheckOutOfSpace();
                yield break;
            }

            PlaceModel targetTray = targetSlot.placedTray;
            Transform emptyHole = targetTray.GetFirstEmptyHole();
            if (emptyHole == null)
            {
                processCupDone = true;
                CheckOutOfSpace();
                yield break;
            }

            if (targetTray.IsFull())
            {
                targetSlot.SetOccupied(false);
            }
            cupQueue.RemoveAt(0);
            removedCupsCount++;
            cup.transform.DOKill();
            cup.SetSortingOrder(10003);

            // Set state to PLAYING when cup starts moving
            currentBoardState = BoardState.Playing;
            UpdateBoosterButtonStates();

            cup.transform.DOScale(Vector3.one * 0.75f, 0.1f).SetEase(Ease.InOutQuad);
            Vector3 startPos = cup.transform.position;
            Vector3 midPoint = (startPos + emptyHole.position) / 2 + new Vector3(0, 0.0f, 1.0f);
            Vector3[] path = { startPos, midPoint, emptyHole.position };
            targetTray.FillHole(emptyHole, cup);
            cup.SetSortingOrderInJump();
            SoundManager.Instance.PlaySFX(SoundManager.Instance.cupJumpClip);
            bool cupAnimationFinished = false;
            remainCupsCount--;
            GameManager.instance.SaveActionForUndo(new UndoAction(targetSlot.placedTray, cup, targetSlot));
            cup.transform.DOPath(path, 0.2f, PathType.CatmullRom).SetEase(Ease.InOutCubic).OnComplete(() =>
            {
                UpdateCupLeftDisplay();
                targetTray.FinalizeCupPlacement(emptyHole, cup);
                targetTray.SortCupsOnTray();
                if (targetTray.IsFull())
                {
                    targetTray.DestroyTrayWithEffect();
                    targetSlot.SetOccupied(false);
                }
                cupAnimationFinished = true;

                // Set state to IDLE after cup movement completes
                currentBoardState = BoardState.Idle;
                UpdateBoosterButtonStates();
            });
            yield return new WaitUntil(() => cupAnimationFinished);
            MoveQueue();
        }

        private BoardSlot FindSlotForCup(CupModel cup)
        {
            foreach (BoardSlot slot in boardSlots)
            {
                if (!slot.IsAvailable() && slot.placedTray != null && slot.placedTray.CanAcceptCup(cup))
                {
                    return slot;
                }
            }
            return null;
        }

        private void MoveQueue()
        {
            moveQueueCompleted = false;
            currentBoardState = BoardState.Playing;  // Set state to PLAYING when queue starts moving
            UpdateBoosterButtonStates();

            for (int i = 0; i < cupQueue.Count; i++)
            {
                if (i == (cupQueue.Count - 1))
                    DoMoveCup(cupQueue[i], i, true);
                else
                    DoMoveCup(cupQueue[i], i, false);
            }
        }

        public void DoMoveCup(CupModel cup, int index, bool lastCup)
        {
            cup.transform.DOKill();
            cup.transform.DOMove(cupSpawnPoints[index].position, 0.1f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                if (lastCup)
                {
                    currentBoardState = BoardState.Idle;  // Set state to IDLE when queue movement completes
                    UpdateBoosterButtonStates();
                    MoreCupInQueue();
                }
            });
        }

        public void MoreCupInQueue()
        {

            if (cupQueue.Count < cupSpawnPoints.Count && cupInLevelList.Count > 0)
            {
                cupInLevelList[0].transform.SetParent(cupParent);
                cupInLevelList[0].transform.position = cupSpawnPoints[cupQueue.Count].position;
                cupInLevelList[0].transform.localRotation = Quaternion.Euler(0, 0, 0);
                cupQueue.Add(cupInLevelList[0]);
                cupInLevelList.RemoveAt(0);
            }

            if (cupQueue.Count == 0)
            {

            }

            moveQueueCompleted = true;
            StartCoroutine(ProcessCupQueue());
        }


        public void ShiftQueue(CupModel cup)
        {
            if (cupQueue.Count == 0)
            {
                return;
            }

            CupModel lastCup = cupQueue[cupQueue.Count - 1];
            cupQueue.RemoveAt(cupQueue.Count - 1);
            lastCup.transform.SetParent(cupPoolingRoot);
            lastCup.transform.localPosition = Vector3.zero;

            for (int i = cupQueue.Count - 1; i >= 0; i--)
            {
                cupQueue[i].transform.DOKill();
                cupQueue[i].transform.DOMove(cupSpawnPoints[i + 1].position, 0.3f).SetEase(Ease.OutQuad);
            }

            cupQueue.Insert(0, cup);
            cup.transform.SetParent(cupParent);
            cup.transform.position = cupSpawnPoints[0].position;
            cup.transform.localScale = Vector3.one;
            cup.transform.DOKill();
            cup.transform.DOMove(cupSpawnPoints[0].position, 0.3f).SetEase(Ease.OutQuad);
            remainCupsCount++;
            removedCupsCount--;
            UpdateCupLeftDisplay();
            //Debug.Log("Queue shifted successfully with new cup! remainCupsCount " + remainCupsCount);
        }


        public void ClearBoard()
        {

            foreach (var slot in boardSlots)
            {
                Destroy(slot.gameObject);
            }


            foreach (Transform child in waitingAreaParent)
            {
                Destroy(child.gameObject);
            }


            foreach (var cup in cupQueue)
            {
                Destroy(cup.gameObject);
            }
            cupQueue.Clear();


            foreach (var specialElement in specialElements.Values)
            {
                Destroy(specialElement.gameObject);
            }
            specialElements.Clear();
            activeTrays.Clear();
            boardSlots.Clear();

            UpdateAvailableSlots();

            GameManager.instance.SetGameState(GameState.Playing);
        }

        private void GenerateSpecialElements()
        {
            if (currentLevel.SpecialElementList == null) return;

            foreach (var specialElementData in currentLevel.SpecialElementList)
            {
                Vector3 spawnPosition = new Vector3((specialElementData.PosX - 0.5f * currentLevel.maxCol) * waitingAreaOffset, (specialElementData.PosY - 0.5f * currentLevel.maxRow) * waitingAreaOffset, spawnedTrays[specialElementData.LinkPlateId].trayLayer * 0.1f);
                SpecialElementModel specialElementModel = Instantiate(GameManager.instance.gameConfigSo.specialElementPrefab);
                specialElementModel.transform.SetParent(waitingAreaParent);
                specialElementModel.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                specialElementModel.transform.localPosition = spawnPosition;
                specialElementModel.transform.localScale = Vector3.one;

                specialElementModel.Initialize(specialElementData, spawnedTrays);
                specialElements[specialElementData.ID] = specialElementModel;
            }
        }
        
        public void HandleSpecialElementTap(int elementId)
        {
            if (specialElements.ContainsKey(elementId))
            {
                specialElements[elementId].OnTapCurrentPlate();
            }
        }

        public bool HasAvailableSlot()
        {
            return availableSlots > 0;
        }
        
        public void UnlockNewSlot()
        {
            if (availableSlots < maxSlots)
            {
                SoundManager.Instance.PlaySFX(SoundManager.Instance.newSlot);
                boardSlots[availableSlots].SetSlotState(true);
                availableSlots++;
                unlockedSlots++;
            }
        }

        public int GetAvailableSlots()
        {
            int count = 0;
            foreach (var slot in boardSlots)
            {
                if (slot.IsAvailable())
                    count++;
            }
            return count;
        }
        
        public void CheckOutOfSpace()
        {
            if (GetAvailableSlots() == 0)
            {
                SoundManager.Instance.PlaySFX(SoundManager.Instance.levelFail);
                GameManager.instance.SetGameState(GameState.GameOver);
                GameManager.instance.uiManager.uiGameFail.Show();
            }
        }

        /// <summary>
        /// Update available slot count
        /// </summary>
        public void UpdateAvailableSlots()
        {
            int availableCount = 0;
            foreach (var slot in boardSlots)
            {
                if (!slot.IsOccupied())
                {
                    availableCount++;
                }
            }
        }

        private void CheckLevelCompletion()
        {
            bool cupQueueEmpty = (cupQueue.Count == 0);
            bool allTraysProcessed = (activeTrays.Count == 0);
            bool allEffectsDone = !DOTween.IsTweening(this);
            bool noRemainingCups = (remainCupsCount == 0);
            if (cupQueueEmpty && allTraysProcessed && allEffectsDone && noRemainingCups)
            {
                Debug.Log("[BoardManager] Level completed, showing UI Game Success!");
                StartCoroutine(ShowGameSuccessUIWithDelay());
            }
        }
        private IEnumerator ShowGameSuccessUIWithDelay()
        {
            GameManager.instance.SetGameState(GameState.LevelComplete);
            yield return new WaitForSeconds(2.5f);
            SoundManager.Instance.PlaySFX(SoundManager.Instance.levelSuccess);
            GameManager.instance.uiManager.uiGameSuccess.InitUI();
            GameManager.instance.uiManager.uiGameSuccess.Show();
        }
        public void RemoveTrayFromActiveList(PlaceModel tray)
        {
            if (activeTrays.Contains(tray))
            {
                activeTrays.Remove(tray);
                CheckLevelCompletion();
            }
        }

        /// <summary>
        /// Update remaining cups display
        /// </summary>
        public void UpdateCupLeftDisplay()
        {
            if (cupNumText != null)
            {
                cupNumText.text = remainCupsCount.ToString();
                cupNumText.gameObject.SetActive(remainCupsCount > 0);
            }

            if (cupNumDesText != null)
            {
                cupNumDesText.gameObject.SetActive(remainCupsCount > 0);
            }

            if (checkmarkImage != null)
            {
                if (remainCupsCount == 0)
                {
                    checkmarkImage.localScale = Vector3.zero;
                    checkmarkImage.gameObject.SetActive(true);
                    checkmarkImage.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
                }
                else
                {
                    checkmarkImage.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Get the first locked slot on the board
        /// </summary>
        public BoardSlot GetFirstLockedSlot()
        {
            return boardSlots.FirstOrDefault(slot => slot.isSlotLockedByAds);
        }

        /// <summary>
        /// Get the count of unlocked slots
        /// </summary>
        public int GetUnlockedSlotCount()
        {
            return boardSlots.Count(slot => !slot.isSlotLockedByAds);
        }

        /// <summary>
        /// Get the count of remaining locked slots
        /// </summary>
        public int GetRemainingLockedSlots()
        {
            return boardSlots.Count(slot => slot.isSlotLockedByAds);
        }

        /// <summary>
        /// Get the slot occupied by a given tray
        /// </summary>
        public BoardSlot GetSlotByTray(PlaceModel tray)
        {
            foreach (var slot in boardSlots)
            {
                if (slot.placedTray == tray)
                {
                    return slot;
                }
            }
            return null;
        }


        private List<PlaceModel> waitingTrays = new List<PlaceModel>();

        /// <summary>
        /// Add tray back to waiting area on Undo
        /// </summary>
        public void AddToWaitingTrays(PlaceModel tray)
        {
            if (!waitingTrays.Contains(tray))
            {
                waitingTrays.Add(tray);
            }
        }

        /// <summary>
        /// Get the next available position in the queue for an undone cup
        /// </summary>
        public Vector3 GetWaitingPosition()
        {
            int queueCount = cupQueue.Count;

            if (queueCount < cupSpawnPoints.Count)
            {
                return cupSpawnPoints[queueCount].position;
            }

            return cupSpawnPoints[cupSpawnPoints.Count - 1].position;
        }


        public void UndoTrayMove(PlaceModel tray, BoardSlot previousSlot)
        {
            if (previousSlot != null)
            {
                previousSlot.SetOccupied(false);
            }

            if (activeTrays.Contains(tray))
            {
                activeTrays.Remove(tray);
            }
            
            tray.transform.DOLocalRotate(tray.initRot.eulerAngles, 0.3f).SetEase(Ease.InOutQuad);

            tray.transform.DOLocalMove(tray.initPos, 0.3f).SetEase(Ease.InOutQuad).OnComplete(() =>
            { 
                foreach (int childId in tray.childIds)
                {
                    if (spawnedTrays.ContainsKey(childId))
                    {
                        tray.currentSlot = null;
                        PlaceModel childTray = spawnedTrays[childId];
                        childTray.parentCount++;
                        if (childTray.parentCount > 0)
                        {
                            childTray.UpdateCoverState(true);
                        }
                    }
                }
                CheckLevelCompletion();
            });
        }

        public void ReturnCupToQueue(CupModel cup)
        {
            cup.transform.SetParent(cupParent);
            cup.transform.DOMove(GetWaitingPosition(), 0.3f).SetEase(Ease.OutQuad);
            cupQueue.Insert(0, cup);
            remainCupsCount++;
            removedCupsCount--;
            UpdateCupLeftDisplay();
        }


        public bool IsQueueIdle()
        {
            return !DOTween.IsTweening(this) && moveQueueCompleted && currentBoardState == BoardState.Idle;
        }

        /// <summary>
        /// Remove tray from the board when Undo is used
        /// </summary>
        public void ClearTrayFromBoard(PlaceModel tray)
        {
            BoardSlot occupiedSlot = GetSlotByTray(tray);
            if (occupiedSlot != null)
            {
                occupiedSlot.SetOccupied(false);
                occupiedSlot.placedTray = null;
            }
        }
        public Vector3 GetCupQueuePosition()
        {
            return cupSpawnPoints[0].position;
        }

        /// <summary>
        /// Reinsert a cup into the queue when undoing
        /// </summary>
        public void ReinsertCupIntoQueue(CupModel cup)
        {
            ShiftQueue(cup);
        }

        /// <summary>
        /// Get the Special Element linked to a tray
        /// </summary>
        public SpecialElementModel GetSpecialElementByTray(PlaceModel tray)
        {
            foreach (var specialElement in specialElements.Values)
            {
                if (specialElement.ContainsTray(tray))
                {
                    return specialElement;
                }
            }
            return null;
        }
        public void PrepareGame()
        {
            StartCoroutine(PrepareCups());
            StartCoroutine(PrepareTrays());
        }

        /// <summary>
        /// Check and update the board state based on current Tray and Cup movements
        /// </summary>
        private void UpdateBoardState()
        {
            bool isAnyTrayMoving = DOTween.IsTweening(waitingAreaParent) || 
                                   activeTrays.Any(tray => DOTween.IsTweening(tray.transform)) ||
                                   boardSlots.Any(slot => slot.placedTray != null && DOTween.IsTweening(slot.placedTray.transform));
            bool isAnyCupMoving = !moveQueueCompleted || !processCupDone || cupQueue.Any(cup => DOTween.IsTweening(cup.transform));
        
            if (isAnyTrayMoving || isAnyCupMoving)
            {
                currentBoardState = BoardState.Playing;
            }
            else
            {
                currentBoardState = BoardState.Idle;
            }
        }

        private IEnumerator PrepareCups()
        {
            float totalTime = 0.5f;
            float stepDelay = 0.05f;
            int startSoundOrder = 0;

            for (int i = 0; i < cupQueue.Count; i++)
            {
                CupModel cup = cupQueue[i];
                if (cup == null) continue;
                int steps = 22 - i;
                float stepTime = totalTime / Mathf.Max(steps, 1);
                Sequence cupSequence = DOTween.Sequence();
                for (int j = 22; j > i; j--)
                {
                    cupSequence.Append(cup.transform.DOMove(cupSpawnPoints[j - 1].position, stepTime).SetEase(Ease.Linear));
                }

                cupSequence.Play();
                startSoundOrder++;
                if (startSoundOrder % 3 == 0)
                    SoundManager.Instance.PlaySFX(SoundManager.Instance.startGameSfx);
                yield return new WaitForSeconds(stepDelay);
            }
        }
        private IEnumerator PrepareTrays()
        {
            float delayBetweenTrays = 0.025f;
            float scaleDuration = 0.5f;
            int index = 0;
            for (int i = 0; i < animPlaceList.Count; i++)
            {
                animPlaceList[i].transform.localScale = Vector3.zero;
                animPlaceList[i].transform.DOScale(Vector3.one, scaleDuration).SetEase(Ease.OutBack).SetDelay(index * delayBetweenTrays);
                index++;
            }
            yield return new WaitForSeconds(scaleDuration + (spawnedTrays.Count * delayBetweenTrays));
            GameManager.instance.currentState = GameState.Playing;
        }

    }
}

