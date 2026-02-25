
public struct PlayStyleShowSignal {
    public int playerId;
    public PlayerState playerState;
    public PlayStyleShowSignal(int playerSimPlayerId, PlayerState ContextPlayerState) {
        playerId = playerSimPlayerId;
        playerState = ContextPlayerState;
    }

}
