using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuickPdfJoin.CustomEventArgs;
using QuickPdfJoin.DataTypes;

namespace QuickPdfJoin.Controls;

public interface IMainView
{
	void Show();

	void PopulateInputPdfFiles(IReadOnlyList<PdfFileInfo> inputPdfFiles);

	void SetUiEnabledState(bool isEnabled);

	Task ShowSuccessMessage(string successMessage);
	Task ShowErrorMessage(string errorMessage);

	event EventHandler<AddInputPdfFilesEventArgs>? AddInputPdfFiles;
	event EventHandler<JoinPdfFilesEventArgs>? JoinPdfFiles;
}
