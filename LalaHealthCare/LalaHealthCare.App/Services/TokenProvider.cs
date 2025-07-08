using LalaHealthCare.DataAccess.ServiceHttp;

namespace LalaHealthCare.App.Services;

public class TokenProvider : ITokenProvider
{
    private readonly AppState _appState;

    public TokenProvider(AppState appState)
    {
        _appState = appState;
    }

    public string? GetAccessToken()
    {
        return _appState.Token;
    }

    public void SetAccessToken(string? token)
    {
        _appState.Token = token;
    }  
}
