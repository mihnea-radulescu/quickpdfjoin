using System;
using System.Collections.Generic;
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

		_mainView.AddPdfFiles += OnAddPdfFiles;
		_mainView.JoinPdfFiles += OnJoinPdfFiles;
	}

	#region Private

	private readonly IPdfJoiner _pdfJoiner;
	private readonly IMainView _mainView;

	private void OnAddPdfFiles(object? sender, AddPdfFilesEventArgs e)
	{
		var pdfFilePaths = e.PdfFilePaths;

		var pdfFiles = pdfFilePaths
			.Select(aPdfFilePath => new FileInfo(GetFileNameFromPath(aPdfFilePath), aPdfFilePath))
			.ToList();

		_mainView.PopulatePdfFiles(pdfFiles);
	}

	private async void OnJoinPdfFiles(object? sender, JoinPdfFilesEventArgs e)
	{
		var inputPdfFiles = e.InputPdfFiles;
		var outputPdfFilePath = e.OutputPdfFilePath;

		var inputPdfFilePaths = inputPdfFiles
			.Select(anInputPdfFile => anInputPdfFile.FilePath)
			.ToList();

		var outputPdfFileName = GetFileNameFromPath(outputPdfFilePath);
		var outputPdfFile = new FileInfo(outputPdfFileName, outputPdfFilePath);

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

	private static string GetFileNameFromPath(string filePath)
		=> System.IO.Path.GetFileName(filePath);

	private static bool HasInputOutputFileCollision(
		IReadOnlyList<string> inputPdfFilePaths, string outputPdfFilePath) =>
			inputPdfFilePaths.Contains(outputPdfFilePath, StringComparer.InvariantCultureIgnoreCase);

	private static string GetInputOutputFileCollisionErrorMessage(FileInfo outputPdfFile)
		=> $@"Cannot save output PDF file ""{outputPdfFile.FileName}"", since it would overwrite one of the input PDF files!";

	private static string GetOutputFileSavedSuccessMessage(FileInfo outputPdfFile)
		=> $@"Output PDF file ""{outputPdfFile.FileName}"" has been successfully saved.";

	private static string GetOutputFileNotSavedErrorMessage(FileInfo outputPdfFile)
		=> $@"Could not save output PDF file ""{outputPdfFile.FileName}""!";

	#endregion
}
