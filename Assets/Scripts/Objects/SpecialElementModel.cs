using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Objects
{
    public class SpecialElementModel : MonoBehaviour
    {
        [Header("References")]
        public Transform root;
        public SpriteRenderer bottomSprite;
        public SpriteRenderer topSprite;
        public SpriteRenderer shadowSprite;
        public TextMesh cupNumText;

        private List<int> linkedPlateIds = new List<int>();
        private int currentPlateId;
        private int nextPlateIndex = 0;
        private Dictionary<int, PlaceModel> trayDictionary;
        public List<PlaceModel> linkedTrays = new List<PlaceModel>();
        private PlaceModel activeTray;

        public int id;
        public int linkPlateId;
        public List<int> linkPlateIdList = new List<int>();
        /// <summary>
        /// Initialize Special Element with data from LevelDataSO
        /// </summary>
        public void Initialize(SpecialElement data, Dictionary<int, PlaceModel> trayDict)
        {
            id = data.ID;
            linkPlateId = data.LinkPlateId;
            linkPlateIdList = new List<int>(data.LinkPlateIdList);

            linkedTrays.Clear();

            if (trayDict.ContainsKey(linkPlateId))
            {
                activeTray = trayDict[linkPlateId];
                linkedTrays.Add(activeTray);
            }

            foreach (int plateId in linkPlateIdList)
            {
                if (trayDict.ContainsKey(plateId))
                {
                    linkedTrays.Add(trayDict[plateId]);
                }
            }

            if (linkedTrays.Count > 0)
            {
                activeTray.SetDimmed(false);
                for (int i = 1; i < linkedTrays.Count; i++)
                {
                    linkedTrays[i].SetDimmed(true);
                }
            }
            UpdateSortingOrder();
            UpdateSpecialItemRotation();
            UpdateCupNumText();
        }

        /// <summary>
        /// Update the number of remaining trays displayed on the UI
        /// </summary>
        private void UpdateCupNumText()
        {
            cupNumText.text = (linkedTrays.Count - 1).ToString();
            cupNumText.gameObject.SetActive(linkedTrays.Count > 1);
        }

        /// <summary>
        /// Handle when tapping the current plate
        /// </summary>
        public void OnTapCurrentPlate()
        {
            if (!trayDictionary.ContainsKey(currentPlateId)) return;

            PlaceModel currentTray = trayDictionary[currentPlateId];

            if (!currentTray.IsCovered())
            {
                trayDictionary.Remove(currentPlateId);
                Destroy(currentTray.gameObject);

                if (linkedPlateIds.Count > 0)
                {
                    currentPlateId = linkedPlateIds[0];
                    linkedPlateIds.RemoveAt(0);

             
                    PlaceModel newTray = trayDictionary[currentPlateId];
                    newTray.SetParentTray(null);
                    UpdateParentChildRelation();
                }

                UpdateCupNumText();
            }
        }

        /// <summary>
        /// Update Parent-Child relationship between trays in stack
        /// </summary>
        private void UpdateParentChildRelation()
        {
            if (linkedPlateIds.Count > 0)
            {
                int newParentId = currentPlateId;
                foreach (int childId in linkedPlateIds)
                {
                    if (trayDictionary.ContainsKey(childId))
                    {
                        trayDictionary[childId].transform.SetParent(trayDictionary[newParentId].transform);
                    }
                    newParentId = childId;
                }
            }
        }

        public void ReleaseTopTray()
        {
            if (linkedTrays.Count == 0) return;
       
            PlaceModel removedTray = linkedTrays[0];
            linkedTrays.RemoveAt(0); 


            if (linkedTrays.Count > 0)
            {
                PlaceModel newMainTray = linkedTrays[0];
                linkedTrays.RemoveAt(0);

                activeTray = newMainTray;
                linkPlateId = newMainTray.trayId;
                newMainTray.SetDimmed(false); 

                newMainTray.transform.DOMove(removedTray.transform.position, 0.3f).SetEase(Ease.OutQuad);

                foreach (var tray in linkedTrays)
                {
               
                }

                linkedTrays.Insert(0, newMainTray); 
                                               
                UpdateSortingOrder();
            }
            else
            {
                activeTray = null;
                linkPlateId = -1;
            }

            UpdateCupNumText();
        }
        /// <summary>
        /// Check if the tray belongs to this Special Element
        /// </summary>
        public bool ContainsTray(PlaceModel tray)
        {
            return linkedTrays.Contains(tray) || (activeTray != null && activeTray == tray);
        }

        public void UpdateSortingOrder()
        {

            bottomSprite.sortingOrder = 10;

            for (int i = 0; i < linkedTrays.Count; i++)
            {
           
            }

     
            if (activeTray != null)
            {
          
            }

            topSprite.sortingOrder = 10000;
            UpdateTextSortingOrder();
        }

        /// <summary>
        /// Update the direction of the Special Item so that the opening direction is towards the main tray.
        /// </summary>
        private void UpdateSpecialItemRotation()
        {
            if (activeTray == null) return;

            Vector3 directionToTray = activeTray.transform.position - transform.position;
            float angle = Mathf.Atan2(directionToTray.z, directionToTray.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0, -angle + 90, 0);
        }

        /// <summary>
        /// Update Sorting Order for TextMeshPro to always be on top.
        /// </summary>
        private void UpdateTextSortingOrder()
        {
            MeshRenderer textRenderer = cupNumText.GetComponent<MeshRenderer>();
            if (textRenderer != null)
            {
                textRenderer.sortingLayerName = "Effect";
                textRenderer.sortingOrder = 100; 
            }
        }
    }
}