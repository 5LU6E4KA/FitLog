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
using static FitLog.Notifications.MyNotify;

namespace FitLog.Pages
{

    public class WeightsTwo
    {
        public DateTime WeightTime { get; set; }
        public double Weight { get; set; }
    }

    public partial class WeightPage : Page
    {
        private Users _currentUser;
        private DateTime _currentPulseChartDate = DateTime.Now;
        public List<string> Period { get; set; }

        private List<string> period = new List<string>
        {
            "Сегодня", "Неделя"
        };
        public WeightPage(Users user)
        {
            InitializeComponent();
            _currentUser = user;
            Period = period;
            ChartUpdateWeight();
            PeriodComboBox.SelectedIndex = 0;
            DataContext = this;
        }

        private void ChartUpdateWeight()
        {
            var currentDate = DateTime.Now.Date;
            DateWeightTextBlock.Text = _currentPulseChartDate.ToString("dd.MM.yyyy");
            var info = DatabaseContext.DbContext.Context.Weights
                .ToList()
                .Where(x => x.Users == _currentUser && x.MeasurementTimeWeight > currentDate);

            List<WeightsTwo> weights = new List<WeightsTwo>();
            foreach (var weight in info)
            {
                weights.Add(new WeightsTwo
                {
                    WeightTime = weight.MeasurementTimeWeight.Value,
                    Weight = (double)weight.BodyWeight
                });
            }

            LineGraficWeight.ItemsSource = weights;
        }


        public static void ExportToExcelWeight(List<Weights> weights)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            string fileName = $"WeightOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx"; // Добавляем временный штамп к имени файла
            string filePath = System.IO.Path.Combine(downloadsPath, fileName);

            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("WeightData");

                // Добавляем заголовки
                worksheet.Cells[1, 1].Value = "Момент занесения";
                worksheet.Cells[1, 2].Value = "Вес (кг)";

