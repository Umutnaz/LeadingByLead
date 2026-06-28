using Core;
using MongoDB.Driver;

namespace Backend.Repositories;

public class GameSessionRepository : IGameSessionRepository
{
    private readonly IMongoCollection<GameSession> _collection;

    public GameSessionRepository(IMongoDatabase database)
    {
        _collection =
            database.GetCollection<GameSession>("GameSessions");
    }

    public async Task<GameSession> CreateAsync(GameSession session)
    {
        session.Id = "1";

        await _collection.ReplaceOneAsync(
            existing => existing.Id == "1",
            session,
            new ReplaceOptions { IsUpsert = true });

        return session;
    }

    public async Task<GameSession?> GetAsync(string id)
    {
        return await _collection
            .Find(session => session.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<GameSession>> GetAllAsync()
    {
        return await _collection
            .Find(_ => true)
            .ToListAsync();
    }

    public async Task UpdateAsync(
        string id,
        GameSession session)
    {
        session.Id = id;

        await _collection.ReplaceOneAsync(
            existing => existing.Id == id,
            session);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(
            session => session.Id == id);
    }

    public async Task JoinPlayerAsync(
        string sessionId,
        Player player)
    {
        await _collection.UpdateOneAsync(
            session => session.Id == sessionId,
            Builders<GameSession>.Update.Push(
                session => session.Players,
                player));
    }

    public async Task RemovePlayerAsync(
        string sessionId,
        string playerId)
    {
        await _collection.UpdateOneAsync(
            session => session.Id == sessionId,
            Builders<GameSession>.Update.PullFilter(
                session => session.Players,
                Builders<Player>.Filter.Eq(
                    player => player.Id,
                    playerId)));
    }

    public async Task PostPlayerStateAsync(
        string sessionId,
        PlayerState state)
    {
        var session = await GetAsync(sessionId);

        if (session == null)
            return;

        var existing = session.PlayerStates.FirstOrDefault(
            playerState =>
                playerState.PlayerId == state.PlayerId &&
                playerState.CurrentQuestionIndex ==
                state.CurrentQuestionIndex);

        if (existing != null)
            session.PlayerStates.Remove(existing);

        session.PlayerStates.Add(state);

        await UpdateAsync(sessionId, session);
    }

    public async Task<List<PlayerState>> GetPlayerStatesAsync(
        string sessionId)
    {
        var session = await GetAsync(sessionId);

        return session?.PlayerStates ??
               new List<PlayerState>();
    }

    public async Task RemovePlayerStateAsync(
        string sessionId,
        string playerId)
    {
        await _collection.UpdateOneAsync(
            session => session.Id == sessionId,
            Builders<GameSession>.Update.PullFilter(
                session => session.PlayerStates,
                Builders<PlayerState>.Filter.Eq(
                    state => state.PlayerId,
                    playerId)));
    }
}