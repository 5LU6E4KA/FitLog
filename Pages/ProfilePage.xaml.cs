using FitLog.Controls;
using FitLog.Entities;
using MimeKit;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static FitLog.Controls.CustomMessageBox;

namespace FitLog.Pages
{

    public partial class ProfilePage : Page
    {
        private Users _currentUser;
        private Timer _foodGoalTimer;
        private Timer _liquidGoalTimer;
        private Timer _weightGoalTimer;
        private Timer _frequencyGoalTimer;
        private bool _isGoalChanged = false;

        public ObservableCollection<string> SelectedFiles { get; set; } = new ObservableCollection<string>();

        public const string email = "fitlog05@mail.ru";
        public const string password = "i3twPDH2aUTkuf7kkP30";


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

            _frequencyGoalTimer = new Timer();
            _frequencyGoalTimer.Interval = 2000;
            _frequencyGoalTimer.AutoReset = false;
            _frequencyGoalTimer.Elapsed += FrequencyGoalTimerElapsed;

            // Загрузка целей пользователя при загрузке страницы
            LoadUserFoodGoals();
            LoadUserLiquidGoals();
            LoadUserWeightGoals();
            LoadUserFrequencyGoals();

            FoodGoalTextBox.AddHandler(DataObject.PastingEvent, new DataObjectPastingEventHandler(OnPasting));
            FrequencyGoalTextBox.AddHandler(DataObject.PastingEvent, new DataObjectPastingEventHandler(OnPasting));
            LiquidGoalTextBox.AddHandler(DataObject.PastingEvent, new DataObjectPastingEventHandler(OnPasting));
            WeightGoalTextBox.AddHandler(DataObject.PastingEvent, new DataObjectPastingEventHandler(OnPasting));

            DataContext = this;

        }

        private void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            // Отменяем вставку любых данных
            e.CancelCommand();
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
            
            _currentUser = DatabaseContext.DbContext.Context.Users.FirstOrDefault(u => u.ID == _currentUser.ID);

