using Avalonia;
using Avalonia.Markup.Xaml;
using QuickPdfJoin.Logic;
using QuickPdfJoin.Presenters;
using QuickPdfJoin.Views;

namespace QuickPdfJoin;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        IPdfJoiner pdfJoiner = new PdfJoiner();
        IMainView mainView = new MainWindow();

        MainPresenter mainPresenter = new MainPresenter(pdfJoiner, mainView);
        mainView.Show();
    }
}
