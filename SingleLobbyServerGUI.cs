using UnityEngine;
using System.Collections;

namespace LANLobbyIPService {
    [RequireComponent(typeof(SingleLobbyServer))]
    public class SingleLobbyServerGUI : MonoBehaviour {
        SingleLobbyServer server;
        string[] logs = new string[8];

        void Awake() {
            server = GetComponent<SingleLobbyServer>();
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
            GUILayout.BeginArea(new Rect(20, 20, 200, 500));
            GUILayout.BeginVertical();

            GUILayout.Label("self ip");
            GUILayout.TextField(server.serverIP.ToString());
            GUILayout.Space(5);

            GUILayout.Label("port");
            GUILayout.TextField(server.port.ToString());
            GUILayout.Space(5);

            GUILayout.Label("client count");
            GUILayout.TextField(server.connectionIDs.Count.ToString());
            GUILayout.Space(5);

            GUILayout.Label("has host");
            GUILayout.TextField(server.lobby.hasHost.ToString());
            GUILayout.Space(5);


            GUILayout.Label("host ip");
            GUILayout.TextField(server.lobby.hostIP.ToString());
            GUILayout.Space(5);

            GUILayout.Label("host game mode ID");
            GUILayout.TextField(server.lobby.gameModeID.ToString());
            GUILayout.Space(5);

            GUILayout.Label("log:");

            GUILayout.TextField(GetLog());

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}