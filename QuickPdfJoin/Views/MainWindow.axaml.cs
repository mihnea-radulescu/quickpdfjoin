using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using QuickPdfJoin.CustomEventArgs;

namespace QuickPdfJoin.Views;

public partial class MainWindow : Window, IMainView
{
    static MainWindow()
    {
        var supportedFilePickerFileTypes = new List<FilePickerFileType>
        {
            new FilePickerFileType("PDF files")
            {
                Patterns = new List<string>
                {
                    "*.pdf",
                    "*.PDF"
                }
            }
		};

        InputPdfFilesOpenOptions = new FilePickerOpenOptions
		{
			AllowMultiple = true,
			FileTypeFilter = supportedFilePickerFileTypes,
			Title = "Select Input PDF File or Files"
		};

        OutputPdfFileSaveOptions = new FilePickerSaveOptions
        {
            DefaultExtension = ".pdf",
            FileTypeChoices = supportedFilePickerFileTypes,
            ShowOverwritePrompt = true,
            Title = "Select Output PDF File"
        };
	}
    
    public MainWindow()
    {
        InitializeComponent();

		SetInitialUiStatus();
	}

	public void SetUiEnabledStatus(bool isEnabled)
	{
		if (isEnabled)
		{
			Closing -= OnMainWindowClosing;
			_buttonJoinPdfFiles.Content = JoinPdfFilesDefaultButtonText;
		}
		else
		{
			Closing += OnMainWindowClosing;
			_buttonJoinPdfFiles.Content = JoinPdfFilesInProgressButtonText;
		}

		_buttonClearInputPdfFiles.IsEnabled = isEnabled;
		_buttonAddInputPdfFiles.IsEnabled = isEnabled;
		_buttonJoinPdfFiles.IsEnabled = isEnabled;
	}

	public async Task ShowSuccessMessage(string successMessage)
	{
		var successMessageBox = MessageBoxManager.GetMessageBoxStandard(
			"Output PDF File Successfully Saved",
			successMessage,
			MsBox.Avalonia.Enums.ButtonEnum.Ok,
			MsBox.Avalonia.Enums.Icon.Success,
			WindowStartupLocation.CenterOwner);

		await successMessageBox.ShowWindowDialogAsync(this);
	}

	public async Task ShowErrorMessage(string errorMessage)
	{
		var errorMessageBox = MessageBoxManager.GetMessageBoxStandard(
			"Output PDF File Save Error",
			errorMessage,
			MsBox.Avalonia.Enums.ButtonEnum.Ok,
			MsBox.Avalonia.Enums.Icon.Error,
			WindowStartupLocation.CenterOwner);

		await errorMessageBox.ShowWindowDialogAsync(this);
	}

	public event EventHandler<JoinPdfFilesEventArgs>? JoinPdfFiles;

    #region Private

    private const string JoinPdfFilesDefaultButtonText = "Join PDF files";
	private const string JoinPdfFilesInProgressButtonText = "Joining PDF files...";

	private static readonly FilePickerOpenOptions InputPdfFilesOpenOptions;
	private static readonly FilePickerSaveOptions OutputPdfFileSaveOptions;

	private void OnMainWindowClosing(object? sender, WindowClosingEventArgs e)
	{
		e.Cancel = true;
	}

	private void OnClearInputPdfFilesClick(object sender, RoutedEventArgs e)
	{
		SetInitialUiStatus();
	}

	private async void OnAddInputPdfFilesClick(object sender, RoutedEventArgs e)
	{
		var inputPdfFiles = await StorageProvider.OpenFilePickerAsync(
			InputPdfFilesOpenOptions);

		var addedInputPdfFilePaths = inputPdfFiles
			.Select(anInputPdfFile => anInputPdfFile.Path.LocalPath)
			.ToList();

		foreach (var anAddedInputPdfFilePath in addedInputPdfFilePaths)
		{
			var anAddedInputPdfListItem = new ListBoxItem
			{
				Content = anAddedInputPdfFilePath,
				HorizontalContentAlignment = HorizontalAlignment.Center
			};

			_listBoxInputPdfFiles.Items.Add(anAddedInputPdfListItem);
		}

		_buttonClearInputPdfFiles.IsEnabled = IsEnabledButtonClearInputPdfFiles();
		_buttonJoinPdfFiles.IsEnabled = IsEnabledButtonJoinPdfFiles();
	}

	private async void OnJoinPdfFilesClick(object sender, RoutedEventArgs e)
	{
		var outputPdfFile = await StorageProvider.SaveFilePickerAsync(
			OutputPdfFileSaveOptions);

        if (outputPdfFile is not null)
        {
			var inputPdfFilePaths = GetInputPdfFilePaths();
			var outputPdfFilePath = outputPdfFile.Path.LocalPath;

			var joinPdfFilesEventArgs =
				new JoinPdfFilesEventArgs(inputPdfFilePaths, outputPdfFilePath);

			JoinPdfFiles?.Invoke(this, joinPdfFilesEventArgs);
		}
	}

	private void SetInitialUiStatus()
	{
		_listBoxInputPdfFiles.Items.Clear();

		_buttonClearInputPdfFiles.IsEnabled = false;
		_buttonJoinPdfFiles.Content = JoinPdfFilesDefaultButtonText;
		_buttonJoinPdfFiles.IsEnabled = false;
	}

	private bool IsEnabledButtonClearInputPdfFiles() => _listBoxInputPdfFiles.ItemCount >= 1;
	private bool IsEnabledButtonJoinPdfFiles() => _listBoxInputPdfFiles.ItemCount >= 2;

	private IReadOnlyList<string> GetInputPdfFilePaths()
		=> _listBoxInputPdfFiles.Items
				.Select(anItem => (ListBoxItem)anItem!)
				.Select(aListBoxItem => (string)aListBoxItem.Content!)
				.ToList();

	#endregion
}
