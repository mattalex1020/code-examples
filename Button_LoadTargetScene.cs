using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using LibrarySystem;
using GameManagement;

namespace Modular.UI {
    /// <summary>
    /// A modular button script that handles player input into the GameSceneManager.
    /// </summary>
    public class Button_LoadTargetScene : MonoBehaviour
    {
        #region Variables
        [SerializeField]
        [Tooltip("Used to select which scene is loaded by the button.")]
        private EnumLibrary.CoreScenes targetScene;

        [SerializeField]
        [Tooltip("If true, scene will load asynchronously. If false, scene will load directly.")]
        private bool asynchLoading;
        #endregion

        /// <summary>
        /// Tells the GSM to replace the current scene on load. If asynchLoading is true, it will replace the scene upon completed loading in a different CPU thread.
        /// </summary>
        public void LoadSceneDirect()
        {
            int enumValue = (int)targetScene; //Convert the enum selection into an int.
            string sceneName = EnumLibrary.CoreScenes.GetName(typeof(EnumLibrary.CoreScenes), enumValue); //Get the string of the targetScene enum.
            GameSceneManager.Instance.DirectSceneLoad(sceneName, asynchLoading); //Replace the current scene with the target scene.

        }

        /// <summary>
        /// Tells the GSM to add the new scene to the game. If asynchLoading is true, it will add the scene using a different CPU thread.
        /// </summary>
        public void LoadSceneAdditive()
        {
            int enumValue = (int)targetScene; //Convert the enum selection into an int.
            string sceneName = EnumLibrary.CoreScenes.GetName(typeof(EnumLibrary.CoreScenes), enumValue); //Get the string of the targetScene enum.
            GameSceneManager.Instance.AdditiveSceneLoad(sceneName, asynchLoading); //Add the target scene to the game.

        }


    }
}