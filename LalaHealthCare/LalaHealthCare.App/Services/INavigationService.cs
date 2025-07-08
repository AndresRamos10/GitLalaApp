namespace LalaHealthCare.App.Services;

public interface INavigationService
{
    Task NavigateToAsync(string uri);
    void NavigateTo(string uri);
    Task NavigateToWebPortalNativeAsync(string? path = null);
}
