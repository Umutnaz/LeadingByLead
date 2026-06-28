using Core;

namespace Frontend.Services.IService;

public interface IGameSessionService
{
    string LastError { get; }

    Task<GameSession?> CreateSessionAsync(GameSession session);

    Task<GameSession?> GetSessionAsync(string id);

    Task<List<GameSession>> GetAllSessionsAsync();

    Task<Player?> JoinSessionAsync(
        string sessionId,
        Player player);

    Task<bool> RemovePlayerAsync(
        string sessionId,
        string playerId);

    Task<bool> StartSessionAsync(string sessionId);

    Task<bool> FinishSessionAsync(string sessionId);

    Task<bool> AdvanceQuestionAsync(string sessionId);

    Task<bool> DeleteSessionAsync(string sessionId);

    Task<bool> PostPlayerStateAsync(
        string sessionId,
        PlayerState state);

    Task<List<PlayerState>> GetPlayerStatesAsync(
        string sessionId);

    Task<List<PlayerState>> GetCurrentPlayerStatesAsync(
        string sessionId);

    Task<bool> DeletePlayerStateAsync(
        string sessionId,
        string playerId);
}