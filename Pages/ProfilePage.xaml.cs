using FitLog.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Mail;
using MimeKit;
using MailKit.Net.Smtp;

namespace FitLog.Pages
{

    public partial class ProfilePage : Page
    {
        private Users _currentUser;
        private Timer _foodGoalTimer;
        private Timer _liquidGoalTimer;
        private Timer _weightGoalTimer;
        private Timer _sleepGoalTimer;
        private bool _isGoalChanged = false;

        public ObservableCollection<string> SelectedFiles { get; set; } = new ObservableCollection<string>();


        public ProfilePage(Users user)
        {
            InitializeComponent();
            _currentUser = user;

            // Инициализация таймера
            _foodGoalTimer = new Timer();
            _foodGoalTimer.Interval = 2000;
            _foodGoalTimer.AutoReset = false;
            _foodGoalTimer.Elapsed += FoodGoalTimerElapsed;

            _liquidGoalTimer = new Timer();
            _liquidGoalTimer.Interval = 2000;
            _liquidGoalTimer.AutoReset = false;
            _liquidGoalTimer.Elapsed += LiquidGoalTimerElapsed;

            _weightGoalTimer = new Timer();
            _weightGoalTimer.Interval = 2000;
            _weightGoalTimer.AutoReset = false;
            _weightGoalTimer.Elapsed += WeightGoalTimerElapsed;

            _sleepGoalTimer = new Timer();
            _sleepGoalTimer.Interval = 2000;
            _sleepGoalTimer.AutoReset = false;
            _sleepGoalTimer.Elapsed += SleepGoalTimerElapsed;

            // Загрузка целей пользователя при загрузке страницы
            LoadUserFoodGoals();
            LoadUserLiquidGoals();
            LoadUserWeightGoals();
            LoadUserSleepGoals();

            DataContext = this;

        }

        private void LoadUserFoodGoals()
        {
            // Загрузить цели пользователя из базы данных
            _currentUser = DatabaseContext.DbContext.Context.Users.FirstOrDefault(u => u.ID == _currentUser.ID);

            // Отобразить загруженные цели на странице
            if (_currentUser != null)
            {
                FoodGoalTextBox.Text = _currentUser.FoodGoal.ToString();
            }
        }

        private void LoadUserLiquidGoals()
        {
            // Загрузить цели пользователя из базы данных
            _currentUser = DatabaseContext.DbContext.Context.Users.FirstOrDefault(u => u.ID == _currentUser.ID);

            // Отобразить загруженные цели на странице
            if (_currentUser != null)
            {
                LiquidGoalTextBox.Text = _currentUser.LiquidGoal.ToString();
            }
        }

        private void LoadUserWeightGoals()
        {
            // Загрузить цели пользователя из базы данных
            _currentUser = DatabaseContext.DbContext.Context.Users.FirstOrDefault(u => u.ID == _currentUser.ID);

            // Отобразить загруженные цели на странице
            if (_currentUser != null)
            {
                WeightGoalTextBox.Text = _currentUser.WeightGoal.ToString();
            }
        }

        private void LoadUserSleepGoals()
        {
            // Загрузить цели пользователя из базы данных
            _currentUser = DatabaseContext.DbContext.Context.Users.FirstOrDefault(u => u.ID == _currentUser.ID);

            // Отобразить загруженные цели на странице
            if (_currentUser != null)
            {
                SleepGoalTextBox.Text = _currentUser.SleepGoal.ToString();
            }
        }

