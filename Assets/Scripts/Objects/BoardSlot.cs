using UnityEngine;

namespace Objects {

    public class BoardSlot : MonoBehaviour
    {
        public GameObject lockIcon;
        public GameObject warnBorder;
        public BoxCollider clickAreaCollider;
        private bool isOccupied = false; 
        public bool isSlotLocked = false;
        public bool isSlotLockedByAds = false;
        private bool isReleaseTray = true;
        public PlaceModel placedTray;

        /// <summary>
        /// Check if slot is available
        /// </summary>
        public bool IsAvailable()
        {
            return !isOccupied && !isSlotLocked && !isSlotLockedByAds;
        }

        /// <summary>
        /// Place tray in slot
        /// </summary>
        public void SetOccupied(bool occupied)
        {
            isOccupied = occupied;
        }
        /// <summary>
        ///Set tray on slot
        /// </summary>
        public void SetOccupied(bool occupied, PlaceModel tray = null)
        {
            isOccupied = occupied;
            placedTray = tray;
        }

        /// <summary>
        /// Check if slot is occupied
        /// </summary>
        public bool IsOccupied()
        {
            return isOccupied;
        }

        /// <summary>
        /// Place tray in slot
        /// </summary>
        public void ReleaseTray(bool releaseTray)
        {
            isReleaseTray = releaseTray;
        }

        /// <summary>
        /// Toggle lock icon
        /// </summary>
        public void SetLockAds(bool isLocked)
        {
            lockIcon.SetActive(isLocked);
        }

        /// <summary>
        /// Toggle warning border / Viền cảnh báo
        /// </summary>
        public void SetWarning(bool isWarning)
        {
            warnBorder.SetActive(isWarning);
        }

        /// <summary>
        ///  Enable/Disable slot interaction
        /// </summary>
        public void SetInteractable(bool isInteractable)
        {
            clickAreaCollider.enabled = isInteractable;
        }

        public void LockSlotAds()
        {
            isSlotLockedByAds = true;
            SetSlotState(true);
        }

        public void UnlockSlotAds()
        {
            isSlotLockedByAds = false;
            SetSlotState(true);
        }

        public void LockSlotInTable()
        {
            isSlotLocked = true;
        }

        public void UnlockSlotInTable()
        {
            isSlotLocked = false;
        }

        /// <summary>
        /// Set slot state (lock/unlock)
        /// </summary>
        public void SetSlotState(bool unlocked)
        {
            isSlotLockedByAds = !unlocked;
            lockIcon.SetActive(!unlocked);
        }

    }
}