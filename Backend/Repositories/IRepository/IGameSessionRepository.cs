using Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Repositories;

public interface IGameSessionRepository
{
    Task<GameSession> CreateAsync(GameSession session);
    Task<GameSession?> GetAsync(string id);
    Task<List<GameSession>> GetAllAsync();
    Task UpdateAsync(string id, GameSession session);
    Task DeleteAsync(string id);

    Task JoinPlayerAsync(string sessionId, Player player);
    Task AdvanceQuestionAsync(string sessionId);
    Task SetStateAsync(string sessionId, Core.GameState state);
    
    // Player state snapshots
    Task PostPlayerStateAsync(string sessionId, PlayerState state);
    Task<List<PlayerState>> GetPlayerStatesAsync(string sessionId);
    Task RemovePlayerStateAsync(string sessionId, string playerId);
}

