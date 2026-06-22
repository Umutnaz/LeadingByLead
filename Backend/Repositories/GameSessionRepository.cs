using Core;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Repositories;

public class GameSessionRepository : IGameSessionRepository
{
    private readonly IMongoCollection<GameSession> _collection;

    public GameSessionRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<GameSession>("GameSessions");
    }

    public async Task<GameSession> CreateAsync(GameSession session)
    {
        await _collection.InsertOneAsync(session);
        return session;
    }

    public async Task<GameSession?> GetAsync(string id) =>
        await _collection.Find(s => s.Id == id).FirstOrDefaultAsync();

    public async Task<List<GameSession>> GetAllAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public async Task UpdateAsync(string id, GameSession session) =>
        await _collection.ReplaceOneAsync(s => s.Id == id, session);

    public async Task DeleteAsync(string id) =>
        await _collection.DeleteOneAsync(s => s.Id == id);

    public async Task JoinPlayerAsync(string sessionId, Player player) =>
        await _collection.UpdateOneAsync(
            s => s.Id == sessionId,
            Builders<GameSession>.Update.Push(p => p.Players, player)
        );

    public async Task AdvanceQuestionAsync(string sessionId) =>
        await _collection.UpdateOneAsync(
            s => s.Id == sessionId,
            Builders<GameSession>.Update.Inc(s => s.CurrentQuestionIndex, 1)
        );

    public async Task SetStateAsync(string sessionId, Core.GameState state) =>
        await _collection.UpdateOneAsync(
            s => s.Id == sessionId,
            Builders<GameSession>.Update.Set(s => s.State, state)
        );

    public async Task PostPlayerStateAsync(string sessionId, PlayerState state)
    {
        var session = await GetAsync(sessionId);
        if (session == null) return;

        // update or add snapshot
        var existing = session.PlayerStates.FirstOrDefault(p => p.PlayerId == state.PlayerId);
        if (existing != null)
        {
            session.PlayerStates.Remove(existing);
        }
        session.PlayerStates.Add(state);

        await UpdateAsync(sessionId, session);
    }

    public async Task<List<PlayerState>> GetPlayerStatesAsync(string sessionId)
    {
        var session = await GetAsync(sessionId);
        return session?.PlayerStates ?? new List<PlayerState>();
    }

    public async Task RemovePlayerStateAsync(string sessionId, string playerId) =>
        await _collection.UpdateOneAsync(
            s => s.Id == sessionId,
            Builders<GameSession>.Update.PullFilter(s => s.PlayerStates, Builders<PlayerState>.Filter.Eq(p => p.PlayerId, playerId))
        );
}

