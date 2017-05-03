using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;

namespace LANLobbyIPService {
    [System.Serializable]
    public class Lobby {
        public string hostIP;
        public int hostConnectionID;
        public bool hasHost;
        public int gameModeID;
    }

    /// <summary>
    /// Only support 1 lobby at the same time
    /// the first to join will be assigned as host
    /// client will be sent to this lobby automatically
    /// </summary>
    public abstract class LobbyServerBase : MonoBehaviour {
        public int port = 7778;
        public bool debug;

        [Header("readonly")]
        public string serverIP;
        public List<int> connectionIDs;

        protected virtual void Start() {
            bool result = Listen(port);
            if (!result)
                Debug.LogError("Cannot start server!");
            else {
                serverIP = Network.player.ipAddress;
            }

            AddServerCallbacks();
            connectionIDs = new List<int>();

            InitLobbyData();
        }


        #region Abstract functions
        /// <summary>
        /// init lobby info. called at server start.
        /// </summary>
        protected abstract void InitLobbyData();

        /// <summary>
        /// Given the gameModeID, returns an available lobby to join
        /// return null if no available lobby is found.
        /// </summary>
        /// <param name="gameModeID"></param>
        /// <returns></returns>
        protected abstract Lobby FindAvailableLobby(int gameModeID);


        /// <summary>
        /// Given the hostID, returns a lobby which has this hostID.
        /// </summary>
        /// <param name="hostConnID"></param>
        /// <returns></returns>
        protected abstract Lobby FindLobbyByHostConnID(int hostConnID);

        /// <summary>
        /// add a new lobby with acquired host info to lobby storage.
        /// </summary>
        /// <param name="hostIP"></param>
        /// <param name="hostConnectionID"></param>
        /// <param name="gameModeID"></param>
        protected abstract void AddLobby(string hostIP, int hostConnectionID, int gameModeID);


        /// <summary>
        /// remove a lobby from lobby storage
        /// </summary>
        protected abstract void RemoveLobby(Lobby lobby);
        #endregion



        #region Lobby Operation
        private bool Listen(int port) {
            return NetworkServer.Listen(port);
        }

        private void SendToClient(int connectionID, short msgType, MessageBase msg) {
            Log("send: " + msg.ToString());
            NetworkServer.SendToClient(connectionID, msgType, msg);
        }

        

        protected void RegisterHandler(short msgType, NetworkMessageDelegate handler) {
            NetworkServer.RegisterHandler(msgType, handler);
        }

        protected void Log(object msg) {
            if (!debug)
                return;
            if (logEvent != null)
                logEvent.Invoke(msg.ToString());
            Debug.Log("LobbyServer: " + msg);
        }

        protected void Log(object msg1, object msg2) {
            if (!debug)
                return;
            Log(msg1.ToString() + msg2.ToString());
        }

        public delegate void OnLogMessage(string message);
        public event OnLogMessage logEvent;

        protected void TellClientToHost(int connID) {
            Log("tell client to host.");
            SendToClient(connID, LobbyMessageID.ServerRequestStartHost, new EmptyMessage());
        }

        protected void TellClientToJoin(int connID, string hostIP) {
            Log("tell client to join.");
            SendToClient(connID, LobbyMessageID.ServerRequestStartClient, new StringMessage(hostIP));
        }
        #endregion


        #region Lobby callback
        private void AddServerCallbacks() {
            RegisterHandler(MsgType.Connect, OnConnect);
            RegisterHandler(MsgType.Disconnect, OnDisconnect);
            RegisterHandler(LobbyMessageID.ClientRequestLobby, OnClientRequestLobby);
            RegisterHandler(LobbyMessageID.ClientReplyHostResult, OnClientReplyHostResult);
            RegisterHandler(LobbyMessageID.ClientRequestCloseLobby, OnClientRequestCloseLobby);
        }

        protected virtual void OnConnect(NetworkMessage msg) {
            connectionIDs.Add(msg.conn.connectionId);
        }

        protected virtual void OnDisconnect(NetworkMessage msg) {
            Log("Disconnect");
            connectionIDs.Remove(msg.conn.connectionId);

            Lobby lobby = FindLobbyByHostConnID(msg.conn.connectionId);
            if (lobby != null) {
                Log("lobby host disconnected, close lobby");
                RemoveLobby(lobby);
            }
        }


        protected void OnClientRequestLobby(NetworkMessage msg) {
            IntegerMessage requestMsg = msg.ReadMessage<IntegerMessage>();
            int gameModeID = requestMsg.value;

            Log("request lobby received.");
            Lobby availableLobby  = FindAvailableLobby(gameModeID);

            if (availableLobby == null) {
                TellClientToHost(msg.conn.connectionId);
            } else {
                TellClientToJoin(msg.conn.connectionId, availableLobby.hostIP);
            }
        }

        /// <summary>
        /// saves host info if host succeeded
        /// </summary>
        /// <param name="msg"> an integer message which contains the result. 1 is success </param>
        private void OnClientReplyHostResult(NetworkMessage msg) {
            IntegerMessage resultMsg = msg.ReadMessage<IntegerMessage>();
            Log("client reply host, game modeID: ", resultMsg.value);
            Log("save host info");
            AddLobby(msg.conn.address, msg.conn.connectionId, resultMsg.value);
        }


        /// <summary>
        /// A host has requested to close a lobby
        /// </summary>
        /// <param name="msg"></param>
        private void OnClientRequestCloseLobby(NetworkMessage msg) {
            IntegerMessage hostConnIDMessage = msg.ReadMessage<IntegerMessage>();
            int hostConnID = hostConnIDMessage.value;
            Lobby lobby = FindLobbyByHostConnID(hostConnID);

            if (lobby != null) {
                Log("close lobby request accepted.");
                RemoveLobby(lobby);
                SendToClient(hostConnID, LobbyMessageID.ServerReplyAcceptCloseLobby, new EmptyMessage());
            }
        }        
        #endregion

    }
}