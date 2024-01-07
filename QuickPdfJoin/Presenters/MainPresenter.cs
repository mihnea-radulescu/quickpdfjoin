using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuickPdfJoin.CustomEventArgs;
using QuickPdfJoin.Logic;
using QuickPdfJoin.Views;

namespace QuickPdfJoin.Presenters;

public class MainPresenter
{
	public MainPresenter(
		IPdfJoiner pdfJoiner,
		IMainView mainView)
    {
		_pdfJoiner = pdfJoiner;
		_mainView = mainView;

		_mainView.JoinPdfFiles += OnJoinPdfFiles;
	}

	#region Private

	private readonly IPdfJoiner _pdfJoiner;
	private readonly IMainView _mainView;

	private async void OnJoinPdfFiles(object? sender, JoinPdfFilesEventArgs e)
	{
		var inputPdfFilePaths = e.InputPdfFilePaths;
		var outputPdfFilePath = e.OutputPdfFilePath;

		if (HasInputOutputFileCollision(inputPdfFilePaths, outputPdfFilePath))
		{
			var errorMessage = GetInputOutputFileCollisionErrorMessage(outputPdfFilePath);
			await _mainView.ShowErrorMessage(errorMessage);
		}
		else
		{
			try
			{
				_mainView.SetUiEnabledStatus(false);

				await JoinPdfDocuments(inputPdfFilePaths, outputPdfFilePath);

				var successMessage = GetOutputFileSavedSuccessMessage(outputPdfFilePath);
				await _mainView.ShowErrorMessage(successMessage);
			}
			catch (Exception ex)
			{
				var errorMessage = GetOutputFileNotSavedErrorMessage(outputPdfFilePath, ex);
				await _mainView.ShowErrorMessage(errorMessage);
			}
			finally
			{
				_mainView.SetUiEnabledStatus(true);
			}
		}
	}

	private bool HasInputOutputFileCollision(
		IReadOnlyList<string> inputPdfFilePaths, string outputPdfFilePath) =>
			inputPdfFilePaths.Contains(outputPdfFilePath, StringComparer.InvariantCultureIgnoreCase);

	private async Task JoinPdfDocuments(IReadOnlyList<string> inputPdfFiles, string outputPdfFile)
		=> await Task.Run(() => _pdfJoiner.JoinPdfDocuments(inputPdfFiles, outputPdfFile));

	private string GetOutputFileSavedSuccessMessage(string outputPdfFilePath)
		=> $@"Output PDF file ""{outputPdfFilePath}"" has been successfully saved.";

	private string GetInputOutputFileCollisionErrorMessage(string outputPdfFilePath)
		=> $@"Cannot save output PDF file ""{outputPdfFilePath}"", since it would overwrite one of the input PDF files!";

	private string GetOutputFileNotSavedErrorMessage(string outputPdfFilePath, Exception ex)
		=> $@"Could not save output PDF file ""{outputPdfFilePath}""!{Environment.NewLine}{ex.Source}: {ex.Message}";

	#endregion
}
