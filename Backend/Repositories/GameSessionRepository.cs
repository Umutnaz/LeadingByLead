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
        var existingSessions = await _collection
            .Find(_ => true)
            .ToListAsync();

        var usedIds = existingSessions
            .Select(existing =>
                int.TryParse(existing.Id, out var id)
                    ? id
                    : 0)
            .Where(id => id > 0)
            .ToHashSet();

        var nextId = 1;

        while (usedIds.Contains(nextId))
            nextId++;

        session.Id = nextId.ToString();

        await _collection.InsertOneAsync(session);

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

    public async Task AdvanceQuestionAsync(string sessionId)
    {
        await _collection.UpdateOneAsync(
            session => session.Id == sessionId,
            Builders<GameSession>.Update.Inc(
                session => session.CurrentQuestionIndex,
                1));
    }

    public async Task SetStateAsync(
        string sessionId,
        GameState state)
    {
        await _collection.UpdateOneAsync(
            session => session.Id == sessionId,
            Builders<GameSession>.Update.Set(
                session => session.State,
                state));
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
                playerState.PlayerId == state.PlayerId);

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