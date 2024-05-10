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

    public class Water
    {
        public string Day { get; set; }
        public double Liquid { get; set; }
    }

    public partial class LiquidPage : Page
    {
        private Users _currentUser;
        public List<string> DayOfWeeks { get; set; }
        public List<string> Period { get; set; }
        private DateTime _currentWaterChartDate = DateTime.Now;

        public LiquidPage(Users user)
        {
            _currentUser = user;
            InitializeComponent();
            Period = period;
            PeriodComboBox.SelectedIndex = 0;
            ChartUpdateWater(DateTime.Now);
            DataContext = this;
        }
        private List<string> period = new List<string>
        {
            "Сегодня", "Неделя"
        };
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

        private void ChartUpdateWater(DateTime dateTime)
        {
            _currentWaterChartDate = dateTime;
            StartDateWaterTextBlock.Text = GetFirstDateOfWeek(dateTime, DayOfWeek.Monday).ToString("dd.MM.yyyy");
            FinishDateWaterTextBlock.Text = GetFirstDateOfWeek(dateTime.AddDays(7), DayOfWeek.Monday).
                AddDays(-1).ToString("dd.MM.yyyy");
            var info = DatabaseContext.DbContext.Context.Liquids.ToList().
                Where(x => x.Users == _currentUser && x.DrinkingTime > GetFirstDateOfWeek(dateTime, DayOfWeek.Monday) &&
                x.DrinkingTime < GetFirstDateOfWeek(dateTime.AddDays(7), DayOfWeek.Monday));

            List<Water> waters = new List<Water>();
            foreach (var item in dayOfWeeks)
            {
                var water = info.Where(x => x.DrinkingTime.Value.DayOfWeek == item.Key);
                waters.Add(new Water { Day = item.Value, Liquid = water.Count() == 0 ? 0 : (double)water.Sum(x => x.LiquidLevel) });
            }
            ColGraficWater.ItemsSource = waters;
        }



        public void SaveWater()
        {
            decimal liquidConsumed = Convert.ToInt32(WaterLevelTextBox.Text);
            DateTime today = DateTime.Today;

            if (string.IsNullOrWhiteSpace(WaterLevelTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите количество выпитой жидкости");
                return;
            }

            if (Convert.ToDecimal(WaterLevelTextBox.Text) > 9999)
            {
                MessageBox.Show("Ограничение в 9999 миллилитров");
                return;
            }


            DatabaseContext.DbContext.Context.Liquids.Add(new Liquids
            {
                UserID = _currentUser.ID,
                LiquidLevel = Convert.ToDecimal(WaterLevelTextBox.Text),
                LiquidType = WaterTypeTextBox.Text,
                DrinkingTime = LiquidTimeDateTimePicker.Value
            });

            DatabaseContext.DbContext.Context.SaveChanges();

            var liquidsForToday = DatabaseContext.DbContext.Context.Liquids
           .Where(m => m.UserID == _currentUser.ID &&
                DbFunctions.TruncateTime(m.DrinkingTime) == today)
           .ToList();


            // Посчитать сумму калорий
            decimal totalLiquidsForToday = liquidsForToday.Sum(m => m.LiquidLevel);


            // Сравнение с целью пользователя
            if (totalLiquidsForToday == _currentUser.LiquidGoal)
            {
                ShowNotification("Жидкость", "Вы достигли свою цель по жидкости за сегодня!");
            }

            if (totalLiquidsForToday > _currentUser.LiquidGoal)
            {
                ShowNotification("Жидкость", "Вы превысили свою цель по жидкости за сегодня!");
            }

            //ClearField.ClearTextBoxes(this);
        }

        private void ButtonSaveWater_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                SaveWater();
                ChartUpdateWater(DateTime.Now);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}");
            }
        }

        private void ButtonLeftWater_Click(object sender, RoutedEventArgs e)
        {
            _currentWaterChartDate = _currentWaterChartDate.AddDays(-7);
            ChartUpdateWater(_currentWaterChartDate);
        }

        private void ButtonRightWater_Click(object sender, RoutedEventArgs e)
        {
            _currentWaterChartDate = _currentWaterChartDate.AddDays(7);
            ChartUpdateWater(_currentWaterChartDate);
        }

        private void ButtonThisWeekWater_Click(object sender, RoutedEventArgs e)
        {
            _currentWaterChartDate = DateTime.Now;
            ChartUpdateWater(_currentWaterChartDate);
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

        public static void ExportToExcelWater(List<Liquids> liquids)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            string fileName = $"LiquidOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx"; // Добавляем временный штамп к имени файла
            string filePath = System.IO.Path.Combine(downloadsPath, fileName);

            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("LiquidData");

                // Добавляем заголовки
                worksheet.Cells[1, 1].Value = "Момент измерения";
                worksheet.Cells[1, 2].Value = "Тип жидкости";
                worksheet.Cells[1, 3].Value = "Количество миллилитров";

                if (liquids.Any()) // Проверяем, есть ли данные
                {
                    // Добавляем данные
                    int row = 2;
                    foreach (var liquid in liquids)
                    {
                        worksheet.Cells[row, 1].Value = liquid.DrinkingTime;
                        worksheet.Cells[row, 2].Value = liquid.LiquidType;
                        worksheet.Cells[row, 3].Value = liquid.LiquidLevel;

                        row++;
                    }

                    // Устанавливаем формат для времени
                    worksheet.Column(1).Style.Numberformat.Format = "hh:mm:ss dd-mm-yyyy";
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

        private void ButtonExportToExcelWater_Click(object sender, RoutedEventArgs e)
        {
            if (PeriodComboBox.SelectedValue.ToString() == "Сегодня")
            {
                var currentDate = DateTime.Now.Date;
                var info = DatabaseContext.DbContext.Context.Liquids.ToList().
                    Where(x => x.Users == _currentUser && x.DrinkingTime > currentDate);
                List<Liquids> liquids = new List<Liquids>();
                foreach (var perem in info)
                {
                    liquids.Add(new Liquids
                    {
                        DrinkingTime = perem.DrinkingTime.Value,
                        LiquidType = perem.LiquidType,
                        LiquidLevel = perem.LiquidLevel

                    });
                }

                ExportToExcelWater(liquids);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                // Начальная дата - 6 дней назад от текущего дня
                var startDate = DateTime.Now.Date.AddDays(-6);

                // Конечная дата - текущий момент
                var endDate = DateTime.Now;

                var info = DatabaseContext.DbContext.Context.Liquids.ToList()
                    .Where(x => x.Users == _currentUser && x.DrinkingTime >= startDate && x.DrinkingTime <= endDate);

                List<Liquids> liquids = new List<Liquids>();
                foreach (var perem in info)
                {
                    liquids.Add(new Liquids
                    {
                        DrinkingTime = perem.DrinkingTime.Value,
                        LiquidType = perem.LiquidType,
                        LiquidLevel = perem.LiquidLevel

                    });
                }

                ExportToExcelWater(liquids);
            }

        }

        public static void ExportToWordMeal(List<Liquids> liquids)
        {
            // Создаем новый документ Word
            using (WordDocument document = new WordDocument())
            {
                // Добавляем раздел с заголовком
                WSection section = document.AddSection() as WSection;
                WParagraph paragraph = section.HeadersFooters.Header.AddParagraph() as WParagraph;
                paragraph.AppendText("Liquid Data").CharacterFormat.FontSize = 14;
                paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;

                // Добавляем таблицу
                WTable table = section.AddTable() as WTable;
                table.ResetCells(liquids.Count + 1, 3); // +1 для заголовков

                // Добавляем заголовки таблицы
                string[] headers = { "Момент измерения", "Тип жидкости", "Количество миллилитров" };
                for (int i = 0; i < headers.Length; i++)
                {
                    table[0, i].AddParagraph().AppendText(headers[i]);
                    table[0, i].CellFormat.VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment.Middle;
                    //table[0, i].CellFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;
                }

                // Добавляем данные в таблицу
                for (int i = 0; i < liquids.Count; i++)
                {
                    Liquids liquid = liquids[i];
                    table[i + 1, 0].AddParagraph().AppendText(liquid.DrinkingTime.ToString());
                    table[i + 1, 1].AddParagraph().AppendText(liquid.LiquidType);
                    table[i + 1, 2].AddParagraph().AppendText(liquid.LiquidLevel.ToString());
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
                string fileName = $"MealOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.docx";
                string filePath = System.IO.Path.Combine(downloadsPath, fileName);
                document.Save(filePath);

                MessageBox.Show($"Файл сохранен в папке \"Загрузки\": {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ButtonExportToWordWater_Click(object sender, RoutedEventArgs e)
        {
            if (PeriodComboBox.SelectedValue.ToString() == "Сегодня")
            {
                var currentDate = DateTime.Now.Date;
                var info = DatabaseContext.DbContext.Context.Liquids.ToList().
                    Where(x => x.Users == _currentUser && x.DrinkingTime > currentDate);
                List<Liquids> liquids = new List<Liquids>();
                foreach (var perem in info)
                {
                    liquids.Add(new Liquids
                    {
                        DrinkingTime = perem.DrinkingTime.Value,
                        LiquidType = perem.LiquidType,
                        LiquidLevel = perem.LiquidLevel

                    });
                }

                ExportToWordMeal(liquids);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                // Начальная дата - 6 дней назад от текущего дня
                var startDate = DateTime.Now.Date.AddDays(-6);

                // Конечная дата - текущий момент
                var endDate = DateTime.Now;

                var info = DatabaseContext.DbContext.Context.Liquids.ToList()
                    .Where(x => x.Users == _currentUser && x.DrinkingTime >= startDate && x.DrinkingTime <= endDate);

                List<Liquids> liquids = new List<Liquids>();
                foreach (var perem in info)
                {
                    liquids.Add(new Liquids
                    {
                        DrinkingTime = perem.DrinkingTime.Value,
                        LiquidType = perem.LiquidType,
                        LiquidLevel = perem.LiquidLevel

                    });
                }

                ExportToWordMeal(liquids);
            }

        }
    }
}

