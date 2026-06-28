using Core;

namespace Backend.Repositories;

public interface IGameSessionRepository
{
    Task<GameSession> CreateAsync(GameSession session);

    Task<GameSession?> GetAsync(string id);

    Task<List<GameSession>> GetAllAsync();

    Task UpdateAsync(string id, GameSession session);

    Task DeleteAsync(string id);

    Task JoinPlayerAsync(string sessionId, Player player);

    Task RemovePlayerAsync(string sessionId, string playerId);

    Task PostPlayerStateAsync(string sessionId, PlayerState state);

    Task<List<PlayerState>> GetPlayerStatesAsync(string sessionId);

    Task RemovePlayerStateAsync(string sessionId, string playerId);
}