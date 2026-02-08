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

	public void JoinPdfDocuments(
		IReadOnlyList<string> inputPdfFiles, string outputPdfFile)
	{
		using var outputPdfWriter = new PdfWriter(
			outputPdfFile, WriterProperties);
		using var outputPdfDocument = new PdfDocument(outputPdfWriter);

		var outputPdfMerger = new PdfMerger(outputPdfDocument);

		foreach (var anInputPdfFile in inputPdfFiles)
		{
			using var anInputPdfReader = new PdfReader(anInputPdfFile);
			using var anInputPdfDocument = new PdfDocument(anInputPdfReader);

			var anInputPdfDocumentPageCount =
				anInputPdfDocument.GetNumberOfPages();
			outputPdfMerger.Merge(
				anInputPdfDocument, 1, anInputPdfDocumentPageCount);
		}
	}

	private static readonly WriterProperties WriterProperties;
}
