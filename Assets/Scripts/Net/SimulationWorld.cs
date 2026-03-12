using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationWorld
{
    private List<ISimulationSystem> systems;
    private SimulationContext context;
    private CommandBuffer commandBuffer;
    private SimEventBus eventBus;
    public InputBuffer InputBuffer;

    public SimulationWorld(
        List<ISimulationSystem> systems,
        SimulationContext context,
        CommandBuffer commandBuffer,
        SimEventBus eventBus,
        InputBuffer inputBuffer)
    {
        this.systems = systems;
        this.context = context;
        this.commandBuffer = commandBuffer;
        this.eventBus = eventBus;
        this.InputBuffer = inputBuffer;
    }

    public void Step(int frame)
    {
        Debug.Log("world Step frame:"+frame);
        InputBuffer.ConsumeFrame(frame, DispatchCommand);

        context.BuildFrom(frame,commandBuffer.Consume());

        for (int i = 0; i < systems.Count; i++)
        {
            systems[i].Tick(context);
        }
        
        eventBus.Flush();
    }

    private void DispatchCommand(InputBuffer.Command cmd)
    {
        var flags = (GameInput.InputEventFlags)cmd.inputType;
        Debug.Log("flags:"+flags);
        if ((flags & GameInput.InputEventFlags.Swap) != 0) {
            EnqueueCommad(SimulationCommandType.Swap, cmd);
            return;
        }
        if ((flags & GameInput.InputEventFlags.ShootRelease) != 0) {
            EnqueueCommad(SimulationCommandType.ShootRelease, cmd);
            return;
        }
        if ((flags & GameInput.InputEventFlags.ShootPress) != 0) {
            EnqueueCommad(SimulationCommandType.ShootPress, cmd);
            return;
        }
        if ((flags & GameInput.InputEventFlags.ShortPass) != 0) {
            EnqueueCommad(SimulationCommandType.ShortPass, cmd);
            return;
        }
        if ((flags & GameInput.InputEventFlags.IncisivePass) != 0) {
            EnqueueCommad(SimulationCommandType.IncisivePass, cmd);
            return;
        }
        if ((flags & GameInput.InputEventFlags.LongPass) != 0) {
            EnqueueCommad(SimulationCommandType.LongPass, cmd);
            return;
        }
        if (flags == GameInput.InputEventFlags.None) {
            EnqueueCommad(SimulationCommandType.NoneInputCommand, cmd);
            return;
        }
    }
    private void EnqueueCommad(SimulationCommandType type, InputBuffer.Command cmd)
    {
        commandBuffer.Enqueue(new SimulationCommand {
            Type = type,
            Direction = cmd.moveVector,
            SeatIndex = cmd.seatIndex,
        });
    }
}
