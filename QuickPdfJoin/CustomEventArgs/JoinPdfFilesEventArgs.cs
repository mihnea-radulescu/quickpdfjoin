using System;
using System.Collections.Generic;
using QuickPdfJoin.DataTypes;

namespace QuickPdfJoin.CustomEventArgs;

public class JoinPdfFilesEventArgs : EventArgs
{
	public JoinPdfFilesEventArgs(
		IReadOnlyList<PdfFileInfo> inputPdfFiles,
		string outputPdfFilePath)
	{
		InputPdfFiles = inputPdfFiles;
		OutputPdfFilePath = outputPdfFilePath;
	}

	public IReadOnlyList<PdfFileInfo> InputPdfFiles { get; }
	public string OutputPdfFilePath { get; }
}
