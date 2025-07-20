using Avalonia;
using Avalonia.Markup.Xaml;
using QuickPdfJoin.Controls;
using QuickPdfJoin.Logic;

namespace QuickPdfJoin;

public class App : Application
{
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		IPdfJoiner pdfJoiner = new PdfJoiner();
		IMainView mainView = new MainWindow();

		var mainPresenter = new MainPresenter(pdfJoiner, mainView);
		mainView.Show();
	}
}