                if (weights.Any()) // Проверяем, есть ли данные
                {
                    // Добавляем данные
                    int row = 2;
                    foreach (var perem in weights)
                    {
                        worksheet.Cells[row, 1].Value = perem.MeasurementTimeWeight;
                        worksheet.Cells[row, 2].Value = perem.BodyWeight;

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

        private void ButtonExportToExcelWeight_Click(object sender, RoutedEventArgs e)
        {
            if (PeriodComboBox.SelectedValue.ToString() == "Сегодня")
            {
                var currentDate = DateTime.Now.Date;
                var info = DatabaseContext.DbContext.Context.Weights.ToList().
                    Where(x => x.Users == _currentUser && x.MeasurementTimeWeight > currentDate);
                List<Weights> weights = new List<Weights>();
                foreach (var perem in info)
                {
                    weights.Add(new Weights
                    {
                        MeasurementTimeWeight = perem.MeasurementTimeWeight,
                        BodyWeight = perem.BodyWeight

                    });
                }

                ExportToExcelWeight(weights);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                // Начальная дата - 6 дней назад от текущего дня
                var startDate = DateTime.Now.Date.AddDays(-6);

                // Конечная дата - текущий момент
                var endDate = DateTime.Now;

                var info = DatabaseContext.DbContext.Context.Weights.ToList()
                    .Where(x => x.Users == _currentUser && x.MeasurementTimeWeight >= startDate && x.MeasurementTimeWeight <= endDate);

                List<Weights> weights = new List<Weights>();
                foreach (var perem in info)
                {
                    weights.Add(new Weights
                    {
                        MeasurementTimeWeight = perem.MeasurementTimeWeight,
                        BodyWeight = perem.BodyWeight

                    });
                }

                ExportToExcelWeight(weights);
            }

        }

        public static void ExportToWordWeight(List<Weights> weights)
        {
            // Создаем новый документ Word
            using (WordDocument document = new WordDocument())
            {
                // Добавляем раздел с заголовком
                WSection section = document.AddSection() as WSection;
                WParagraph paragraph = section.HeadersFooters.Header.AddParagraph() as WParagraph;
                paragraph.AppendText("Weight Data").CharacterFormat.FontSize = 14;
                paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;

                // Добавляем таблицу
                WTable table = section.AddTable() as WTable;
                table.ResetCells(weights.Count + 1, 2); // +1 для заголовков

                // Добавляем заголовки таблицы
                string[] headers = { "Момент занесения", "Температура" };
                for (int i = 0; i < headers.Length; i++)
                {
                    table[0, i].AddParagraph().AppendText(headers[i]);
                    table[0, i].CellFormat.VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment.Middle;
                    //table[0, i].CellFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;
                }

                // Добавляем данные в таблицу
                for (int i = 0; i < weights.Count; i++)
                {
                    Weights weight = weights[i];
                    table[i + 1, 0].AddParagraph().AppendText(weight.MeasurementTimeWeight.ToString());
                    table[i + 1, 1].AddParagraph().AppendText(weight.BodyWeight.ToString());
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
                string fileName = $"WeightOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.docx";
                string filePath = System.IO.Path.Combine(downloadsPath, fileName);
                document.Save(filePath);

                MessageBox.Show($"Файл сохранен в папке \"Загрузки\": {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ButtonExportToWordWeight_Click(object sender, RoutedEventArgs e)
        {
            if (PeriodComboBox.SelectedValue.ToString() == "Сегодня")
            {
                var currentDate = DateTime.Now.Date;
                var info = DatabaseContext.DbContext.Context.Weights.ToList().
                    Where(x => x.Users == _currentUser && x.MeasurementTimeWeight > currentDate);
                List<Weights> weights = new List<Weights>();
                foreach (var perem in info)
                {
                    weights.Add(new Weights
                    {
                        MeasurementTimeWeight = perem.MeasurementTimeWeight,
                        BodyWeight = perem.BodyWeight

                    });
                }

                ExportToWordWeight(weights);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                // Начальная дата - 6 дней назад от текущего дня
                var startDate = DateTime.Now.Date.AddDays(-6);

                // Конечная дата - текущий момент
                var endDate = DateTime.Now;

                var info = DatabaseContext.DbContext.Context.Weights.ToList()
                    .Where(x => x.Users == _currentUser && x.MeasurementTimeWeight >= startDate && x.MeasurementTimeWeight <= endDate);

                List<Weights> weights = new List<Weights>();
                foreach (var perem in info)
                {
                    weights.Add(new Weights
                    {
                        MeasurementTimeWeight = perem.MeasurementTimeWeight,
                        BodyWeight = perem.BodyWeight

                    });
                }

                ExportToWordWeight(weights);
            }

        }


        public void SaveWeight()
        {
            var weightGoal = _currentUser.WeightGoal;
            DateTime today = DateTime.Today;
            DateTime currentTime = DateTime.Now;

            if (Convert.ToDecimal(WeightTextBox.Text) > 650 || Convert.ToDecimal(WeightTextBox.Text) < 10)
            {
                MessageBox.Show("Человек не может иметь такой вес");
                return;
            }

            var newWeightTime = WeightTimePicker.Value;
            if (newWeightTime.Value.Date > today || (newWeightTime.Value.Date == today && newWeightTime.Value.TimeOfDay > currentTime.TimeOfDay))
            {
                MessageBox.Show("Вы не можете добавить вес для будущей даты и времени");
                return; // Прерываем выполнение метода, если дата или время не настоящие
            }

            DatabaseContext.DbContext.Context.Weights.Add(new Weights
            {
                UserID = _currentUser.ID,
                BodyWeight = Convert.ToDecimal(WeightTextBox.Text),
                MeasurementTimeWeight = newWeightTime,
            });

            DatabaseContext.DbContext.Context.SaveChanges();

            var weightsForToday = DatabaseContext.DbContext.Context.Weights
                .Where(m => m.UserID == _currentUser.ID &&
                             DbFunctions.TruncateTime(m.MeasurementTimeWeight) == today)
                .OrderByDescending(m => m.MeasurementTimeWeight)
                .ToList();

            var latestWeightForToday = weightsForToday.FirstOrDefault();

            if (latestWeightForToday != null && latestWeightForToday.MeasurementTimeWeight == newWeightTime)
            {
                if (latestWeightForToday.BodyWeight == weightGoal)
                {
                    ShowNotification("Вес", "Вы достигли своей цели по весу за сегодня!");
                    // Здесь можно выполнить дополнительные действия в случае достижения цели
                }
                else if (latestWeightForToday.BodyWeight > weightGoal)
                {
                    ShowNotification("Вес", "Вы превысили свою цель по весу за сегодня!");
                    // Здесь можно выполнить дополнительные действия в случае превышения цели
                }
                else
                {
                    ShowNotification("Вес", "Продолжайте работу над своей целью по весу!");
                }
            }
        }




        private void ButtonSaveWeight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveWeight();
                ChartUpdateWeight();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}");
            }
        }
    }
}
