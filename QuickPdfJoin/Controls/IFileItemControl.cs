using System;
using QuickPdfJoin.CustomEventArgs;

namespace QuickPdfJoin.Controls;

public interface IFileItemControl
{
	string? FileName { get; set; }

	bool CanMoveFileUp { get; set; }
	bool CanMoveFileDown { get; set; }

	event EventHandler<ReorderFileEventArgs>? ReorderFile;
}
