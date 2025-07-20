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

namespace QuickPdfJoin.Controls;

public partial class MainWindow : Window, IMainView
{
	static MainWindow()
	{
		DocumentsFolderPath = GetDocumentsFolderPath();
		SupportedFileTypes = GetSupportedFileTypes();
	}

	public MainWindow()
	{
		InitializeComponent();

		SetInitialUiState();
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

	public event EventHandler<JoinPdfFilesEventArgs>? JoinPdfFiles;

	#region Private

	private const string JoinPdfFilesDefaultButtonText = "Join PDF files";
	private const string JoinPdfFilesInProgressButtonText = "Joining PDF files...";

	private static readonly string DocumentsFolderPath;
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
		var documentsFolder = await GetDocumentsFolder();
		var inputPdfFilesOpenOptions = new FilePickerOpenOptions
		{
			AllowMultiple = true,
			FileTypeFilter = SupportedFileTypes,
			Title = "Select Input PDF File or Files",
			SuggestedStartLocation = documentsFolder
		};

		var inputPdfFiles = await StorageProvider.OpenFilePickerAsync(inputPdfFilesOpenOptions);

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
		var documentsFolder = await GetDocumentsFolder();
		var outputPdfFileSaveOptions = new FilePickerSaveOptions
		{
			DefaultExtension = ".pdf",
			FileTypeChoices = SupportedFileTypes,
			ShowOverwritePrompt = true,
			Title = "Select Output PDF File",
			SuggestedStartLocation = documentsFolder
		};

		var outputPdfFile = await StorageProvider.SaveFilePickerAsync(outputPdfFileSaveOptions);

		if (outputPdfFile is not null)
		{
			var inputPdfFilePaths = GetInputPdfFilePaths();
			var outputPdfFilePath = outputPdfFile.Path.LocalPath;

			var joinPdfFilesEventArgs =
				new JoinPdfFilesEventArgs(inputPdfFilePaths, outputPdfFilePath);

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

	private IReadOnlyList<string> GetInputPdfFilePaths()
		=> _listBoxInputPdfFiles.Items
				.Select(anItem => (ListBoxItem)anItem!)
				.Select(aListBoxItem => (string)aListBoxItem.Content!)
				.ToList();

	private async Task<IStorageFolder?> GetDocumentsFolder()
		=> await StorageProvider.TryGetFolderFromPathAsync(DocumentsFolderPath);

	private static string GetDocumentsFolderPath() => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

	private static IReadOnlyList<FilePickerFileType> GetSupportedFileTypes()
	{
		var supportedFileTypes = new List<FilePickerFileType>
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

		return supportedFileTypes;
	}

	#endregion
}
