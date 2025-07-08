namespace LalaHealthCare.DataAccess.ServiceHttp;

public interface ITokenProvider
{
    /// <summary>
    /// Obtiene el token de acceso actual
    /// </summary>
    string? GetAccessToken();

    /// <summary>
    /// Establece el token de acceso
    /// </summary>
    void SetAccessToken(string? token);
}
