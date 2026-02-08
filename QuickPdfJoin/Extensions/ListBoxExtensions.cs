using System.Collections.Generic;
using Avalonia.Controls;

namespace QuickPdfJoin.Extensions;

public static class ListBoxExtensions
{
	extension(ListBox listBox)
	{
		public IReadOnlyList<int> GetSelectedIndices()
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
}
