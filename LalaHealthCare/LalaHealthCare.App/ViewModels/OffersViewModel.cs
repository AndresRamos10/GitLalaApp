using System.Collections.ObjectModel;
using System.Windows.Input;
using LalaHealthCare.App.Components.Dialogs;
using LalaHealthCare.App.Services;
using LalaHealthCare.Business.Services;
using LalaHealthCare.DataAccess.Models;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace LalaHealthCare.App.ViewModels;

public class OffersViewModel : ViewModelBase
{
    private readonly IOfferService _offerService;
    private readonly ILogger<OffersViewModel> _logger;
    private readonly IDialogService _dialogService;
    private ISnackbar? _snackbar;

    private DateTime? _selectedDate = DateTime.Today;
    private ObservableCollection<Offer> _offers = new();
    private bool _isLoadingOffers;
    private Offer? _selectedOffer;

    public event Action? OnDataUpdated;

    public OffersViewModel(
        IOfferService offerService,
        ILogger<OffersViewModel> logger,
        ISnackbar snackbar,
        ILoggingService loggingService,
        AppState appState,
        IDialogService dialogService) : base(loggingService, appState)
    {
        _offerService = offerService;
        _logger = logger;
        _snackbar = snackbar;
        _dialogService = dialogService;
        Title = "Offers";

        ViewOfferCommand = new Command<Offer>(async (offer) => await ViewOfferAsync(offer));
        LoadOffersCommand = new Command(async () => await LoadOffersAsync());
    }

    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set {
            if (SetProperty(ref _selectedDate, value))
            {
                _ = LoadOffersAsync();
            }
        }
    }

    public ObservableCollection<Offer> Offers
    {
        get => _offers;
        set => SetProperty(ref _offers, value);
    }

    public bool IsLoadingOffers
    {
        get => _isLoadingOffers;
        set => SetProperty(ref _isLoadingOffers, value);
    }

    public Offer? SelectedOffer
    {
        get => _selectedOffer;
        set => SetProperty(ref _selectedOffer, value);
    }

    public string NextOfferInfo
    {
        get
        {
            var nextOffer = Offers.FirstOrDefault(o => o.Status == OfferStatus.Pending);
            return nextOffer != null
                ? $"{nextOffer.ScheduledDateTime:HH:mm} - {nextOffer.ScheduledDateTime.AddMinutes(nextOffer.Duration.TotalMinutes):HH:mm}"
                : "No pending offers";
        }
    }

    public ICommand LoadOffersCommand { get; }
    public ICommand ViewOfferCommand { get; }   

    public override async Task InitializeAsync()
    {
        await LoadOffersAsync();
    }

    private async Task LoadOffersAsync()
    {

        IsLoadingOffers = true;
        try
        {
            var offers = await _offerService.GetOffersForDateAsync(SelectedDate ?? DateTime.Now);
            Offers = new ObservableCollection<Offer>(offers);
            OnPropertyChanged(nameof(NextOfferInfo));

            OnDataUpdated?.Invoke();
        }
        finally
        {
            IsLoadingOffers = false;
        }
    }

    private async Task ViewOfferAsync(Offer offer)
    {
        try
        {
            SelectedOffer = await _offerService.GetOfferDetailsAsync(offer.Id);

            var parameters = new DialogParameters
            {
                ["Offer"] = SelectedOffer
            };

            var options = new DialogOptions
            {
                MaxWidth = MaxWidth.Small,
                FullWidth = true,
                BackdropClick = false
            };

            var dialog = await _dialogService.ShowAsync<OfferDetailsDialog>("Offer Details", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is bool accepted)
            {
                if (accepted)
                {
                    await AcceptOfferAsync();
                }
                else
                {
                    await ShowDeclineDialogAsync();
                }
            }
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex, "Error viewing offer details");
        }
    }

    private async Task ShowDeclineDialogAsync()
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
            BackdropClick = false
        };

        var dialog = await _dialogService.ShowAsync<OfferDeclineDialog>("Decline Offer", new DialogParameters(), options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is string reason)
        {
            DeclineReason = reason;
            await DeclineOfferAsync();
        }
    }

    private async Task AcceptOfferAsync()
    {
        if (SelectedOffer == null) return;

        await ExecuteAsync(async () =>
        {
            var success = await _offerService.AcceptOfferAsync(SelectedOffer.Id);

            if (success)
            {
                ShowSnackbar("Offer accepted successfully!", Severity.Success);
                await LoadOffersAsync();
            }
            else
            {
                ShowSnackbar("Failed to accept offer. Please try again.", Severity.Error);
            }
        });
    }

    private async Task DeclineOfferAsync()
    {
        if (SelectedOffer == null || string.IsNullOrWhiteSpace(DeclineReason))
        {
            ShowSnackbar("Please provide a reason for declining.", Severity.Warning);
            return;
        }

        await ExecuteAsync(async () =>
        {
            var success = await _offerService.DeclineOfferAsync(SelectedOffer.Id, DeclineReason);

            if (success)
            {
                ShowSnackbar("Offer declined.", Severity.Info);
                await LoadOffersAsync();
            }
            else
            {
                ShowSnackbar("Failed to decline offer. Please try again.", Severity.Error);
            }
        });
    }

    private string DeclineReason { get; set; } = string.Empty;   

    protected override Task ShowErrorToUserAsync(string message)
    {
        ShowSnackbar(message, Severity.Error);
        return Task.CompletedTask;
    }

    private void ShowSnackbar(string message, Severity severity)
    {
        _snackbar?.Add(message, severity, config =>
        {
            config.VisibleStateDuration = 3000;
            config.ShowCloseIcon = true;
        });
    }
}