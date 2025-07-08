using LalaHealthCare.App.Pages;
using Microsoft.AspNetCore.Components;

namespace LalaHealthCare.App.Services;

public class NavigationService : INavigationService
{
    private readonly NavigationManager _navigationManager;

    public NavigationService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public async Task NavigateToAsync(string uri)
    {
        await Task.Yield(); // Permite que el ciclo de renderizado se complete
        _navigationManager.NavigateTo(uri);
    }

    public void NavigateTo(string uri)
    {
        _navigationManager.NavigateTo(uri);
    }

    public async Task NavigateToWebPortalNativeAsync(string? path = null)
    {
        try
        {
            var webPortalPage = new WebPortalPage();

            // Usar navegación modal que funciona desde cualquier contexto
            await Application.Current.MainPage.Navigation.PushModalAsync(webPortalPage);

        }
        catch (Exception ex)
        {
        }
    }
}