using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections;

namespace LANLobbyIPService {
    public class LobbyClient : MonoBehaviour {
        public string serverIP = "localhost";
        public int port = 7778;
        public bool debug;
        public bool dontDestroyOnLoad;

        [Header("Readonly")]
        public bool isHost;
        public string lobbyHostIP;
        public bool connected {
            get { return client.isConnected; }
        }

        private NetworkClient client;

        void Awake() {
            client = new NetworkClient();
            AddClientCallbacks();

            if (dontDestroyOnLoad) {
                DontDestroyOnLoad(gameObject);
            }
        }

        #region Client Operation
        /// <summary>
        /// Connect to the LAN matchmake server
        /// </summary>
        /// <param name="serverIP"></param>
        /// <param name="port"></param>
        [ContextMenu("Connect")]
        public void Connect() {
            if (connected)
                client.Disconnect();

            ResetStatus();
            client.Connect(serverIP, port);

        }

        /// <summary>
        /// every client should disconnect when game begins or ends
        /// </summary>
        [ContextMenu("Disconnect")]
        public void Disconnect() {
            EnforceConnected();
            client.Disconnect();
            ResetStatus();
        }

        /// <summary>
        /// request to enter or create a lobby
        /// </summary>
        /// <param name="gameModeID"> the ID to specify lobby requirement. Players in the same lobby will have the same ID</param>
        public void RequestLobby(int gameModeID) {
            Log("request lobby");
            EnforceConnected();

            Send(LobbyMessageID.ClientRequestLobby, new IntegerMessage(gameModeID));
        }

        /// <summary>
        /// if host succeeded, report to lobby server
        /// </summary>
        public void ReportHostStarted(int gameModeID) {
            Log("host started.");
            isHost = true;
            Send(LobbyMessageID.ClientReplyHostResult, new IntegerMessage(gameModeID));
        }

        /// <summary>
        /// if client successfully finds the host, disconnect the client from IP service
        /// </summary>
        [ContextMenu("Report client started")]
        public void ReportClientStarted() {
            Log("lobby joined.");
            ClientCloseLobby();
        }

        /// <summary>
        /// if it is a host, it will request server to close the current lobby
        /// </summary>
        [ContextMenu("Lobby Start")]
        public void LobbyStart() {
            HostRequestCloseLobby();
        }

        /// <summary>
        /// host should call this function when the host wants to cancel the game
        /// </summary>
        [ContextMenu("Lobby Exit")]
        public void LobbyExit() {
            HostRequestCloseLobby();
        }


        public delegate void LobbyClientEventHandler(string hostIP);

        /// <summary>
        /// Invoked when client connected to lobby ip server
        /// </summary>
        public event LobbyClientEventHandler OnClientConnect;

        /// <summary>
        /// Invoked when lobby server requests starting as host
        /// </summary>
        public event LobbyClientEventHandler OnShouldStartHost;


        /// <summary>
        /// Invoked when lobby server requests starting as client
        /// </summary>
        public event LobbyClientEventHandler OnShouldStartClient;


        private void ClientCloseLobby() {
            EnforceConnected();
            if (isHost)
                return;
            Log("Client disconnect from lobby server");
            Disconnect();
        }

        /// <summary>
        /// MUST call this method when host wants to start game or exit
        /// this will tell the server to remove the active room
        /// </summary>
        private void HostRequestCloseLobby() {
            EnforceConnected();
            if (!isHost)
                return;
            Send(LobbyMessageID.ClientRequestCloseLobby, new EmptyMessage());
        }


        private void ResetStatus() {
            lobbyHostIP = "";
            isHost = false;
        }


        private void Send(short msgType, MessageBase msg) {
            EnforceConnected();
            client.Send(msgType, msg);
        }


        private void RegisterHandler(short msgType, NetworkMessageDelegate handler) {
            client.RegisterHandler(msgType, handler);
        }

        private void EnforceConnected() {
            if (!client.isConnected) {
                throw new System.Exception("Not connected to lobby server!");
            }
        }

        private void Log(object msg) {
            if (!debug)
                return;
            Debug.Log("LobbyClient: " + msg);
        }

        private void Log(object msg1, object msg2) {
            if (!debug)
                return;
            Log(msg1.ToString() + msg2.ToString());
        }
        #endregion

        #region Client callback
        private void AddClientCallbacks() {
            RegisterHandler(MsgType.Connect, OnConnected);
            RegisterHandler(MsgType.Disconnect, OnDisconnected);
            RegisterHandler(LobbyMessageID.ServerRequestStartHost, OnServerRequestStartHost);
            RegisterHandler(LobbyMessageID.ServerRequestStartClient, OnServerRequestStartClient);
            RegisterHandler(LobbyMessageID.ServerReplyAcceptCloseLobby, OnServerReplyAcceptCloseLobby);
        }

        private void OnConnected(NetworkMessage msg) {
            Log("Connected to server");
            if (OnClientConnect != null)
                OnClientConnect.Invoke("");
        }

        private void OnDisconnected(NetworkMessage msg) {
            Log("Disconnected from server");
        }

        private void OnServerRequestStartHost(NetworkMessage msg) {
            Log("need to start host.");
            lobbyHostIP = "self";

            if (OnShouldStartHost != null)
                OnShouldStartHost.Invoke(lobbyHostIP);
        }

        private void OnServerRequestStartClient(NetworkMessage msg) {
            Log("need to join lobby");
            StringMessage hostIPMsg = msg.ReadMessage<StringMessage>();
            lobbyHostIP = hostIPMsg.value;

            if (OnShouldStartClient != null)
                OnShouldStartClient.Invoke(lobbyHostIP);
        }

        private void OnServerReplyAcceptCloseLobby(NetworkMessage msg) {
            Log("can close lobby now");
            Log("disconnect from lobby server");
            Disconnect();

        }
        #endregion


    }
}
