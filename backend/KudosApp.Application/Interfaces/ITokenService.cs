using KudosApp.Domain.Entities;

namespace KudosApp.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
