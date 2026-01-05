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
using QuickPdfJoin.Extensions;

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

		_joinPdfFilesButton.Content = DefaultJoinPdfFilesButtonText;

		SetButtonsEnabledState();
	}

	public void PopulatePdfFiles(IReadOnlyList<FileInfo> pdfFiles)
	{
		const int firstPdfIndex = 0;

		for (var i = 0; i < pdfFiles.Count; i++)
		{
			var aPdfFile = pdfFiles[i];

			IFileItemControl pdfFileItemControl = new FileItemControl
			{
				FileName = aPdfFile.FileName
			};

			pdfFileItemControl.ReorderFile += OnReorderPdfFiles;

			var pdfFileListBoxItem = new ListBoxItem
			{
				Tag = aPdfFile,
				Content = pdfFileItemControl,
				HorizontalContentAlignment = DefaultHorizontalAlignment
			};

			_pdfFilesListBox.Items.Add(pdfFileListBoxItem);

			if (i == firstPdfIndex)
			{
				_pdfFilesListBox.SelectedItem = pdfFileListBoxItem;
			}
		}
	}

	public void SetUiEnabledState(bool isEnabled)
	{
		if (isEnabled)
		{
			Closing -= OnMainWindowClosing;
			_joinPdfFilesButton.Content = DefaultJoinPdfFilesButtonText;

			SetButtonsEnabledState();
		}
		else
		{
			Closing += OnMainWindowClosing;
			_joinPdfFilesButton.Content = InProgressJoinPdfFilesButtonText;

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
			null,
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
			null,
			WindowStartupLocation.CenterOwner);

		await errorMessageBox.ShowWindowDialogAsync(this);
	}

	public event EventHandler<AddPdfFilesEventArgs>? AddPdfFiles;
	public event EventHandler<JoinPdfFilesEventArgs>? JoinPdfFiles;

	private const string DefaultJoinPdfFilesButtonText = "Join PDF files";
	private const string InProgressJoinPdfFilesButtonText = "Joining PDF files...";

	private static readonly IReadOnlyList<FilePickerFileType> SupportedFileTypes;

	private const HorizontalAlignment DefaultHorizontalAlignment = HorizontalAlignment.Center;

	private static void OnMainWindowClosing(object? sender, WindowClosingEventArgs e)
	{
		e.Cancel = true;
	}

	private void OnClearPdfFilesClick(object? sender, RoutedEventArgs e)
	{
		var listBoxItems = GetListBoxItems();
		RemoveListBoxItems(listBoxItems);

		SetButtonsEnabledState();
	}

	private async void OnAddPdfFilesClick(object? sender, RoutedEventArgs e)
	{
		var pdfFilesOpenOptions = new FilePickerOpenOptions
		{
			AllowMultiple = true,
			FileTypeFilter = SupportedFileTypes,
			Title = "Select PDF File or Files"
		};

		var pdfStorageFiles = await StorageProvider.OpenFilePickerAsync(pdfFilesOpenOptions);

		if (pdfStorageFiles.Any())
		{
			var pdfFilePaths = pdfStorageFiles
				.Select(aPdfFile => aPdfFile.Path.LocalPath)
				.ToList();

			var addPdfFilesEventArgs = new AddPdfFilesEventArgs(pdfFilePaths);
			AddPdfFiles?.Invoke(this, addPdfFilesEventArgs);

			UpdatePdfFileItemsReorderFlags();
			SetButtonsEnabledState();
		}
	}

	private void OnRemoveSelectedPdfFilesClick(object? sender, RoutedEventArgs e)
	{
		var selectedListBoxItems = GetSelectedListBoxItems();
		var selectedIndices = _pdfFilesListBox.GetSelectedIndices();

		RemoveListBoxItems(selectedListBoxItems);

		if (HasAnyPdfFiles())
		{
			var indexToSelect = selectedIndices.Min() - 1;
			var normalizedIndexToSelect = indexToSelect >= 0 ? indexToSelect : 0;

			_pdfFilesListBox.SelectedIndex = normalizedIndexToSelect;

			UpdatePdfFileItemsReorderFlags();
		}

		SetButtonsEnabledState();
	}

	private void OnReorderPdfFiles(object? sender, ReorderFileEventArgs e)
	{
		var selectedListBoxItems = GetSelectedListBoxItems();
		var selectedSingleListBoxItem = selectedListBoxItems.Single();

		var selectedFileItemControl = (IFileItemControl)selectedSingleListBoxItem.Content!;
		selectedFileItemControl.ReorderFile -= OnReorderPdfFiles;

		IFileItemControl newFileItemControl = new FileItemControl
		{
			FileName = selectedFileItemControl.FileName
		};

		newFileItemControl.ReorderFile += OnReorderPdfFiles;

		var newListBoxItem = new ListBoxItem
		{
			Tag = selectedSingleListBoxItem.Tag,
			Content = newFileItemControl,
			HorizontalContentAlignment = DefaultHorizontalAlignment
		};

		var selectedIndex = _pdfFilesListBox.Items.IndexOf(selectedSingleListBoxItem);
		_pdfFilesListBox.Items.RemoveAt(selectedIndex);

		var reorderType = e.ReorderType;

		switch (reorderType)
		{
			case ReorderType.MoveUp:
				_pdfFilesListBox.Items.Insert(selectedIndex - 1, newListBoxItem);
				break;

			case ReorderType.MoveDown:
				_pdfFilesListBox.Items.Insert(selectedIndex + 1, newListBoxItem);
				break;
		}

		_pdfFilesListBox.SelectedItem = newListBoxItem;

		UpdatePdfFileItemsReorderFlags();
	}

	private async void OnJoinPdfFilesClick(object? sender, RoutedEventArgs e)
	{
		var outputPdfFileSaveOptions = new FilePickerSaveOptions
		{
			DefaultExtension = ".pdf",
			SuggestedFileName = "Joined.pdf",
			FileTypeChoices = SupportedFileTypes,
			ShowOverwritePrompt = true,
			Title = "Select Output PDF File"
		};

		var outputPdfStorageFile = await StorageProvider.SaveFilePickerAsync(outputPdfFileSaveOptions);

		if (outputPdfStorageFile is not null)
		{
			var inputPdfFiles = GetInputPdfFiles();
			var outputPdfFilePath = outputPdfStorageFile.Path.LocalPath;

			var joinPdfFilesEventArgs = new JoinPdfFilesEventArgs(inputPdfFiles, outputPdfFilePath);
			JoinPdfFiles?.Invoke(this, joinPdfFilesEventArgs);
		}
	}

	private void OnPdfFilesListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		_removeSelectedPdfFilesButton.IsEnabled = HasPdfFilesSelected();
	}

	private void SetButtonsEnabledState()
	{
		_clearPdfFilesButton.IsEnabled = HasAnyPdfFiles();
		_addPdfFilesButton.IsEnabled = true;
		_removeSelectedPdfFilesButton.IsEnabled = HasPdfFilesSelected();
		_joinPdfFilesButton.IsEnabled = CanJoinPdfFiles();
	}

	private void DisableButtons()
	{
		_clearPdfFilesButton.IsEnabled = false;
		_addPdfFilesButton.IsEnabled = false;
		_removeSelectedPdfFilesButton.IsEnabled = false;
		_joinPdfFilesButton.IsEnabled = false;
	}

	private bool HasAnyPdfFiles() => _pdfFilesListBox.ItemCount >= 1;
	private bool CanJoinPdfFiles() => _pdfFilesListBox.ItemCount >= 2;
	private bool HasPdfFilesSelected() => _pdfFilesListBox.SelectedItems!.Count > 0;

	private IReadOnlyList<FileInfo> GetInputPdfFiles()
	{
		var listBoxItems = GetListBoxItems();

		var inputPdfFiles = listBoxItems
			.Select(aListBoxItem => (FileInfo)aListBoxItem.Tag!)
			.ToList();

		return inputPdfFiles;
	}

	private IReadOnlyList<ListBoxItem> GetListBoxItems()
		=> _pdfFilesListBox.Items
			.Cast<ListBoxItem>()
			.ToList();

	private IReadOnlyList<ListBoxItem> GetSelectedListBoxItems()
		=> _pdfFilesListBox.SelectedItems!
			.Cast<ListBoxItem>()
			.ToList();

	private void RemoveListBoxItems(IReadOnlyList<ListBoxItem> listBoxItems)
	{
		var fileItemControls = listBoxItems
			.Select(aListBoxItem => (IFileItemControl)aListBoxItem.Content!)
			.ToList();

		foreach (var aFileItemControl in fileItemControls)
		{
			aFileItemControl.ReorderFile -= OnReorderPdfFiles;
		}

		foreach (var aListBoxItem in listBoxItems)
		{
			_pdfFilesListBox.Items.Remove(aListBoxItem);
		}
	}

	private void UpdatePdfFileItemsReorderFlags()
	{
		var listBoxItems = GetListBoxItems();

		var pdfFileItemControls = listBoxItems
			.Select(aListBoxItem => (IFileItemControl)aListBoxItem.Content!)
			.ToList();

		const int firstPdfIndex = 0;
		var lastPdfIndex = pdfFileItemControls.Count - 1;

		for (var i = 0; i < pdfFileItemControls.Count; i++)
		{
			var aPdfFileItemControl = pdfFileItemControls[i];

			aPdfFileItemControl.CanMoveFileUp = true;
			aPdfFileItemControl.CanMoveFileDown = true;

			if (i == firstPdfIndex)
			{
				aPdfFileItemControl.CanMoveFileUp = false;
			}

			if (i == lastPdfIndex)
			{
				aPdfFileItemControl.CanMoveFileDown = false;
			}
		}
	}

	private static IReadOnlyList<FilePickerFileType> GetSupportedFileTypes()
	{
		var supportedFileTypes = new List<FilePickerFileType>
		{
			new("PDF files")
			{
				Patterns = ["*.pdf", "*.PDF"]
			}
		};

		return supportedFileTypes;
	}
}
