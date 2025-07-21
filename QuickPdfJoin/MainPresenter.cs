using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QuickPdfJoin.Controls;
using QuickPdfJoin.CustomEventArgs;
using QuickPdfJoin.DataTypes;
using QuickPdfJoin.Logic;

namespace QuickPdfJoin;

public class MainPresenter
{
	public MainPresenter(
		IPdfJoiner pdfJoiner,
		IMainView mainView)
	{
		_pdfJoiner = pdfJoiner;
		_mainView = mainView;

		_mainView.AddInputPdfFiles += OnAddInputPdfFiles;
		_mainView.JoinPdfFiles += OnJoinPdfFiles;
	}

	#region Private

	private readonly IPdfJoiner _pdfJoiner;
	private readonly IMainView _mainView;

	private void OnAddInputPdfFiles(object? sender, AddInputPdfFilesEventArgs e)
	{
		var inputPdfFilePaths = e.InputPdfFilePaths;

		var inputPdfFiles = inputPdfFilePaths
			.Select(anInputPdfFilePath => new PdfFileInfo(
				GetFileNameFromPath(anInputPdfFilePath), anInputPdfFilePath))
			.ToList();

		_mainView.PopulateInputPdfFiles(inputPdfFiles);
	}

	private async void OnJoinPdfFiles(object? sender, JoinPdfFilesEventArgs e)
	{
		var inputPdfFiles = e.InputPdfFiles;
		var outputPdfFilePath = e.OutputPdfFilePath;

		var inputPdfFilePaths = inputPdfFiles
			.Select(anInputPdfFile => anInputPdfFile.FilePath)
			.ToList();

		var outputPdfFile = new PdfFileInfo(
			GetFileNameFromPath(outputPdfFilePath), outputPdfFilePath);

		if (HasInputOutputFileCollision(inputPdfFilePaths, outputPdfFilePath))
		{
			var errorMessage = GetInputOutputFileCollisionErrorMessage(outputPdfFile);
			await _mainView.ShowErrorMessage(errorMessage);
		}
		else
		{
			try
			{
				_mainView.SetUiEnabledState(false);

				await JoinPdfDocuments(inputPdfFilePaths, outputPdfFilePath);

				var successMessage = GetOutputFileSavedSuccessMessage(outputPdfFile);
				await _mainView.ShowSuccessMessage(successMessage);
			}
			catch
			{
				var errorMessage = GetOutputFileNotSavedErrorMessage(outputPdfFile);
				await _mainView.ShowErrorMessage(errorMessage);
			}
			finally
			{
				_mainView.SetUiEnabledState(true);
			}
		}
	}

	private async Task JoinPdfDocuments(IReadOnlyList<string> inputPdfFiles, string outputPdfFile)
		=> await Task.Run(() => _pdfJoiner.JoinPdfDocuments(inputPdfFiles, outputPdfFile));

	private static string GetFileNameFromPath(string filePath) => Path.GetFileName(filePath);

	private static bool HasInputOutputFileCollision(
		IReadOnlyList<string> inputPdfFilePaths, string outputPdfFilePath) =>
			inputPdfFilePaths.Contains(outputPdfFilePath, StringComparer.InvariantCultureIgnoreCase);

	private static string GetInputOutputFileCollisionErrorMessage(PdfFileInfo outputPdfFile)
		=> $@"Cannot save output PDF file ""{outputPdfFile.FileName}"", since it would overwrite one of the input PDF files!";

	private static string GetOutputFileSavedSuccessMessage(PdfFileInfo outputPdfFile)
		=> $@"Output PDF file ""{outputPdfFile.FileName}"" has been successfully saved.";

	private static string GetOutputFileNotSavedErrorMessage(PdfFileInfo outputPdfFile)
		=> $@"Could not save output PDF file ""{outputPdfFile.FileName}""!";

	#endregion
}
