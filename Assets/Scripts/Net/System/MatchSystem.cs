
    using SocketProtocol;
    using UnityEngine;

    public class MatchSystem:ISimulationSystem {
        public void Stop() {
        }
        public GameSimState currentSimState;
        public MatchState currentState;
        private const float DURATION_GAME_SEC = 2 * 60f;
        public float timeLeft;
        private GameSimStateFactory _gameSimStateFactory=new GameSimStateFactory();
        public SimEventBus _eventBus;
        private CommandBuffer _commandBuffer;
        private RoomMatchType _currentMatchType;
        public int goalsHome;
        public int goalsAway;
        public MatchSystem(SimEventBus eventBus,CommandBuffer commandBuffer,RoomMatchType matchType) {
            _eventBus=eventBus;
            _commandBuffer=commandBuffer;
            _currentMatchType=matchType;
            goalsHome=0; goalsAway=0;
            // timeLeft = DURATION_GAME_SEC;
            timeLeft = 2f;
            SwitchGameState(MatchState.RESET);
        }

        public bool resetAndHomeKickoff() {
            if (_currentMatchType != RoomMatchType.UltimateTeam) {
                return true;
            }
            //比赛初始
            if (_currentMatchType == RoomMatchType.UltimateTeam && goalsHome == 0 && goalsAway == 0) {
                return true;
            }
            //which score
            
            return false;
            
        }
        public void Tick(SimulationContext context)
        {
            foreach (var command in context.Commands)
            {
                switch (command.Type)
                {
                    case SimulationCommandType.AllPlayersReadyForKickoff:
                        SwitchGameState(MatchState.KICKOFF);
                        break;
                    case SimulationCommandType.ShortPass:
                        currentSimState?.OnKickoffStart(command.SeatIndex);
                        break;
                    case SimulationCommandType.TeamScoring:
                        currentSimState?.OnTeamScoring(command.isHome);
                        break;
                }
            }
            currentSimState._Update(context.DeltaTime);
        }

        //是否需要SimulationContext作为OnEnter的参数
        public void SwitchGameState(MatchState newState, GameStateData data = null) {
            if (currentState == newState) return;
            if (currentSimState != null) {
                currentSimState.OnExit();
            }
            currentState=newState;
            currentSimState = _gameSimStateFactory.GetFreshState(newState);
            currentSimState.Setup(this,data,_eventBus,_commandBuffer);
            currentSimState.OnEnter();
        }

        public bool IsTimeUp() {
            return timeLeft <= 0f;
        }

        public bool IsTied()
        {
            return goalsHome == goalsAway;
        }

        public bool getWinnerIsHome() {
            return goalsHome > goalsAway;
        }
    }