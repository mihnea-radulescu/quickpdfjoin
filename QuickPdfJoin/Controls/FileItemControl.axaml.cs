using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QuickPdfJoin.CustomEventArgs;
using QuickPdfJoin.DataTypes;

namespace QuickPdfJoin.Controls;

public partial class FileItemControl : UserControl, IFileItemControl
{
	static FileItemControl()
	{
		MoveUpFileEventArgs = new ReorderFileEventArgs(ReorderType.MoveUp);
		MoveDownFileEventArgs = new ReorderFileEventArgs(ReorderType.MoveDown);
	}

	public FileItemControl()
	{
		InitializeComponent();
	}

	public string? FileName
	{
		get => _fileName;
		set
		{
			_fileName = value;

			if (_fileName is not null)
			{
				_fileNameTextBlock.Text = _fileName;
			}
		}
	}

	public bool CanMoveFileUp
	{
		get => _canMoveFileUp;
		set
		{
			_canMoveFileUp = value;

			_moveFileUpBorder.IsVisible = _canMoveFileUp;
		}
	}

	public bool CanMoveFileDown
	{
		get => _canMoveFileDown;
		set
		{
			_canMoveFileDown = value;

			_moveFileDownBorder.IsVisible = _canMoveFileDown;
		}
	}

	public event EventHandler<ReorderFileEventArgs>? ReorderFile;

	private static readonly ReorderFileEventArgs MoveUpFileEventArgs;
	private static readonly ReorderFileEventArgs MoveDownFileEventArgs;

	private string? _fileName;

	private bool _canMoveFileUp;
	private bool _canMoveFileDown;

	private void OnMoveFileUp(object? sender, RoutedEventArgs e) => ReorderFile?.Invoke(this, MoveUpFileEventArgs);
	private void OnMoveFileDown(object? sender, RoutedEventArgs e) => ReorderFile?.Invoke(this, MoveDownFileEventArgs);
}
