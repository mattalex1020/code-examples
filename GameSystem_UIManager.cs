using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameSystem
{
    public class GameSystem_UIManager : MonoBehaviour
    {
        /// <summary>
        ///     This script is a singleton that manages ALL UI elements throughout the entire game.
        /// 
        ///     How to use: Place the script onto the "PERSISTENT" GameObject located in the title screen. 
        /// It will continue into any and all scenes, with the scenes automatically setting the relevant information on scene load.
        /// 
        /// </summary>

        #region Singleton Declaration
        public static GameSystem_UIManager Instance;
        private void Awake()
        {
            Instance = this;
        }
        #endregion

        #region Internal Classes
        [System.Serializable]
        internal class HotbarSlot
        {
            [SerializeField]
            internal Image image_activeBlueprintBackground; //The image used to indicate the slot is active.
            [SerializeField]
            internal Image image_blueprintPicture; //The visual icon of the blueprint in the slot.
            [SerializeField]
            internal TMP_Text text_blueprintCost; //The visual cost of the blueprint in the slot.
            [SerializeField]
            internal TMP_Text text_blueprintStackCount; //The visual stack amount of the current blueprint.
            [SerializeField]
            internal TMP_Text text_cooldownCounter;
            [SerializeField]
            internal bool bool_isActive; //Affects the activeSlotImage.
            }
        [System.Serializable]
        internal class EnhancerSlot
        {
            [SerializeField]
            internal TMP_Text text_enhancerModifier;
        }
        #endregion

        #region EXTERNAL CALL VARIABLES
        internal enum HUDUpdateType
        {
            ExpAndLevels,
            DeathStats
        }
        internal enum ScanType
        {
            Friendly,
            Neutral,
            Hostile
        }
        #endregion

        #region Start Menu



        #endregion

        #region PreGame






        #endregion

        #region HUD
        [Space(3)]
        [Header("HUD")]

        [Space]
        [SerializeField]
        private GameObject canvas_HUD;

        [Space]
        [Header("Window: Player Info")]

        [Space]
        [SerializeField]
        private TMP_Text text_playerName; //Defaults to Xilitian Warrior for now.
        [SerializeField]
        internal TMP_Text text_playerLevel;
        [SerializeField]
        private TMP_Text text_playerExperienceStatus;

        [Space(3)]
        [Header("Window: States and Enhancers")]

        [Space]
        [SerializeField]
        internal Image image_SprintingIcon;
        [SerializeField]
        internal Color color_activeSprint; // 2BA8A0
        [SerializeField]
        internal Color color_inactiveSprint; // 606060

        [Space]
        [SerializeField]
        internal List<EnhancerSlot> list_enhancerSlots;

        [Space(3)]
        [Header("Window: Entity Information")]

        [Space]
        [SerializeField]
        internal GameObject window_entityTarget;
        [SerializeField]
        internal TMP_Text text_entityName;
        [SerializeField]
        internal TMP_Text text_entityHealth;
        [SerializeField]
        internal Image image_entityHealthBar;
        [SerializeField]
        internal Image image_entityPicture;

        [Space(3)]
        [Header("Window: Game Status")]

        [Space]
        [SerializeField]
        private TMP_Text text_difficulty;
        [SerializeField]
        private TMP_Text text_difficultyRating;

        [Space(3)]
        [Header("Window: Crosshair")]

        [Space]
        [SerializeField]
        internal Image image_crosshair;
        [SerializeField]
        internal Color color_friendlyTarget; // 2AA128
        [SerializeField]
        internal Color color_neutralTarget; // FFFFFF
        [SerializeField]
        internal Color color_hostileTarget; // C53535

        internal bool targetIsFriendly;
        internal bool targetIsNeutral;
        internal bool targetIsHostile;

        [Space(3)]
        [Header("Window: Active Blueprint")]

        [Space]
        [SerializeField]
        internal TMP_Text text_activeBlueprintName;
        [SerializeField]
        internal TMP_Text text_activeBlueprintDescription;
        [SerializeField]
        internal Image image_activeBlueprintElementIcon;

        [Space(3)]
        [Header("Window: Vitality, Nanite, Flux Bars")]

        [Space]
        [SerializeField]
        internal Image image_vitalityBarFill;
        [SerializeField]
        internal TMP_Text text_vitalityAmount;
        [SerializeField]
        internal Image image_naniteBarFill;
        [SerializeField]
        internal TMP_Text text_naniteAmount;

        [Space(3)]
        [Header("Window: Hotbar")]

        [Space]
        [SerializeField]
        internal List<HotbarSlot> list_hotbarSlots; //How many slots we have
        [SerializeField]
        internal Color color_activeHotbarSlot; // 3DBCCC / The active color of the hotbar slot
        [SerializeField]
        internal Color color_inactiveHotbarSlot; // 787878 / The inactive color of the hotbar slot
        [SerializeField]
        internal Color color_cooldown;
        [SerializeField]
        internal Sprite sprite_emptyHotbarSlot; //The icon used on empty slots. Is set by default.
        






        #endregion

        #region Menus

        [Space(3)]
        [Header("Menu: Paused")]

        [Space]
        [SerializeField]
        private GameObject canvas_PauseMenu;
        internal bool pauseMenuIsActive;

        [Space(3)]
        [Header("Menu: Settings")]

        [Space]
        [SerializeField]
        private GameObject canvas_SettingsMenu;




        #endregion








        void Start()
        {

        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape)) //The input recieved here can possibly cause issues...need to avoid duplicate calls. 
            {
                TogglePauseMenu();
            }


            UpdateHUD();
        }

        private void UpdateHUD()
        {
            if(GameSystem_Progression.Instance == null)
            {
                return;
            }
            text_difficultyRating.text = "Difficulty: " + GameSystem_Progression.Instance.currentDifficultyRating;
            text_playerExperienceStatus.text = GameSystem_EntityManager.Instance.collectivePlayerExperience + " / " + GameSystem_EntityManager.Instance.experienceNeededToLevel;
        }

        internal void UpdateCrosshair(ScanType scanType)
        {
            if (scanType == ScanType.Friendly)
            {
                image_crosshair.color = color_friendlyTarget;
            }
            else if (scanType == ScanType.Neutral)
            {
                image_crosshair.color = color_neutralTarget;
            }
            else if (scanType == ScanType.Hostile)
            {
                image_crosshair.color = color_hostileTarget;
            }
            else
            {
                image_crosshair.color = color_neutralTarget;
            }

        }

        internal void TogglePauseMenu() //This only toggles the menu. It doesn't affect the game.
        {
            if(pauseMenuIsActive == true)
            {
                pauseMenuIsActive = false;
                canvas_PauseMenu.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                pauseMenuIsActive = true;
                canvas_PauseMenu.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        internal void ToggleSprintIcon(bool onOrOff)
        {
            if(onOrOff == true)
            {
                image_SprintingIcon.color = color_activeSprint;
            }
            else
            {
                image_SprintingIcon.color = color_inactiveSprint;
            }
        }


    }
}