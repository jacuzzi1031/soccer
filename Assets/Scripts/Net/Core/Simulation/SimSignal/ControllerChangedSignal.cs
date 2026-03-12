
public struct ControllerChangedSignal
{
    public int OldPlayerId; // -1 表示没有旧玩家
    public int NewPlayerId;
    public ControlScheme Scheme;
}