using System;
using System.Collections.Generic;

namespace QuickPdfJoin.CustomEventArgs;

public class AddInputPdfFilesEventArgs : EventArgs
{
	public AddInputPdfFilesEventArgs(IReadOnlyList<string> inputPdfFilePaths)
	{
		InputPdfFilePaths = inputPdfFilePaths;
	}

	public IReadOnlyList<string> InputPdfFilePaths { get; }
}
