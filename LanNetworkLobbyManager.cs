using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace LANLobbyIPService {
    [RequireComponent(typeof(LobbyClient))]
    public class LanNetworkLobbyManager : NetworkLobbyManager {
        // client used for request host ip
        LobbyClient lobbyIPServiceClient;
        public int pendingGameModeID { get; private set; }

        public string IPServiceAddress {
            get { return lobbyIPServiceClient.serverIP; }
            set { lobbyIPServiceClient.serverIP = value; }
        }

        public int playersCount {
            get {
                int result = 0;
                foreach (var p in lobbySlots) {
                    if (p != null) {
                        result++;
                    }
                }
                return result;
            }
        }

        protected virtual void Start() {
            lobbyIPServiceClient = GetComponent<LobbyClient>();
            lobbyIPServiceClient.OnClientConnect += OnLobbyIPServiceConnected;
            lobbyIPServiceClient.OnShouldStartClient += OnLobbyIPServiceShouldStartClient;
            lobbyIPServiceClient.OnShouldStartHost += OnLobbyIPServiceShouldStartHost;
        }

        public virtual void StartSearchGame(int gameModeID) {
            pendingGameModeID = gameModeID;

            if (lobbyIPServiceClient.connected) {
                StopSearchGameService();
            }
            lobbyIPServiceClient.Connect();
        }

        [ContextMenu("StopHostSafe")]
        public void StopHostSafe() {
            if (lobbyIPServiceClient.connected) {
                StopSearchGameService();
            }
            Debug.Log("Stop host");
            StopHost();
        }

        public virtual void StopSearchGameService() {
            lobbyIPServiceClient.LobbyExit();
        }


        public virtual void StartGame() {
            lobbyIPServiceClient.LobbyStart();
            ServerChangeScene(playScene);
        }



        public override void OnLobbyServerPlayersReady() {
            base.OnLobbyServerPlayersReady();

            // close lobby service
            lobbyIPServiceClient.LobbyStart();
        }

        private void OnLobbyIPServiceConnected(string hostIP) {
            lobbyIPServiceClient.RequestLobby(pendingGameModeID);
        }

        private void OnLobbyIPServiceShouldStartClient(string hostIP) {
            networkAddress = hostIP;
            StartClient();
        }

        private void OnLobbyIPServiceShouldStartHost(string hostIP) {
            StartHost();

        }

        public override void OnLobbyStartHost() {
            lobbyIPServiceClient.ReportHostStarted(pendingGameModeID);
        }

        public override void OnLobbyStartClient(NetworkClient _lobbyClient) {
            lobbyIPServiceClient.ReportClientStarted();

        }

        public bool isLobbyHost {
            get { return lobbyIPServiceClient.isHost; }
        }

    }
}