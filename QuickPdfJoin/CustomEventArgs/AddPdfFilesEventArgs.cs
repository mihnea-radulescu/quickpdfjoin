using System;
using System.Collections.Generic;

namespace QuickPdfJoin.CustomEventArgs;

public class AddPdfFilesEventArgs : EventArgs
{
	public AddPdfFilesEventArgs(IReadOnlyList<string> pdfFilePaths)
	{
		PdfFilePaths = pdfFilePaths;
	}

	public IReadOnlyList<string> PdfFilePaths { get; }
}
