using System.Collections.ObjectModel;
using System.Windows.Input;
using LalaHealthCare.Business.Services;
using LalaHealthCare.DataAccess.Models;
using LalaHealthCare.App.Services;
using LalaHealthCare.App.Components.Dialogs;
using MudBlazor;
using Color = MudBlazor.Color;
using LalaHealthCare.App.Models;

namespace LalaHealthCare.App.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly IVisitService _visitService;
    private readonly IAuthenticationService _authService;
    
    private readonly ISnackbar _snackbar;
    private readonly ILoggingService _loggingService;
    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;

    private ObservableCollection<Visit> _visits = new();
    private Visit? _nextVisit;
    private string _searchTerm = string.Empty;
    private DateTime? _selectedDate = DateTime.Today;
    

    public event Action? OnDataUpdated;

    public DashboardViewModel(
        AppState appState,
        IVisitService visitService,
        IAuthenticationService authService,
        ISnackbar snackbar,
        ILoggingService loggingService,
        IDialogService dialogService,
        INotificationService notificationService) : base(loggingService, appState)
    {
        _visitService = visitService;
        _authService = authService;       
        _snackbar = snackbar;
        _loggingService = loggingService;
        _dialogService = dialogService;
        _notificationService = notificationService;

        Title = "Dashboard";

        // Inicializar comandos
        LoadVisitsCommand = new Command(async () => await LoadVisitsAsync());
        SearchCommand = new Command(async () => await SearchVisitsAsync());
        CheckInCommand = new Command<Visit>(async (visit) => await CheckInAsync(visit));
        CheckOutCommand = new Command<Visit>(async (visit) => await CheckOutAsync(visit));
        AddNewVisitCommand = new Command(AddNewVisit);                
    }

    #region Properties

    public ObservableCollection<Visit> Visits
    {
        get => _visits;
        set => SetProperty(ref _visits, value);
    }

    public Visit? NextVisit
    {
        get => _nextVisit;
        set => SetProperty(ref _nextVisit, value);
    }

    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (SetProperty(ref _searchTerm, value))
            {
                _ = SearchVisitsAsync();
            }
        }
    }

    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
            {
                _ = LoadVisitsAsync();
            }
        }
    }

    #endregion

    #region Commands

    public ICommand LoadVisitsCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand CheckInCommand { get; }
    public ICommand CheckOutCommand { get; }
    public ICommand AddNewVisitCommand { get; }       

    #endregion

    #region Methods

    private async Task LoadVisitsAsync()
    {
        try
        {
            IsBusy = true;
            var visits = await _visitService.GetVisitsByDateAsync(SelectedDate ?? DateTime.Today);
            Visits = new ObservableCollection<Visit>(visits);

            NextVisit = visits
                .Where(v => v.Status == VisitStatus.Planned && v.ScheduledDateTime > DateTime.Now)
                .OrderBy(v => v.ScheduledDateTime)
                .FirstOrDefault();

            // Programar notificaciones para visitas planificadas
            await ScheduleNotificationsForVisits(visits);

            OnDataUpdated?.Invoke();
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error loading visits", ex);
            _snackbar.Add("Error al cargar las visitas", Severity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ScheduleNotificationsForVisits(List<Visit> visits)
    {
        try
        {
            // Solo programar notificaciones para visitas futuras planificadas
            var futureVisits = visits.Where(v =>
                v.Status == VisitStatus.Planned &&
                v.ScheduledDateTime > DateTime.Now).ToList();

            foreach (var visit in futureVisits)
            {
                await _notificationService.ScheduleVisitRemindersAsync(visit);
            }
        }
        catch (Exception ex)
        {
            // No interrumpir el flujo si falla la programación de notificaciones
            await _loggingService.LogErrorAsync("Error scheduling notifications", ex);
        }
    }

    private async Task SearchVisitsAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            await LoadVisitsAsync();
            return;
        }

        try
        {
            IsBusy = true;
            var visits = await _visitService.SearchVisitsAsync(SearchTerm);
            Visits = new ObservableCollection<Visit>(visits);
            OnDataUpdated?.Invoke();
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error searching visits", ex);
            _snackbar.Add("Error al buscar visitas", Severity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CheckInAsync(Visit visit)
    {
        if (visit == null) return;

        // Mostrar diálogo de confirmación
        var parameters = new DialogParameters<CheckInConfirmationDialog>
        {
            { x => x.Visit, visit }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        var dialog = await _dialogService.ShowAsync<CheckInConfirmationDialog>("Check-In Confirmation", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is CheckInConfirmationResult confirmationResult && confirmationResult.Confirmed)
        {
            try
            {
                IsBusy = true;
                var locationResult = confirmationResult.LocationResult;
                var success = await _visitService.CheckInAsync(
                    visit.Id,
                    locationResult?.Latitude,
                    locationResult?.Longitude,
                    locationResult?.Address
                );

                if (success)
                {
                    _snackbar.Add($"Check-in exitoso para {visit.PatientName}", Severity.Success);

                    // Cancelar notificaciones para esta visita
                    await _notificationService.CancelVisitRemindersAsync(visit.Id);

                    await LoadVisitsAsync();
                }
                else
                {
                    _snackbar.Add("Error al hacer check-in", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"Error during check-in for visit {visit.Id}", ex);
                _snackbar.Add("Error al hacer check-in", Severity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    private async Task CheckOutAsync(Visit visit)
    {
        if (visit == null) return;

        // Mostrar diálogo de confirmación con observaciones y firma
        var parameters = new DialogParameters<CheckOutConfirmationDialog>
        {
            { x => x.Visit, visit }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        var dialog = await _dialogService.ShowAsync<CheckOutConfirmationDialog>("Check-Out Confirmation", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is CheckOutConfirmationResult confirmationResult && confirmationResult.Confirmed)
        {
            try
            {
                IsBusy = true;
                var locationResult = confirmationResult.LocationResult;
                var success = await _visitService.CheckOutAsync(
                    visit.Id,
                    confirmationResult.Observations,
                    confirmationResult.SignatureData,
                    locationResult?.Latitude,
                    locationResult?.Longitude,
                    locationResult?.Address
                );

                if (success)
                {
                    _snackbar.Add($"Check-out exitoso para {visit.PatientName}", Severity.Success);
                    await LoadVisitsAsync();
                }
                else
                {
                    _snackbar.Add("Error al hacer check-out", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"Error during check-out for visit {visit.Id}", ex);
                _snackbar.Add("Error al hacer check-out", Severity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    private void AddNewVisit()
    {
        _snackbar.Add("Función para agregar visita - Por implementar", Severity.Info);
    }    

    public string GetVisitCardClass(Visit visit)
    {
        return $"mb-3 visit-card visit-status-{visit.Status.ToString().ToLower()}";
    }

    public Color GetStatusColor(VisitStatus status) => status switch
    {
        VisitStatus.Completed => Color.Success,
        VisitStatus.InProgress => Color.Warning,
        VisitStatus.Planned => Color.Info,
        _ => Color.Default
    };

    public string GetStatusText(VisitStatus status) => status switch
    {
        VisitStatus.Completed => "Completed",
        VisitStatus.InProgress => "In Progress",
        VisitStatus.Planned => "Planned",
        VisitStatus.Cancelled => "Cancelled",
        _ => status.ToString()
    };

    protected override async Task ShowErrorToUserAsync(string message)
    {
        _snackbar.Add(message, Severity.Error);
        await Task.CompletedTask;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        if (!_authService.IsAuthenticated)
        {
            return;
        }

        // Solicitar permisos de notificación
        await RequestNotificationPermissionsAsync();

        await LoadVisitsAsync();
    }

    private async Task RequestNotificationPermissionsAsync()
    {
        try
        {
            var hasPermission = await _notificationService.RequestPermissionAsync();
            if (!hasPermission)
            {
                _snackbar.Add("Notifications are disabled. You can enable them in the settings.", Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error requesting notification permissions", ex);
        }
    }

    #endregion
}