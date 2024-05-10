using FitLog.Entities;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static FitLog.DateClass.DateInfo;
using static FitLog.Notifications.MyNotify;

namespace FitLog.Pages
{
    public class Sleeping
    {
        public string Day { get; set; }
        public int SleepTime { get; set; }
    }

    public partial class SleepPage : Page
    {
        private Users _currentUser;
        public List<string> Period { get; set; }
        public List<string> DayOfWeeks { get; set; }

        private DateTime _currentSleepChartDate = DateTime.Now;

        public SleepPage(Users user)
        {
            InitializeComponent();
            _currentUser = user;
            BedTimePicker.DefaultValue = DateTime.Now;
            WakeUpTimePicker.DefaultValue = DateTime.Now;
            Period = period;
            PeriodComboBox.SelectedIndex = 0;
            ChartUpdateSleep(DateTime.Now);
            DataContext = this;
        }

        private List<string> period = new List<string>
        {
            "Сегодня", "Неделя"
        };

        private void ButtonSaveSleep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime selectedDate = BedTimePicker.Value?.Date ?? DateTime.Now.Date;
                SaveSleep(selectedDate);
                ChartUpdateSleep(selectedDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}");
            }
        }

        private int GetDaySleepTime(DateTime dateTime)
        {
            var info = DatabaseContext.DbContext.Context.Sleeps.ToList().
                Where(x => x.Users == _currentUser && x.SleepTime.Value.Date == dateTime.Date);

            var sleeping = new TimeSpan(0);
            foreach (var elem in info)
            {
                sleeping += elem.WakeUpTime - elem.BedTime;
            }
            return (int)sleeping.TotalHours;
        }

        public void SaveSleep(DateTime selectedDate)
        {
            DateTime today = DateTime.Today;
            var wakeUp = WakeUpTimePicker.Value;
            var bedTime = BedTimePicker.Value;

            // Проверка на пустые значения времени
            // Проверка на пустые значения времени
            if (wakeUp.ToString().Length == 0 || bedTime.ToString().Length == 0)
            {
                MessageBox.Show("Пожалуйста, введите время");
                return;
            }



            // Проверка корректности времени ложания спать и времени пробуждения
            if (bedTime >= wakeUp)
            {
                MessageBox.Show("Пожалуйста, введите корректное значение времени");
                return;
            }

            DateTime bedTimeWithDate = selectedDate.Date.Add(bedTime.Value.TimeOfDay);
            DateTime wakeUpWithDate = selectedDate.Date.Add(wakeUp.Value.TimeOfDay);

            // Расчет времени сна
            TimeSpan sleepDuration = wakeUpWithDate - bedTimeWithDate;
            int hoursOfSleep = (int)sleepDuration.TotalHours;

            // Проверка на количество часов сна больше 24
            if (hoursOfSleep > 24)
            {
                MessageBox.Show("Вы не можете спать больше 24 часов в день");
                return;
            }

            // Проверка на внос данных на будущие даты
            // Проверка на внос данных на будущие даты
            if (selectedDate > DateTime.Today || bedTimeWithDate > DateTime.Now || wakeUpWithDate > DateTime.Now
                || bedTimeWithDate.Date != selectedDate.Date || wakeUpWithDate.Date != selectedDate.Date)
            {
                MessageBox.Show("Вы не можете лечь спать или проснуться в будущем");
                return;
            }


            // Добавление данных о сне в базу данных
            DatabaseContext.DbContext.Context.Sleeps.Add(new Sleeps
            {
                UserID = _currentUser.ID,
                SleepTime = selectedDate.Date,
                BedTime = bedTimeWithDate,
                WakeUpTime = wakeUpWithDate,
                HoursOfSleep = hoursOfSleep
            });

            DatabaseContext.DbContext.Context.SaveChanges();

            BedTimePicker.Text = "";
            WakeUpTimePicker.Text = "";

            // Подсчет суммарного количества часов сна за сегодня
            var totalSleepsForToday = DatabaseContext.DbContext.Context.Sleeps
                .Where(m => m.UserID == _currentUser.ID && DbFunctions.TruncateTime(m.SleepTime) == today)
                .Sum(m => m.HoursOfSleep);

            // Сравнение с целью пользователя и вывод уведомлений
            if (totalSleepsForToday == _currentUser.SleepGoal)
            {
                ShowNotification("Сон", "Вы достигли своей цели по сну за сегодня!");
            }
            else if (totalSleepsForToday > _currentUser.SleepGoal)
            {
                ShowNotification("Сон", "Вы превысили свою цель по сну за сегодня!");
            }
        }




        private Dictionary<DayOfWeek, string> dayOfWeeks = new Dictionary<DayOfWeek, string>
        {
            {DayOfWeek.Monday, "Пн" },
            {DayOfWeek.Tuesday, "Вт" },
            {DayOfWeek.Wednesday, "Ср" },
            {DayOfWeek.Thursday, "Чт" },
            {DayOfWeek.Friday, "Пт" },
            {DayOfWeek.Saturday, "Сб" },
            {DayOfWeek.Sunday, "Вс" }

        };

        private void ChartUpdateSleep(DateTime dateTime)
        {
            _currentSleepChartDate = dateTime;
            StartDateSleepTextBlock.Text = GetFirstDateOfWeek(dateTime, DayOfWeek.Monday).ToString("dd.MM.yyyy");
            FinishDateSleepTextBlock.Text = GetFirstDateOfWeek(dateTime.AddDays(7), DayOfWeek.Monday).
                AddDays(-1).ToString("dd.MM.yyyy");
            var info = DatabaseContext.DbContext.Context.Sleeps.ToList().
                Where(x => x.Users == _currentUser && x.SleepTime > GetFirstDateOfWeek(dateTime, DayOfWeek.Monday) &&
                x.SleepTime < GetFirstDateOfWeek(dateTime.AddDays(7), DayOfWeek.Monday));

            List<Sleeping> sleepings = new List<Sleeping>();
            foreach (var item in dayOfWeeks)
            {
                var sleep = info.Where(x => x.SleepTime.Value.DayOfWeek == item.Key);


                sleepings.Add(new Sleeping { Day = item.Value, SleepTime = sleep.Count() == 0 ? 0 : GetDaySleepTime(sleep.First().SleepTime.Value) });
            }


            ColGraficSleeps.ItemsSource = sleepings;
        }

        private void ButtonLeftSleep_Click(object sender, RoutedEventArgs e)
        {
            _currentSleepChartDate = _currentSleepChartDate.AddDays(-7);
            ChartUpdateSleep(_currentSleepChartDate);
        }

        private void ButtonRightSleep_Click(object sender, RoutedEventArgs e)
        {
            _currentSleepChartDate = _currentSleepChartDate.AddDays(7);
            ChartUpdateSleep(_currentSleepChartDate);
        }

        private void ButtonThisWeekSleep_Click(object sender, RoutedEventArgs e)
        {
            _currentSleepChartDate = DateTime.Now;
            ChartUpdateSleep(_currentSleepChartDate);
        }

        //private void OpenBasicsHealthyDiet(object sender, RoutedEventArgs e)
        //{
        //    string url = "https://14.rospotrebnadzor.ru/content/2090/79455/";

        //    Process.Start(url);
        //}

        //private void OpenHealthySnacks(object sender, RoutedEventArgs e)
        //{
        //    string url = "https://cgie.62.rospotrebnadzor.ru/content/1408/95790/";

        //    Process.Start(url);
        //}

        //private void OpenRecipes(object sender, RoutedEventArgs e)
        //{
        //    string url = "https://yummybook.ru/category/zdorovoe-pitanie";

        //    Process.Start(url);
        //}

        //var info = DatabaseContext.DbContext.Context.Sleeps.ToList().
        //              Where(x => x.Users == _currentUser && x.BedTime > GetFirstDateOfWeek(dateTime, DayOfWeek.Monday) &&
        //              x.WakeUpTime < GetFirstDateOfWeek(dateTime.AddDays(7), DayOfWeek.Monday));

        public static void ExportToExcelSleep(List<Sleeps> sleeps)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            string fileName = $"SleepOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx"; // Добавляем временный штамп к имени файла
            string filePath = System.IO.Path.Combine(downloadsPath, fileName);

            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("SleepData");

                // Добавляем заголовки
                worksheet.Cells[1, 1].Value = "Время отхода ко сну";
                worksheet.Cells[1, 2].Value = "Время пробуждения";
                worksheet.Cells[1, 3].Value = "Количество часов";

                if (sleeps.Any()) // Проверяем, есть ли данные
                {
                    // Добавляем данные
                    int row = 2;
                    foreach (var sleep in sleeps)
                    {
                        worksheet.Cells[row, 1].Value = sleep.BedTime;
                        worksheet.Cells[row, 2].Value = sleep.WakeUpTime;
                        worksheet.Cells[row, 3].Value = sleep.HoursOfSleep;

                        row++;
                    }

                    // Устанавливаем формат для времени
                    worksheet.Column(1).Style.Numberformat.Format = "hh:mm:ss dd-mm-yyyy";
                    worksheet.Cells[1, 1, 1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[1, 1, 1, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    worksheet.Column(2).Style.Numberformat.Format = "hh:mm:ss dd-mm-yyyy";
                    worksheet.Cells[1, 1, 1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[1, 1, 1, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    // Устанавливаем выравнивание для данных
                    worksheet.Cells[2, 1, row - 1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[2, 1, row - 1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells[2, 3, row - 1, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[2, 3, row - 1, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                }

                // Автоподбор ширины столбцов
                worksheet.Cells.AutoFitColumns();

                // Сохраняем изменения
                package.Save();
            }
            MessageBox.Show($"Файл сохранен в папке \"Загрузки\": {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ButtonExportToExcelSleep_Click(object sender, RoutedEventArgs e)
        {
            if (PeriodComboBox.SelectedValue.ToString() == "Сегодня")
            {
                var currentDate = DateTime.Now.Date;
                var info = DatabaseContext.DbContext.Context.Sleeps.ToList().
                    Where(x => x.Users == _currentUser && x.SleepTime > currentDate);
                List<Sleeps> sleeps = new List<Sleeps>();
                foreach (var perem in info)
                {
                    sleeps.Add(new Sleeps
                    {
                        BedTime = perem.BedTime,
                        WakeUpTime = perem.WakeUpTime,
                        HoursOfSleep = perem.HoursOfSleep,

                    });
                }

                ExportToExcelSleep(sleeps);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                // Начальная дата - 6 дней назад от текущего дня
                var startDate = DateTime.Now.Date.AddDays(-6);

                // Конечная дата - текущий момент
                var endDate = DateTime.Now;

                var info = DatabaseContext.DbContext.Context.Sleeps.ToList()
                    .Where(x => x.Users == _currentUser && x.SleepTime >= startDate && x.SleepTime <= endDate);

                List<Sleeps> sleeps = new List<Sleeps>();
                foreach (var perem in info)
                {
                    sleeps.Add(new Sleeps
                    {
                        BedTime = perem.BedTime,
                        WakeUpTime = perem.WakeUpTime,
                        HoursOfSleep = perem.HoursOfSleep,

                    });
                }

                ExportToExcelSleep(sleeps);
            }

        }

        public static void ExportToWordSleep(List<Sleeps> sleeps)
        {
            // Создаем новый документ Word
            using (WordDocument document = new WordDocument())
            {
                // Добавляем раздел с заголовком
                WSection section = document.AddSection() as WSection;
                WParagraph paragraph = section.HeadersFooters.Header.AddParagraph() as WParagraph;
                paragraph.AppendText("Sleep Data").CharacterFormat.FontSize = 14;
                paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;

                // Добавляем таблицу
                WTable table = section.AddTable() as WTable;
                table.ResetCells(sleeps.Count + 1, 3); // +1 для заголовков

                // Добавляем заголовки таблицы
                string[] headers = { "Время отхода ко сну", "Время пробуждения", "Количество часов" };
                for (int i = 0; i < headers.Length; i++)
                {
                    table[0, i].AddParagraph().AppendText(headers[i]);
                    table[0, i].CellFormat.VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment.Middle;
                    //table[0, i].CellFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;
                }

                // Добавляем данные в таблицу
                for (int i = 0; i < sleeps.Count; i++)
                {
                    Sleeps sleep = sleeps[i];
                    table[i + 1, 0].AddParagraph().AppendText(sleep.BedTime.ToString());
                    table[i + 1, 1].AddParagraph().AppendText(sleep.WakeUpTime.ToString());
                    table[i + 1, 2].AddParagraph().AppendText(sleep.HoursOfSleep.ToString());
                }

                // Устанавливаем форматирование для таблицы
                foreach (WTableRow row in table.Rows)
                {
                    foreach (WTableCell cell in row.Cells)
                    {
                        foreach (WParagraph paragraphCell in cell.Paragraphs)
                        {
                            paragraphCell.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;
                        }
                    }
                }

                // Сохраняем документ
                string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
                string fileName = $"SleepOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.docx";
                string filePath = System.IO.Path.Combine(downloadsPath, fileName);
                document.Save(filePath);

                MessageBox.Show($"Файл сохранен в папке \"Загрузки\": {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ButtonExportToWordSleep_Click(object sender, RoutedEventArgs e)
        {
            if (PeriodComboBox.SelectedValue.ToString() == "Сегодня")
            {
                var currentDate = DateTime.Now.Date;
                var info = DatabaseContext.DbContext.Context.Sleeps.ToList().
                    Where(x => x.Users == _currentUser && x.SleepTime > currentDate);
                List<Sleeps> sleeps = new List<Sleeps>();
                foreach (var perem in info)
                {
                    sleeps.Add(new Sleeps
                    {
                        BedTime = perem.BedTime,
                        WakeUpTime = perem.WakeUpTime,
                        HoursOfSleep = perem.HoursOfSleep,

                    });
                }

                ExportToWordSleep(sleeps);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                // Начальная дата - 6 дней назад от текущего дня
                var startDate = DateTime.Now.Date.AddDays(-6);

                // Конечная дата - текущий момент
                var endDate = DateTime.Now;

                var info = DatabaseContext.DbContext.Context.Sleeps.ToList()
                    .Where(x => x.Users == _currentUser && x.SleepTime >= startDate && x.SleepTime <= endDate);

                List<Sleeps> sleeps = new List<Sleeps>();
                foreach (var perem in info)
                {
                    sleeps.Add(new Sleeps
                    {
                        BedTime = perem.BedTime,
                        WakeUpTime = perem.WakeUpTime,
                        HoursOfSleep = perem.HoursOfSleep,

                    });
                }

                ExportToWordSleep(sleeps);
            }

        }

    }
}
