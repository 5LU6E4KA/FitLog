using FitLog.Controls;
using FitLog.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
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
using static FitLog.Controls.CustomMessageBox;

namespace FitLog.Windows
{
    /// <summary>
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void txtEmail_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtEmail.Text) && txtEmail.Text.Length > 0)
                textEmail.Visibility = Visibility.Collapsed;
            else
                textEmail.Visibility = Visibility.Visible;
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

        private void textEmail_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtEmail.Focus();
        }

        private void TransitionToAuthorizationWindow(object sender, RoutedEventArgs e)
        {
            new AuthorizationWindow().Show();
            this.Close();
        }

        private void NavigationToMainMenu(Users user)
        {
            new MainMenuWindow(user).Show();
            this.Close();
        }

        private void SignIn(object sender, RoutedEventArgs e)
        {
            var registrationResult = Registration(txtEmail.Text, passwordBox.Password);
            if (registrationResult != null)
            {
                NavigationToMainMenu(registrationResult);
            }
        }

        public Users Registration(string login, string password)
        {
            if (new[] { login, password }.Any(x => String.IsNullOrWhiteSpace(x)))
            {
               CustomMessageBox.Show("Проверьте заполненность полей!", "Регистрация", MessageWindowImage.Error, MessageWindowButton.Ok);
                return null;
            }

            if (password.Length < 8 || password.Length > 20)
            {
                CustomMessageBox.Show("Минимальная длина пароля - 8 символов, а максимальная длина - 20 символов", "Регистрация", MessageWindowImage.Information, MessageWindowButton.Ok);
                return null;
            }

            if (UserExists(login))
            {
                CustomMessageBox.Show("Пользователь с таким логином уже существует", "Регистрация", MessageWindowImage.Error, MessageWindowButton.Ok);
                return null;
            }
            var user = new Users { Login = login, Password = HashingData.HashingPassword.HashPassword(password) };
            RegisterUser(user);
            CustomMessageBox.Show("Вы успешно зарегистрировались", "Регистрация", MessageWindowImage.Information, MessageWindowButton.Ok);

            return user;

        }

        public bool UserExists(string login)
        {
            return DatabaseContext.DbContext.Context.Users.FirstOrDefault(x => x.Login == login) != null;
        }

        private void RegisterUser(Users user)
        {
            try
            {
                DatabaseContext.DbContext.Context.Users.Add(user);
                DatabaseContext.DbContext.Context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(eve => eve.ValidationErrors)
                    .Select(ve => ve.ErrorMessage);

                var fullErrorMessage = string.Join("; ", errorMessages);

                CustomMessageBox.ShowMessage($"Ошибка при валидации: {fullErrorMessage}");
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            var result = CustomMessageBox.Show("Вы точно хотите выйти из приложения?", "Выход", MessageWindowImage.Information, MessageWindowButton.OkCancel);

            if (result == MessageWindowResult.Ok)
            {
                Application.Current.Shutdown();
            }
        }

    }
}
