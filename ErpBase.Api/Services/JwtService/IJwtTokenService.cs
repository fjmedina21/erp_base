using ErpBase.Api.Entities;

namespace ErpBase.Api.Services.JwtService;

public interface IJwtTokenService
{
    string CreateToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
}