            if (_currentUser != null)
            {
                LiquidGoalTextBox.Text = _currentUser.LiquidGoal.ToString();
            }
        }

        private void LoadUserWeightGoals()
        {
            
            _currentUser = DatabaseContext.DbContext.Context.Users.FirstOrDefault(u => u.ID == _currentUser.ID);

            if (_currentUser != null)
            {
                WeightGoalTextBox.Text = _currentUser.WeightGoal.ToString();
            }
        }

        private void LoadUserFrequencyGoals()
        {
            _currentUser = DatabaseContext.DbContext.Context.Users.FirstOrDefault(u => u.ID == _currentUser.ID);

            if (_currentUser != null)
            {
                FrequencyGoalTextBox.Text = _currentUser.FrequencyGoal.ToString();
            }
        }

        private void UpdateUserFoodGoals()
        {
            if (_isGoalChanged)
            {
                if (string.IsNullOrWhiteSpace(FoodGoalTextBox.Text))
                {
                    FoodGoalTextBox.Text = "0";
                }

                if (int.TryParse(FoodGoalTextBox.Text, out int foodGoalValue))
                {
                    if (foodGoalValue > 9999 || foodGoalValue < 0)
                    {
                        CustomMessageBox.Show("Ограничение от 0 до 9999 килокалорий", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                        return;
                    }

                    var newFoodGoal = (int)foodGoalValue; 
                    if (_currentUser.FoodGoal != newFoodGoal)
                    {
                        var userToUpdate = DatabaseContext.DbContext.Context.Users.FirstOrDefault(u => u.ID == _currentUser.ID);
                        if (userToUpdate != null)
                        {
                            userToUpdate.FoodGoal = newFoodGoal;
                            DatabaseContext.DbContext.Context.SaveChanges();
                            CustomMessageBox.Show("Цель по питанию успешно обновлена", "Успех", MessageWindowImage.Information, MessageWindowButton.Ok);
                        }
                        else
                        {
                            CustomMessageBox.Show("Не удалось найти пользователя для обновления цели", "Внимание", MessageWindowImage.Error, MessageWindowButton.Ok);
                        }
                    }
                }
                else
                {
                    CustomMessageBox.Show("Пожалуйста, введите корректное числовое значение по питанию", "Ошибка", MessageWindowImage.Error, MessageWindowButton.Ok);
                }

                _isGoalChanged = false; // Сбрасываем флаг изменения цели
            }

        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Проверка на цифру
            Regex regex = new Regex("^[0-9]+$");
            
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void NumericAndCommaTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string currentText = textBox.Text;
            string newText = currentText.Insert(textBox.CaretIndex, e.Text);

            // Разрешить только цифры и запятую
            if (!char.IsDigit(e.Text, 0) && e.Text != ",")
            {
                e.Handled = true;
                return;
            }

            // Запрет на ввод запятой первым символом
            if (newText.StartsWith(","))
            {
                e.Handled = true;
                return;
            }

            // Запрет на ввод запятой последним символом
            if (newText.EndsWith(",") && newText.Count(c => c == ',') > 1)
            {
                e.Handled = true;
                return;
            }

            // Запрет на ввод второй запятой
            if (newText.Count(c => c == ',') > 1)
            {
                e.Handled = true;
                return;
            }
        }

        private void UpdateUserLiquidGoals()
        {
            if (_isGoalChanged)
            {
                if (string.IsNullOrWhiteSpace(LiquidGoalTextBox.Text))
                {
                    LiquidGoalTextBox.Text = "0";
                }

                if (int.TryParse(LiquidGoalTextBox.Text, out int liquidGoalValue))
                {
                    if (liquidGoalValue > 9999 || liquidGoalValue < 0)
                    {
                        CustomMessageBox.Show("Ограничение в 9999 миллилитров", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                        return;
                    }

                    var newLiquidGoal = (int)liquidGoalValue; 
                    if (_currentUser.LiquidGoal != newLiquidGoal)
                    {
                        var userToUpdate = DatabaseContext.DbContext.Context.Users.FirstOrDefault(u => u.ID == _currentUser.ID);
                        if (userToUpdate != null)
                        {
                            userToUpdate.LiquidGoal = newLiquidGoal;
                            DatabaseContext.DbContext.Context.SaveChanges();
                            CustomMessageBox.Show("Цель по жидкости успешно обновлена", "Успех", MessageWindowImage.Information, MessageWindowButton.Ok);
                        }
                        else
                        {
                            CustomMessageBox.Show("Не удалось найти пользователя для обновления цели", "Внимание", MessageWindowImage.Error, MessageWindowButton.Ok);
                        }
                    }
                }
                else
                {
                    CustomMessageBox.Show("Пожалуйста, введите корректное числовое значение по жидкости", "Ошибка", MessageWindowImage.Error, MessageWindowButton.Ok);
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
                    WeightGoalTextBox.Text = "0";
                }

                if (decimal.TryParse(WeightGoalTextBox.Text, out decimal weightGoalValue))
                {
                    if (weightGoalValue > 650 || weightGoalValue < 10)
                    {
                        CustomMessageBox.Show("Человек не может иметь такой вес", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                        return;
                    }

                    var newWeightGoal = weightGoalValue;
                    if (_currentUser.WeightGoal != newWeightGoal)
                    {
                        var userToUpdate = DatabaseContext.DbContext.Context.Users.FirstOrDefault(u => u.ID == _currentUser.ID);
                        if (userToUpdate != null)
                        {
                            userToUpdate.WeightGoal = newWeightGoal;
                            DatabaseContext.DbContext.Context.SaveChanges();
                            CustomMessageBox.Show("Цель по весу успешно обновлена", "Успех", MessageWindowImage.Information, MessageWindowButton.Ok);
                        }
                        else
                        {
                            CustomMessageBox.Show("Не удалось найти пользователя для обновления цели", "Внимание", MessageWindowImage.Error, MessageWindowButton.Ok);
                        }
                    }
                }
                else
                {
                    CustomMessageBox.Show("Пожалуйста, введите корректное числовое значение по весу", "Ошибка", MessageWindowImage.Error, MessageWindowButton.Ok);
                }

                _isGoalChanged = false; // Сбрасываем флаг изменения цели
            }

        }

        private void UpdateUserFrequencyGoals()
        {
            if (_isGoalChanged)
            {
                if (string.IsNullOrWhiteSpace(FrequencyGoalTextBox.Text))
                {
                    FrequencyGoalTextBox.Text = "0";
                }

                if (int.TryParse(FrequencyGoalTextBox.Text, out int frequencyGoalValue))
                {
                    if (frequencyGoalValue > 60 || frequencyGoalValue < 5)
                    {
                        CustomMessageBox.Show("Человек не может иметь такой показатель частоты дыхания", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                        return;
                    }

                    var newFrequencyGoal = frequencyGoalValue; 
                    if (_currentUser.FrequencyGoal != newFrequencyGoal)
                    {
                        var userToUpdate = DatabaseContext.DbContext.Context.Users.FirstOrDefault(u => u.ID == _currentUser.ID);
                        if (userToUpdate != null)
                        {
                            userToUpdate.FrequencyGoal = newFrequencyGoal;
                            DatabaseContext.DbContext.Context.SaveChanges();
                            CustomMessageBox.Show("Цель по дыханию успешно обновлена", "Успех", MessageWindowImage.Information, MessageWindowButton.Ok);
                        }
                        else
                        {
                            CustomMessageBox.Show("Не удалось найти пользователя для обновления цели", "Внимание", MessageWindowImage.Error, MessageWindowButton.Ok);
                        }
                    }
                }
                else
                {
                    CustomMessageBox.Show("Пожалуйста, введите корректное числовое значение по частоте дыхательных движений", "Ошибка", MessageWindowImage.Error, MessageWindowButton.Ok);
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
            _liquidGoalTimer.Stop();
            _liquidGoalTimer.Start();
        }

        private void WeightGoalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _isGoalChanged = true;
            _weightGoalTimer.Stop();
            _weightGoalTimer.Start();
        }

        private void FrequencyGoalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _isGoalChanged = true;
            _frequencyGoalTimer.Stop();
            _frequencyGoalTimer.Start();
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

        private void FrequencyGoalTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => UpdateUserFrequencyGoals());
        }


        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                AddFile(openFileDialog.FileName);
            }
        }

        private const int MaxFileSize = 7 * 1024 * 1024;
        private readonly string[] AllowedExtensions = { ".xls", ".xlsx", ".doc", ".docx", ".pdf" };

        private void AddFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > MaxFileSize)
            {
                CustomMessageBox.Show($"Файл {fileInfo.Name} превышает максимальный размер 7 МБ!", "Ошибка", MessageWindowImage.Error, MessageWindowButton.Ok);
                return;
            }

            string fileExtension = fileInfo.Extension.ToLower();
            if (!AllowedExtensions.Contains(fileExtension))
            {
                CustomMessageBox.Show($"Файл {fileInfo.Name} имеет недопустимое расширение! Разрешены только файлы с расширением: {string.Join(", ", AllowedExtensions)}", "Ошибка", MessageWindowImage.Error, MessageWindowButton.Ok);
                return;
            }

            SelectedFiles.Add(filePath);
        }

        private void SelectedFilesListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var listBox = sender as ListBox;
                if (listBox != null && listBox.SelectedItem != null)
                {
                    var selectedFile = listBox.SelectedItem as string;
                    if (selectedFile != null)
                    {
                        SelectedFiles.Remove(selectedFile);
                    }
                }
            }
        }

        private void SendEmail_Click(object sender, RoutedEventArgs e)
        {
            string recipient = RecipientTextBox.Text;
            if (string.IsNullOrWhiteSpace(recipient))
            {
                CustomMessageBox.Show("Введите адрес получателя!", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(recipient, emailPattern))
            {
                CustomMessageBox.Show("Введите корректный адрес электронной почты!", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (SelectedFiles.Count == 0)
            {
                CustomMessageBox.Show("Выберите хотя бы один файл!", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (SelectedFiles.Count > 3)
            {
                CustomMessageBox.Show("Максимальное количество файлов - 3!", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            try
            {
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect("smtp.mail.ru", 587, false);
                    client.Authenticate(email, password);

                    var message = new MimeMessage();
                    message.From.Add(MailboxAddress.Parse(email));
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
                CustomMessageBox.Show("Сообщение успешно отправлено!", "Успех", MessageWindowImage.Information, MessageWindowButton.Ok);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при отправке почты: {ex.Message}", "Ошибка", MessageWindowImage.Error, MessageWindowButton.Ok);
            }
        }



        private void OpenGithub(object sender, RoutedEventArgs e)
        {
            string url = "https://github.com/5LU6E4KA/FitLog";

            Process.Start(url);
        }

        private void OpenTelegram(object sender, RoutedEventArgs e)
        {
            string url = "https://t.me/Dmitry13890";

            var psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };

            Process.Start(psi);
        }

        private void OpenGmail(object sender, RoutedEventArgs e)
        {
            string email = "vitalevd.2004@gmail.com";
            string gmailUrl = $"https://mail.google.com/mail/?view=cm&fs=1&to={email}";

            Process.Start(new ProcessStartInfo(gmailUrl) { UseShellExecute = true });

        }

    }

}

