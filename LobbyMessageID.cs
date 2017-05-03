namespace LANLobbyIPService {
    public static class LobbyMessageID {
        /// <summary>
        /// initiate a new lobby, with host ip and port stored
        /// when a client wants to join a lobby, send the host ip
        /// </summary>
        public const short ClientRequestLobby = 900;
        public const short ServerRequestStartHost = 901;
        public const short ServerRequestStartClient = 902;
        public const short ClientReplyHostResult = 903;
        public const short ClientReplyJoinLobbyResult = 904;

        /// <summary>
        /// when the game is full or started, lobby host will request close this room
        /// </summary>
        public const short ClientRequestCloseLobby = 910;
        public const short ServerReplyAcceptCloseLobby = 911;
    }
    
}