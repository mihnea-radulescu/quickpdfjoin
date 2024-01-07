using System;
using System.Threading.Tasks;
using QuickPdfJoin.CustomEventArgs;

namespace QuickPdfJoin.Views;

public interface IMainView
{
	void Show();
	
	void SetUiEnabledStatus(bool isEnabled);

	Task ShowSuccessMessage(string successMessage);
	Task ShowErrorMessage(string errorMessage);

	event EventHandler<JoinPdfFilesEventArgs>? JoinPdfFiles;
}
