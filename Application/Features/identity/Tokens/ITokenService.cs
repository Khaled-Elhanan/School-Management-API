namespace Application.Features.identity.Tokens;

public interface ITokenService
{
    Task<TokenResponse> loginAsync(TokenRequest request);
    Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
}