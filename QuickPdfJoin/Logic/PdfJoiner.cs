using System.Collections.Generic;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;

namespace QuickPdfJoin.Logic;

public class PdfJoiner : IPdfJoiner
{
	public void JoinPdfDocuments(IReadOnlyList<string> inputPdfFiles, string outputPdfFile)
	{
		using (var outputPdfDocument = new PdfDocument(new PdfWriter(outputPdfFile)))
		{
			var pdfMerger = new PdfMerger(outputPdfDocument);

			foreach (var anInputPdfFile in inputPdfFiles)
			{
				using (var anInputPdfDocument = new PdfDocument(new PdfReader(anInputPdfFile)))
				{
					var anInputPdfDocumentPageCount = anInputPdfDocument.GetNumberOfPages();

					pdfMerger.Merge(anInputPdfDocument, 1, anInputPdfDocumentPageCount);
				}
			}
		}
	}
}
