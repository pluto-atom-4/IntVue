// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using Windows.Media.Core;

namespace IntVue.Views;

/// <summary>
/// Simple media player test page to verify WebM playback functionality.
/// </summary>
public sealed partial class MediaPlayerTestPage : Page
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaPlayerTestPage"/> class.
    /// </summary>
    public MediaPlayerTestPage()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Called when the page is navigated to. Loads and plays the first media file from the questions directory.
    /// </summary>
    /// <param name="e">The navigation event arguments.</param>
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await this.LoadFirstMediaFileAsync();
    }

    /// <summary>
    /// Loads the first WebM media file from the questions directory and attempts to play it.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadFirstMediaFileAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[MediaPlayerTestPage] Starting media load");
            this.TxtStatus.Text = "Loading media files...";

            // Get questions directory from app
            var questionsDir = App.QuestionsDirectory;
            if (string.IsNullOrEmpty(questionsDir))
            {
                this.TxtStatus.Text = "ERROR: No questions directory configured. Use --questions-dir=path argument.";
                System.Diagnostics.Debug.WriteLine("[MediaPlayerTestPage] No questions directory configured");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[MediaPlayerTestPage] Questions directory: {questionsDir}");

            // Check if directory exists
            if (!Directory.Exists(questionsDir))
            {
                this.TxtStatus.Text = $"ERROR: Directory not found: {questionsDir}";
                System.Diagnostics.Debug.WriteLine("[MediaPlayerTestPage] Directory does not exist");
                return;
            }

            // Find first WebM file
            var webmFiles = Directory.GetFiles(questionsDir, "*.webm", SearchOption.TopDirectoryOnly)
                .OrderBy(f => Path.GetFileName(f))
                .ToList();

            if (webmFiles.Count == 0)
            {
                this.TxtStatus.Text = $"ERROR: No WebM files found in {questionsDir}";
                System.Diagnostics.Debug.WriteLine("[MediaPlayerTestPage] No WebM files found");
                return;
            }

            var firstFile = webmFiles.First();
            var fileName = Path.GetFileName(firstFile);

            System.Diagnostics.Debug.WriteLine($"[MediaPlayerTestPage] Found file: {fileName}");
            this.TxtFilename.Text = fileName;

            // Attempt to play the file
            System.Diagnostics.Debug.WriteLine($"[MediaPlayerTestPage] Creating media source from: {firstFile}");
            var mediaSource = MediaSource.CreateFromUri(new Uri(firstFile));

            System.Diagnostics.Debug.WriteLine("[MediaPlayerTestPage] Setting media player source");
            this.MediaPlayer.Source = mediaSource;

            this.TxtStatus.Text = "✓ Media loaded successfully. Press Play to test.";
            System.Diagnostics.Debug.WriteLine("[MediaPlayerTestPage] Media loaded successfully");
        }
        catch (FileNotFoundException ex)
        {
            this.TxtStatus.Text = $"ERROR: File not found - {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[MediaPlayerTestPage] FileNotFoundException: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            this.TxtStatus.Text = $"ERROR: Access denied - {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[MediaPlayerTestPage] UnauthorizedAccessException: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            this.TxtStatus.Text = $"ERROR: Invalid operation - {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[MediaPlayerTestPage] InvalidOperationException: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.TxtStatus.Text = $"ERROR: {ex.GetType().Name} - {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[MediaPlayerTestPage] Exception: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
