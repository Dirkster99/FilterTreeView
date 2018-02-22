namespace FilterTreeView
{
    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Unloaded += MainWindow_Unloaded;
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            var appVM = this.DataContext as IDisposable;

            if (appVM != null)
                appVM.Dispose();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;

            var appVM = new FilterTreeView.ViewModels.AppViewModel();
            this.DataContext = appVM;

            await appVM.LoadSampleDataAsync();
        }
    }
}
