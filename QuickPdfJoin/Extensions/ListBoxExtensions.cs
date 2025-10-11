using System.Collections.Generic;
using Avalonia.Controls;

namespace QuickPdfJoin.Extensions;

public static class ListBoxExtensions
{
	public static IReadOnlyList<int> GetSelectedIndices(this ListBox listBox)
	{
		var selectedIndices = new List<int>();

		var selectedItems = listBox.SelectedItems;

		if (selectedItems is not null)
		{
			for (var i = 0; i < listBox.ItemCount; i++)
			{
				if (selectedItems.Contains(listBox.Items[i]))
				{
					selectedIndices.Add(i);
				}
			}
		}

		return selectedIndices;
	}
}
