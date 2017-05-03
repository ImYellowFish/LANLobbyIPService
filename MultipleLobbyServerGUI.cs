using UnityEngine;
using System.Collections;

namespace LANLobbyIPService {
    [RequireComponent(typeof(MultipleLobbyServer))]
    public class MultipleLobbyServerGUI : MonoBehaviour {
        MultipleLobbyServer server;
        string[] logs = new string[8];

        void Awake() {
            server = GetComponent<MultipleLobbyServer>();
            server.logEvent += OnLogMessage;
        }

        private void OnLogMessage(string message) {
            for (int i = 0; i < logs.Length - 1; i++) {
                logs[i] = logs[i + 1];
            }

            logs[logs.Length - 1] = message;
        }

        private string GetLog() {
            string result = "";
            for (int i = 0; i < logs.Length; i++) {
                result += (logs[i] + "\n");
            }
            return result;
        }

        void OnGUI() {
            GUILayout.BeginArea(new Rect(20, 20, 800, 500));
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("self ip", GUILayout.MinWidth(20), GUILayout.MaxWidth(40));
            GUILayout.TextField(server.serverIP.ToString());
            GUILayout.Space(40);

            GUILayout.Label("port", GUILayout.MinWidth(20), GUILayout.MaxWidth(30));
            GUILayout.TextField(server.port.ToString());
            GUILayout.Space(40);

            GUILayout.Label("client count", GUILayout.MinWidth(20), GUILayout.MaxWidth(80));
            GUILayout.TextField(server.connectionIDs.Count.ToString());
            GUILayout.Space(40);

            GUILayout.Label("lobby count", GUILayout.MinWidth(20), GUILayout.MaxWidth(80));
            GUILayout.TextField(server.lobbys.Count.ToString());
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            for (int i = 0; i < server.lobbys.Count; i++) {
                Lobby lobby = server.lobbys[i];
                GUILayout.BeginHorizontal();
                GUILayout.Label("host IP: ");
                GUILayout.TextField(lobby.hostIP.ToString());
                GUILayout.Space(30);

                GUILayout.Label("host game mode ID");
                GUILayout.TextField(lobby.gameModeID.ToString());
                GUILayout.Space(30);

                GUILayout.Label("host connection ID");
                GUILayout.TextField(lobby.hostConnectionID.ToString());
                GUILayout.Space(200);
                GUILayout.EndHorizontal();
            }
            
            
            GUILayout.Label("log:");

            GUILayout.TextField(GetLog());

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}