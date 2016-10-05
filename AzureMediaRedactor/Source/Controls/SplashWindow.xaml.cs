using System.ComponentModel;
using System.Windows;

namespace AzureMediaRedactor.Controls
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        private BackgroundWorker _worker;

        public SplashWindow(BackgroundWorker worker)
        {
            _worker = worker;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
            InitializeComponent();
            Style = FindResource(typeof(Window)) as Style;
            Owner = Application.Current.MainWindow;
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = _worker.IsBusy;
        }
    }
}
