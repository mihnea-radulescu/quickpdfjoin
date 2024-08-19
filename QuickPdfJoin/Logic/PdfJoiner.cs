using System.Collections.Generic;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;

namespace QuickPdfJoin.Logic;

public class PdfJoiner : IPdfJoiner
{
	static PdfJoiner()
	{
		WriterProperties = new WriterProperties()
			.SetCompressionLevel(CompressionConstants.BEST_COMPRESSION)
			.SetFullCompressionMode(true)
			.UseSmartMode();
	}
	
	public void JoinPdfDocuments(IReadOnlyList<string> inputPdfFiles, string outputPdfFile)
	{
		using (var outputPdfDocument = new PdfDocument(new PdfWriter(outputPdfFile, WriterProperties)))
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
	
	#region Private

	private static readonly WriterProperties WriterProperties;

	#endregion
}
