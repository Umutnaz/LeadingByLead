using Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frontend.Services.IService;

public interface IQuestionsService
{
    Task<List<Question>> GetAllQuestionsAsync();
    Task<Question> GetQuestionAsync(string id);
    Task<Question> CreateQuestionAsync(Question question);
    Task UpdateQuestionAsync(string id, Question question);
    Task DeleteQuestionAsync(string id);
}