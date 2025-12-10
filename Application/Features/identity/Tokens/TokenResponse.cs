namespace Application.Features.identity.Tokens;

public class TokenResponse
{
    public string jwt {get; set;}
    public string RefreshToken {get; set;}
    public DateTime RefreshTokenExpiryDate {get; set;}
}