using FitLog.Entities;
using FitLog.HashingData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FitLog.Windows
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        public AuthorizationWindow()
        {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(passwordBox.Password) && passwordBox.Password.Length > 0)
                textPassword.Visibility = Visibility.Collapsed;
            else
                textPassword.Visibility = Visibility.Visible;
        }

        private void textPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            passwordBox.Focus();
        }

        private void txtEmail_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtEmail.Text) && txtEmail.Text.Length > 0)
                textEmail.Visibility = Visibility.Collapsed;
            else
                textEmail.Visibility = Visibility.Visible;
        }

        private void textEmail_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtEmail.Focus();
        }

        private void TransitionToRegistrationWindow(object sender, RoutedEventArgs e)
        {
            new RegistrationWindow().Show();
            this.Close();
        }

        private void NavigationToMainMenu(Users user)
        {
            new MainMenuWindow(user).Show();
            this.Close();
        }

        public Users Authorization(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Проверьте заполненность полей!", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Information);
                return null;
            }

            var user = DatabaseContext.DbContext.Context.Users.FirstOrDefault(x => x.Login == login);

            if (user == null)
            {
                MessageBox.Show("Пользователь не найден, Вам нужно пройти регистрацию", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Information);
                return null;
            }

            if (HashingPassword.VerifyPassword(password, user.Password))
            {
                MessageBox.Show("Авторизация прошла успешно!");
                return user;
            }
            else
            {
                MessageBox.Show("Неправильный пароль!", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Information);
                return null;
            }
        }

        private void Authentification(object sender, RoutedEventArgs e)
        {
            var registrationResult = Authorization(txtEmail.Text, passwordBox.Password);
            if (registrationResult != null)
            {
                NavigationToMainMenu(registrationResult);
            }
        }
    }
}
