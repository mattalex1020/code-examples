using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MyBox;
using LibrarySystem;
using Mirror;

namespace GameManagement {

    public class GameSceneManager : MonoBehaviour
    {
        #region Singleton Declaration
        public static GameSceneManager Instance; 

        void Awake()
        {
            if(Instance == null)
            {
                Instance = this; //Assign the first iteration of this script to Instance.
            }
            else
            {
                Debug.Log("Instance is already assigned. Duplicate of SceneManager detected on " + this.gameObject); //Log the duplicate.
                this.gameObject.name = this.gameObject.name + "_DuplicateSingletonDetected"; //Flag the object containing the duplicate.
			    this.gameObject.SetActive(false); //Disable the object instead of destroying it. 
            }
        }
        #endregion

        #region Private Variables

        private string newestScene = ""; //Used by the game to set the 'oldScene' when replacing this variable.
        private string oldScene = ""; //Used by the game to unload a scene when a new one is loaded additively.
        
        [SerializeField]
        [Tooltip("Scenes in this list will be used to randomly select the next scene during gameplay.")]
        private string[] availableGameSceneNames; //Used by the 'AddRandomScene' functiion to select the next game scene at random.


        #endregion

        #region Internal Variables
        internal bool readyToUnloadOldScene; //Used by the Overlord to determine players may move to a new scene. Set to true when the next scene is ready.

        #endregion



        #region Private Functions
        private void NewSceneIsReady(string newSceneName)
        {
            readyToUnloadOldScene = true; //Allows us to unload the last scene when ready and have moved to the new scene.
            oldScene = newestScene; //Stores a reference to the current scene as oldScene. Intended for game scenes only.
            newestScene = newSceneName; //Stores a reference to the most recent scene. 
        }

        


        #endregion

        #region Internal Functions
        /// <summary>
        /// This will unload ALL other scenes and ONLY load the target scene. This will typically going to be used for core scenes and not game scenes.
        /// </summary>
        /// <param name="sceneName">Name of the requested scene.</param>
        /// <param name="loadAsAsynch">If true, will use asynchronous loading. If false, will directly load the requested scene.</param>
        internal void DirectSceneLoad(string sceneName, bool loadAsAsynch)
        {
            if(loadAsAsynch == true)
            {
                StartCoroutine(LoadNewScene(sceneName, true)); //Asynchronously loads the requested scene and ONLY that scene.
            }
            else
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single); //Immediately loads ONLY the requested scene.
            }
        }
        /// <summary>
        /// This will add the requested scene to the game and setup variables needed to remove an older scene. This will typically be used for game scenes and not score scenes.
        /// </summary>
        /// <param name="sceneName">Name of the requested scene.</param>
        /// <param name="loadAsAsynch"></param>
        internal void AdditiveSceneLoad(string sceneName, bool loadAsAsynch)
        {
            if (loadAsAsynch == true)
            {
                StartCoroutine(LoadNewScene(sceneName, false)); //Asynchronously adds the requested scene to the game.
            }
            else
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Additive); //Immediately adds the requested scene to the game.
            }
        }
        /// <summary>
        /// Called by the Overlord when players defeat a boss. The Overlord will allow players to move to a new scene when it's finished loading.
        /// </summary>
        internal void AddRandomGameScene()
        {
            List<string> possibleGameScenes = new List<string>(); //Create a temporary list, starting with all the game scenes we can load.
            for(int i = 0; i < availableGameSceneNames.Length; i++)
            {
                if(availableGameSceneNames[i] != newestScene) //Prevent loading a duplicate scene by filtering it out based on the "newestScene."
                {
                    possibleGameScenes.Add(availableGameSceneNames[i]); //Add the scene name to the list.
                }
            }
            string[] filteredGameScenes = possibleGameScenes.ToArray(); //Convert the list to an array.

            int randomValue = Random.Range(0, filteredGameScenes.Length); //Get a random name from the new array of scene names.

            AdditiveSceneLoad(filteredGameScenes[randomValue], true); //Begin asynchronous loading of the new scene using the randomly selected name.
        }
        /// <summary>
        /// Called by the Overlord when players move to a new scene, leaving behind the old one.
        /// </summary>
        internal void RemoveOldGameScene()
        {
            SceneManager.UnloadSceneAsync(oldScene);
        }
        #endregion

        #region Coroutines
        /// <summary>
        /// Used for asynchronous loading.
        /// </summary>
        /// <param name="newScene">Name of requested scene.</param>
        /// <param name="modeIsSingle">If true, will load only the requested scene. If false, will add the scene to the game but not unload any others.</param>
        /// <returns></returns>
        private IEnumerator LoadNewScene(string newScene, bool modeIsSingle)
        {
            if(modeIsSingle == true)
            {
                AsyncOperation newSceneAsynch = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Single);

                while(!newSceneAsynch.isDone)
                {
                    yield return null;
                }

                NewSceneIsReady(newScene);
            }
            else
            {
                AsyncOperation newSceneAsynch = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);

                while (!newSceneAsynch.isDone)
                {
                    yield return null;
                }

                NewSceneIsReady(newScene);
            }

            yield break;
        }

        #endregion



        /// Utilizied like so:
        /// int enumValue = 2; // The value for which you want to get string 
        /// string enumName = Enum.GetName(typeof(EnumDisplayStatus), enumValue);
    }
}