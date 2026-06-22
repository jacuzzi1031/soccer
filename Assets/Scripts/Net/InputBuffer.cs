using System.Collections.Generic;
using GameFrameSync;
using UnityEngine.Pool;

public class InputBuffer
{
    public class Command
    {
        public int frame;
        public int seatIndex;
        public int inputType;
        public Vector2D moveVector;
    }

    // frame -> commands
    private readonly Dictionary<int, Command[]> buffer;

    private readonly ObjectPool<Command> _commandPool;

    public int maxPlayers;

    // 根据你的最大回滚窗口调整
    private const int KEEP_FRAMES = 240;

    public InputBuffer()
    {
        buffer = new Dictionary<int, Command[]>();
        _commandPool = new ObjectPool<Command>(() => new Command());
    }

    public void SetmaxPlayers(int playerCount)
    {
        maxPlayers = playerCount;
    }

    public void Push(ResFrameInputData msg)
    {
        int seatIndex = msg.SeatIndex;

        if ((uint)seatIndex >= (uint)maxPlayers)
            return;

        if (!buffer.TryGetValue(msg.FrameId, out var frameCmds))
        {
            frameCmds = new Command[maxPlayers];
            buffer.Add(msg.FrameId, frameCmds);
        }

        // 同一玩家同一帧输入被覆盖（补包/修正）
        if (frameCmds[seatIndex] != null)
        {
            _commandPool.Release(frameCmds[seatIndex]);
        }

        var cmd = _commandPool.Allocate();

        cmd.frame = msg.FrameId;
        cmd.seatIndex = seatIndex;
        cmd.inputType = msg.InputType;
        cmd.moveVector = msg.MoveVector;

        frameCmds[seatIndex] = cmd;

        TrimBefore(msg.FrameId - KEEP_FRAMES);
    }

    public Command[] GetFrameCommands(int frame)
    {
        buffer.TryGetValue(frame, out var frameCmds);
        return frameCmds;
    }

    /// <summary>
    /// 删除指定帧之前的数据
    /// </summary>
    private void TrimBefore(int minFrame)
    {
        if (buffer.Count == 0)
            return;

        var removeFrames = ListPool<int>.Get();

        foreach (var kv in buffer)
        {
            if (kv.Key < minFrame)
            {
                removeFrames.Add(kv.Key);
            }
        }

        for (int i = 0; i < removeFrames.Count; i++)
        {
            int frame = removeFrames[i];

            if (!buffer.TryGetValue(frame, out var cmds))
                continue;

            for (int j = 0; j < cmds.Length; j++)
            {
                if (cmds[j] != null)
                {
                    _commandPool.Release(cmds[j]);
                    cmds[j] = null;
                }
            }

            buffer.Remove(frame);
        }

        ListPool<int>.Release(removeFrames);
    }

    public void Clear()
    {
        foreach (var frameCmds in buffer.Values)
        {
            for (int i = 0; i < frameCmds.Length; i++)
            {
                if (frameCmds[i] != null)
                {
                    _commandPool.Release(frameCmds[i]);
                    frameCmds[i] = null;
                }
            }
        }

        buffer.Clear();
    }
}