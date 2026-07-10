// Copyright (c) YourProjectName. All rights reserved.

using System;

using IntVue.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace IntVue.Views;

/// <summary>
/// ProductReviewPage - XAML view for playing pre-recorded interview questions with countdown timer.
/// Supports WebM video playback, playlist navigation, and countdown-based recording workflow.
/// </summary>
public sealed partial class ProductReviewPage : Page
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductReviewPage"/> class.
    /// </summary>
    public ProductReviewPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets viewModel instance providing UI state, commands, and business logic.
    /// </summary>
    public ProductReviewViewModel ViewModel
    {
        get
        {
            var viewModel = App.Services.GetService<ProductReviewViewModel>();
            return viewModel ?? throw new InvalidOperationException("ProductReviewViewModel not registered");
        }
    }

    /// <summary>
    /// Page loaded - Initialize ViewModel and MediaPlayerElement bindings.
    /// </summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Wire up MediaPlayerElement to ViewModel
        // Note: MediaPlayerElement playback controlled via ViewModel commands
    }

    /// <summary>
    /// Page unloaded - Clean up resources and dispose ViewModel.
    /// </summary>
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // ViewModel disposal handled by DI container if needed
    }

    /// <summary>
    /// Back button click handler - Navigate back to previous page.
    /// </summary>
    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Frame?.CanGoBack == true)
            {
                Frame.GoBack();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation back failed: {ex.Message}");
        }
    }
}
