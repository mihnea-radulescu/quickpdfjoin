using System;
using QuickPdfJoin.DataTypes;

namespace QuickPdfJoin.CustomEventArgs;

public class ReorderFileEventArgs : EventArgs
{
	public ReorderFileEventArgs(ReorderType reorderType)
	{
		ReorderType = reorderType;
	}

	public ReorderType ReorderType { get; }
}
