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
using QuickPdfJoin.DataTypes;

namespace QuickPdfJoin.Controls;

public partial class MainWindow : Window, IMainView
{
	static MainWindow()
	{
		SupportedFileTypes = GetSupportedFileTypes();
	}

	public MainWindow()
	{
		InitializeComponent();

		SetInitialUiState();
	}

	public void PopulateInputPdfFiles(IReadOnlyList<PdfFileInfo> inputPdfFiles)
	{
		foreach (var anInputPdfFile in inputPdfFiles)
		{
			var inputPdfFileListBoxItem = new ListBoxItem
			{
				Tag = anInputPdfFile,
				Content = anInputPdfFile.FileName,
				HorizontalContentAlignment = HorizontalAlignment.Center
			};

			_listBoxInputPdfFiles.Items.Add(inputPdfFileListBoxItem);
		}
	}

	public void SetUiEnabledState(bool isEnabled)
	{
		if (isEnabled)
		{
			Closing -= OnMainWindowClosing;
			_buttonJoinPdfFiles.Content = JoinPdfFilesDefaultButtonText;

			SetButtonsEnabledState();
		}
		else
		{
			Closing += OnMainWindowClosing;
			_buttonJoinPdfFiles.Content = JoinPdfFilesInProgressButtonText;

			DisableButtons();
		}
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

	public event EventHandler<AddInputPdfFilesEventArgs>? AddInputPdfFiles;
	public event EventHandler<JoinPdfFilesEventArgs>? JoinPdfFiles;

	#region Private

	private const string JoinPdfFilesDefaultButtonText = "Join PDF files";
	private const string JoinPdfFilesInProgressButtonText = "Joining PDF files...";

	private static readonly IReadOnlyList<FilePickerFileType> SupportedFileTypes;

	private static void OnMainWindowClosing(object? sender, WindowClosingEventArgs e)
	{
		e.Cancel = true;
	}

	private void OnClearInputPdfFilesClick(object sender, RoutedEventArgs e)
	{
		SetInitialUiState();
	}

	private async void OnAddInputPdfFilesClick(object sender, RoutedEventArgs e)
	{
		var inputPdfFilesOpenOptions = new FilePickerOpenOptions
		{
			AllowMultiple = true,
			FileTypeFilter = SupportedFileTypes,
			Title = "Select Input PDF File or Files"
		};

		var inputPdfStorageFiles = await StorageProvider.OpenFilePickerAsync(
			inputPdfFilesOpenOptions);

		var inputPdfFilePaths = inputPdfStorageFiles
			.Select(anInputPdfFile => anInputPdfFile.Path.LocalPath)
			.ToList();

		var addInputPdfFilesEventArgs = new AddInputPdfFilesEventArgs(inputPdfFilePaths);
		AddInputPdfFiles?.Invoke(this, addInputPdfFilesEventArgs);

		SetButtonsEnabledState();
	}

	private void OnRemoveInputPdfFilesClick(object sender, RoutedEventArgs e)
	{
		var selectedItems = _listBoxInputPdfFiles.SelectedItems!
			.Cast<ListBoxItem>()
			.ToList();

		foreach (var aSelectedItem in selectedItems)
		{
			_listBoxInputPdfFiles.Items.Remove(aSelectedItem);
		}

		SetButtonsEnabledState();
	}

	private async void OnJoinPdfFilesClick(object sender, RoutedEventArgs e)
	{
		var outputPdfFileSaveOptions = new FilePickerSaveOptions
		{
			DefaultExtension = ".pdf",
			SuggestedFileName = "Joined.pdf",
			FileTypeChoices = SupportedFileTypes,
			ShowOverwritePrompt = true,
			Title = "Select Output PDF File"
		};

		var outputPdfStorageFile = await StorageProvider.SaveFilePickerAsync(
			outputPdfFileSaveOptions);

		if (outputPdfStorageFile is not null)
		{
			var inputPdfFiles = GetInputPdfFiles();
			var outputPdfFilePath = outputPdfStorageFile.Path.LocalPath;

			var joinPdfFilesEventArgs = new JoinPdfFilesEventArgs(inputPdfFiles, outputPdfFilePath);
			JoinPdfFiles?.Invoke(this, joinPdfFilesEventArgs);
		}
	}

	private void OnListBoxInputPdfFilesSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		_buttonRemoveInputPdfFiles.IsEnabled = HasInputPdfFilesSelected();
	}

	private void SetInitialUiState()
	{
		_listBoxInputPdfFiles.Items.Clear();

		_buttonJoinPdfFiles.Content = JoinPdfFilesDefaultButtonText;

		SetButtonsEnabledState();
	}

	private void SetButtonsEnabledState()
	{
		_buttonClearInputPdfFiles.IsEnabled = CanClearInputPdfFiles();
		_buttonAddInputPdfFiles.IsEnabled = true;
		_buttonRemoveInputPdfFiles.IsEnabled = HasInputPdfFilesSelected();
		_buttonJoinPdfFiles.IsEnabled = CanJoinPdfFiles();
	}

	private void DisableButtons()
	{
		_buttonClearInputPdfFiles.IsEnabled = false;
		_buttonAddInputPdfFiles.IsEnabled = false;
		_buttonRemoveInputPdfFiles.IsEnabled = false;
		_buttonJoinPdfFiles.IsEnabled = false;
	}

	private bool CanClearInputPdfFiles() => _listBoxInputPdfFiles.ItemCount >= 1;
	private bool CanJoinPdfFiles() => _listBoxInputPdfFiles.ItemCount >= 2;
	private bool HasInputPdfFilesSelected() => _listBoxInputPdfFiles.SelectedItems!.Count > 0;

	private IReadOnlyList<PdfFileInfo> GetInputPdfFiles()
		=> _listBoxInputPdfFiles.Items
			.Cast<ListBoxItem>()
			.Select(aListBoxItem => (PdfFileInfo)aListBoxItem.Tag!)
			.ToList();

	private static IReadOnlyList<FilePickerFileType> GetSupportedFileTypes()
	{
		var supportedFileTypes = new List<FilePickerFileType>
		{
			new FilePickerFileType("PDF files")
			{
				Patterns = ["*.pdf", "*.PDF"]
			}
		};

		return supportedFileTypes;
	}

	#endregion
}
