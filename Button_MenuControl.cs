using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using LibrarySystem;
using GameManagement;

namespace Modular.UI {
    /// <summary>
    /// A modular button script that handles player input toward the GameUIManager.
    /// </summary>
    public class Button_MenuControl : MonoBehaviour
    {
        #region Variables
        [SerializeField]
        [Tooltip("Used to select which menu is affected by the button.")]
        private EnumLibrary.Menus targetMenu;

        [SerializeField]
        [Tooltip("If true, the menu will be enabled. If false, the menu will be disabled.")]
        private bool setActive;
        #endregion

        /// <summary>
        /// Enable or disable the target menu based on the set active variable.
        /// </summary>
        public void ToggleTargetMenu()
        {
            GameUIManager.Instance.ToggleMenu((int)targetMenu, setActive); //Shows the "Join Game" menu.
        }
    }
}