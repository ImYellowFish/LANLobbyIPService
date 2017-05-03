using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;
using System;

namespace LANLobbyIPService {

    /// <summary>
    /// Only support 1 lobby at the same time
    /// the first to join will be assigned as host
    /// client will be sent to this lobby automatically
    /// </summary>
    public class SingleLobbyServer : LobbyServerBase {
        public Lobby lobby;

        #region Inherited
        // ignore gameModeID as there is only one lobby
        protected override Lobby FindAvailableLobby(int gameModeID) {
            if (lobby.hasHost) {
                return lobby;
            }
            return null;
        }

        protected override Lobby FindLobbyByHostConnID(int hostConnID) {
            if (lobby.hostConnectionID == hostConnID)
                return lobby;
            return null;
        }

        protected override void AddLobby(string hostIP, int hostConnectionID, int gameModeID) {
            lobby.hostIP = hostIP;
            lobby.hostConnectionID = hostConnectionID;
            lobby.hasHost = true;
            lobby.gameModeID = gameModeID;
        }

        protected override void RemoveLobby(Lobby lobby) {
            lobby.hostIP = "";
            lobby.hostConnectionID = -1;
            lobby.hasHost = false;
            lobby.gameModeID = 0;
        }

        protected override void InitLobbyData() {
            lobby = new Lobby();
            RemoveLobby(lobby);
        }

        #endregion
        
    }
}