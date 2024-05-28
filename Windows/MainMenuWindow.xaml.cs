using FitLog.Controls;
using FitLog.Entities;
using FitLog.Pages;
using System.Windows;
using System.Windows.Input;
using static FitLog.Controls.CustomMessageBox;

namespace FitLog.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainMenuWindow.xaml
    /// </summary>
    public partial class MainMenuWindow : Window
    {
        private Users _currentUser;
        public MainMenuWindow(Users user)
        {
            InitializeComponent();
            _currentUser = user;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private bool IsMaximize = false;
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (IsMaximize)
                {
                    this.WindowState = WindowState.Normal;
                    this.Width = 1280;
                    this.Height = 780;

                    IsMaximize = false;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;

                    IsMaximize = true;
                }
            }
        }

        private void OutFromSystem(object sender, RoutedEventArgs e)
        {
            var result = CustomMessageBox.Show("Вы точно хотите выйти из приложения?", "Выход", MessageWindowImage.Information, MessageWindowButton.OkCancel);

            if (result == MessageWindowResult.Ok)
            {
                Application.Current.Shutdown();
            }
        }

        private void NavigateToMealPage(object sender, RoutedEventArgs e)
        {
            frameContent.Navigate(new MealPage(_currentUser));
        }

        private void NavigateToLiquidPage(object sender, RoutedEventArgs e)
        {
            frameContent.Navigate(new LiquidPage(_currentUser));
        }

        private void NavigateToSleepPage(object sender, RoutedEventArgs e)
        {
            frameContent.Navigate(new FrequencyOfRespiratoryMovementsPage(_currentUser));
        }

        private void NavigateToPulsePage(object sender, RoutedEventArgs e)
        {
            frameContent.Navigate(new PulsePage(_currentUser));
        }

        private void NavigateToTemperaturePage(object sender, RoutedEventArgs e)
        {
            frameContent.Navigate(new TemperaturePage(_currentUser));
        }

        private void NavigateToGlucosePage(object sender, RoutedEventArgs e)
        {
            frameContent.Navigate(new GlucosePage(_currentUser));
        }

        private void NavigateToWeightPage(object sender, RoutedEventArgs e)
        {
            frameContent.Navigate(new WeightPage(_currentUser));
        }

        private void NavigateToProfilePage(object sender, RoutedEventArgs e)
        {
            frameContent.Navigate(new ProfilePage(_currentUser));
        }
    }
}

