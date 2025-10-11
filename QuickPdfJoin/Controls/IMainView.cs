using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuickPdfJoin.CustomEventArgs;
using QuickPdfJoin.DataTypes;

namespace QuickPdfJoin.Controls;

public interface IMainView
{
	void Show();

	void PopulatePdfFiles(IReadOnlyList<FileInfo> pdfFiles);

	void SetUiEnabledState(bool isEnabled);

	Task ShowSuccessMessage(string successMessage);
	Task ShowErrorMessage(string errorMessage);

	event EventHandler<AddPdfFilesEventArgs>? AddPdfFiles;
	event EventHandler<JoinPdfFilesEventArgs>? JoinPdfFiles;
}