        private void UpdateUserFoodGoals()
        {
            if (_isGoalChanged)
            {
                if (string.IsNullOrWhiteSpace(FoodGoalTextBox.Text))
                {
                    FoodGoalTextBox.Text = "0"; // Если строка пуста, присваиваем значение 0
                }

                var newFoodGoal = Convert.ToInt32(FoodGoalTextBox.Text);
                if (_currentUser.FoodGoal != newFoodGoal)
                {
                    // Обновить цель пользователя в базе данных
                    var userToUpdate = DatabaseContext.DbContext.Context.Users.FirstOrDefault(u => u.ID == _currentUser.ID);
                    if (userToUpdate != null)
                    {
                        userToUpdate.FoodGoal = newFoodGoal;
                        DatabaseContext.DbContext.Context.SaveChanges();
                        MessageBox.Show("Цель по питанию успешно обновлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось найти пользователя для обновления цели.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                _isGoalChanged = false; // Сбрасываем флаг изменения цели
            }
        }

        private void UpdateUserLiquidGoals()
        {
            if (_isGoalChanged)
            {
                if (string.IsNullOrWhiteSpace(LiquidGoalTextBox.Text))
                {
                    LiquidGoalTextBox.Text = "0"; // Если строка пуста, присваиваем значение 0
                }

                var newLiquidGoal = Convert.ToInt32(LiquidGoalTextBox.Text);
                if (_currentUser.LiquidGoal != newLiquidGoal)
                {
                    // Обновить цель пользователя в базе данных
                    var userToUpdate = DatabaseContext.DbContext.Context.Users.FirstOrDefault(u => u.ID == _currentUser.ID);
                    if (userToUpdate != null)
                    {
                        userToUpdate.LiquidGoal = newLiquidGoal;
                        DatabaseContext.DbContext.Context.SaveChanges();
                        MessageBox.Show("Цель по жидкости успешно обновлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось найти пользователя для обновления цели.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                _isGoalChanged = false; // Сбрасываем флаг изменения цели
            }
        }

        private void UpdateUserWeightGoals()
        {
            if (_isGoalChanged)
            {
                if (string.IsNullOrWhiteSpace(WeightGoalTextBox.Text))
                {
                    WeightGoalTextBox.Text = "0"; // Если строка пуста, присваиваем значение 0
                }

                var newWeightGoal = Convert.ToInt32(WeightGoalTextBox.Text);
                if (_currentUser.WeightGoal != newWeightGoal)
                {
                    // Обновить цель пользователя в базе данных
                    var userToUpdate = DatabaseContext.DbContext.Context.Users.FirstOrDefault(u => u.ID == _currentUser.ID);
                    if (userToUpdate != null)
                    {
                        userToUpdate.WeightGoal = newWeightGoal;
                        DatabaseContext.DbContext.Context.SaveChanges();
                        MessageBox.Show("Цель по весу успешно обновлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось найти пользователя для обновления цели.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                _isGoalChanged = false; // Сбрасываем флаг изменения цели
            }
        }

        private void UpdateUserSleepGoals()
        {
            if (_isGoalChanged)
            {
                if (string.IsNullOrWhiteSpace(SleepGoalTextBox.Text))
                {
                    SleepGoalTextBox.Text = "0"; // Если строка пуста, присваиваем значение 0
                }

                var newSleepGoal = Convert.ToInt32(SleepGoalTextBox.Text);
                if (_currentUser.SleepGoal != newSleepGoal)
                {
                    // Обновить цель пользователя в базе данных
                    var userToUpdate = DatabaseContext.DbContext.Context.Users.FirstOrDefault(u => u.ID == _currentUser.ID);
                    if (userToUpdate != null)
                    {
                        userToUpdate.SleepGoal = newSleepGoal;
                        DatabaseContext.DbContext.Context.SaveChanges();
                        MessageBox.Show("Цель по сну успешно обновлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось найти пользователя для обновления цели.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                _isGoalChanged = false; // Сбрасываем флаг изменения цели
            }
        }

        private void FoodGoalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _isGoalChanged = true;
            // Сбрасываем таймер и запускаем его заново
            _foodGoalTimer.Stop();
            _foodGoalTimer.Start();
        }

        // Обработчик события изменения текста в текстовом поле для цели по жидкости
        private void LiquidGoalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _isGoalChanged = true;
            // Сбрасываем таймер и запускаем его заново
            _liquidGoalTimer.Stop();
            _liquidGoalTimer.Start();
        }

        private void WeightGoalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _isGoalChanged = true;
            // Сбрасываем таймер и запускаем его заново
            _weightGoalTimer.Stop();
            _weightGoalTimer.Start();
        }

        private void SleepGoalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _isGoalChanged = true;
            // Сбрасываем таймер и запускаем его заново
            _sleepGoalTimer.Stop();
            _sleepGoalTimer.Start();
        }

        // Обработчик события таймера
        private void FoodGoalTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => UpdateUserFoodGoals());
        }

        private void LiquidGoalTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => UpdateUserLiquidGoals());
        }

        private void WeightGoalTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => UpdateUserWeightGoals());
        }

        private void SleepGoalTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => UpdateUserSleepGoals());
        }

        // Обработчик клика по кнопке "Выбрать файл"
        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;
                SelectedFiles.Add(fileName);
            }
        }

        // Обработчик клика по кнопке "Отправить почту"
        private void SendEmail_Click(object sender, RoutedEventArgs e)
        {
            string recipient = RecipientTextBox.Text;
            if (string.IsNullOrWhiteSpace(recipient))
            {
                MessageBox.Show("Введите адрес получателя!");
                return;
            }

            if (SelectedFiles.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы один файл!");
                return;
            }

            try
            {
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect("smtp.mail.ru", 587, false);
                    client.Authenticate("dmitry13890@mail.ru", "MCwzVQQXHdeNC5HVS7bf");

                    var message = new MimeMessage();
                    message.From.Add(MailboxAddress.Parse("dmitry13890@mail.ru"));

                    message.To.Add(MailboxAddress.Parse(recipient));

                    message.Subject = "Отправка документа от FitLog";

                    var builder = new BodyBuilder();
                    builder.TextBody = "Результаты отслеживания";

                    foreach (string file in SelectedFiles)
                    {
                        builder.Attachments.Add(file);
                    }

                    message.Body = builder.ToMessageBody();

                    client.Send(message);
                    client.Disconnect(true);
                }

                MessageBox.Show("Почта успешно отправлена!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке почты: {ex.Message}");
            }
        }

    }

}

