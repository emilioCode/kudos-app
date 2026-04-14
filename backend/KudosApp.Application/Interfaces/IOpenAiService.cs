using System.Collections.Generic;
using System.Threading.Tasks;

namespace KudosApp.Application.Interfaces;

public interface IOpenAiService
{
    Task<string> SuggestKudosMessageAsync(string recipientName, string categoryName);
    Task<string> SuggestCategoryAsync(string message, List<string> availableCategories);
}
