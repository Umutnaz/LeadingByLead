using Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frontend.Services.IService;

public interface IGameSessionService
{
    Task<GameSession?> CreateSessionAsync(GameSession session);
    Task<GameSession?> GetSessionAsync(string id);
    Task<List<GameSession>> GetAllSessionsAsync();
    Task<Player?> JoinSessionAsync(string sessionId, Player player);
    Task StartSessionAsync(string sessionId);
    Task FinishSessionAsync(string sessionId);
    Task AdvanceQuestionAsync(string sessionId);
    Task DeleteSessionAsync(string sessionId);

    // Player state snapshots
    Task PostPlayerStateAsync(string sessionId, PlayerState state);
    Task<List<PlayerState>> GetPlayerStatesAsync(string sessionId);
}