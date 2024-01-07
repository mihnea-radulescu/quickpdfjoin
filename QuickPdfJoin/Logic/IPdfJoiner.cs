using System.Collections.Generic;

namespace QuickPdfJoin.Logic;

public interface IPdfJoiner
{
	void JoinPdfDocuments(IReadOnlyList<string> inputPdfFiles, string outputPdfFile);
}
