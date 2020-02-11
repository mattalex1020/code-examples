using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using Mirror;
using GameManagement;

namespace Modular.UI {
    /// <summary>
    /// A modular button script used to handle player input to the NetworkManager.
    /// </summary>
    public class Button_NetworkingActions : MonoBehaviour
    {
        #region Peer2Peer

        #region Variables

        #endregion



        /// <summary>
        /// Used by the start game button to launch a new session.
        /// </summary>
        public void StartHosting()
        {
            if (GameOverlord.Instance.netManager.isNetworkActive)
            {
                return; //No need to start on duplicate calls.
            }
            GameOverlord.Instance.netManager.StartHost();
        }

        /// <summary>
        /// Starts connection to the client specified by the address field.
        /// </summary>
        public void ConnectToHost()
        {
            GameOverlord.Instance.netManager.StartClient(); //Connect to the host and start a client session.
        }


        #endregion

        #region Dedicated
        //Planned in expansion or significant advancement of the project.
        #endregion
    }
}