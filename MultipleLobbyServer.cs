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
    public class MultipleLobbyServer : LobbyServerBase {
        public List<Lobby> lobbys;

        #region Inherited
        protected override void InitLobbyData() {
            lobbys = new List<Lobby>();
        }

        protected override Lobby FindAvailableLobby(int gameModeID) {
            return lobbys.Find((l) => l.gameModeID == gameModeID);
        }

        protected override Lobby FindLobbyByHostConnID(int hostConnectionID) {
            return lobbys.Find((l) => l.hostConnectionID == hostConnectionID);
        }

        

        protected override void AddLobby(string hostIP, int hostConnectionID, int gameModeID) {
            Lobby lobby = lobbys.Find((l) => l.hostConnectionID == hostConnectionID);
            if (lobby == null) {
                lobby = new Lobby();
                lobbys.Add(lobby);
            }

            lobby.hostIP = hostIP;
            lobby.hostConnectionID = hostConnectionID;
            lobby.gameModeID = gameModeID;
            lobby.hasHost = true;
        }


        protected override void RemoveLobby(Lobby lobby) {
            if (lobby == null)
                throw new ArgumentException("Cannot remove empty lobby");

            lobbys.Remove(lobby);
        } 
        #endregion
        
    }
}