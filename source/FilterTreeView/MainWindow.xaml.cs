namespace FilterTreeView
{
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
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;

            var appVM = new ViewModels.AppViewModel();
            this.DataContext = appVM;

            await appVM.LoadSampleDataAsync();
        }
    }
}
