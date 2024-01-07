using System;
using System.Collections.Generic;

namespace QuickPdfJoin.CustomEventArgs;

public class JoinPdfFilesEventArgs : EventArgs
{
    public JoinPdfFilesEventArgs(
        IReadOnlyList<string> inputPdfFilePaths,
        string outputPdfFilePath)
    {
        InputPdfFilePaths = inputPdfFilePaths;
        OutputPdfFilePath = outputPdfFilePath;
    }

    public IReadOnlyList<string> InputPdfFilePaths { get; }
    public string OutputPdfFilePath { get; }
}
