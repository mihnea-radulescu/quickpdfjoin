using System;
using System.Threading.Tasks;
using QuickPdfJoin.CustomEventArgs;

namespace QuickPdfJoin.Controls;

public interface IMainView
{
	void Show();

	void SetUiEnabledState(bool isEnabled);

	Task ShowSuccessMessage(string successMessage);
	Task ShowErrorMessage(string errorMessage);

	event EventHandler<JoinPdfFilesEventArgs>? JoinPdfFiles;
}
