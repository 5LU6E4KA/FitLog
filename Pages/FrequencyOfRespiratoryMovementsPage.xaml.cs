using FitLog.ClearFields;
using FitLog.Controls;
using FitLog.Entities;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static FitLog.Controls.CustomMessageBox;
using static FitLog.Notifications.MyNotify;

namespace FitLog.Pages
{

    public class FrequencyOfRespiratoryTwo
    {
        public DateTime FrequencyTime { get; set; }
        public int FrequencyTwo { get; set; }
    }


    public partial class FrequencyOfRespiratoryMovementsPage : Page
    {
        private Users _currentUser;
        public List<string> Period { get; set; }
        public List<string> DayOfWeeks { get; set; }

        private DateTime _currentFrequencyChartDate = DateTime.Now;

        public FrequencyOfRespiratoryMovementsPage(Users user)
        {
            InitializeComponent();
            _currentUser = user;
            Period = period;
            ChartUpdateFrequencyOfRespiratoryMovements();
            PeriodComboBox.SelectedIndex = 0;
            DataContext = this;
        }

        private List<string> period = new List<string>
        {
            "Сегодня", "Неделя"
        };

        private void ChartUpdateFrequencyOfRespiratoryMovements()
        {
            var currentDate = DateTime.Now.Date;
            DateFrequencyTextBlock.Text = _currentFrequencyChartDate.ToString("dd.MM.yyyy");
            var info = DatabaseContext.DbContext.Context.FrequencyOfRespiratoryMovements
                .ToList()
                .Where(x => x.Users == _currentUser && x.MeasurementTimeFrequency > currentDate);

            List<FrequencyOfRespiratoryTwo> frequencyOfRespiratories = new List<FrequencyOfRespiratoryTwo>();
            foreach (var frequency in info)
            {
                frequencyOfRespiratories.Add(new FrequencyOfRespiratoryTwo
                {
                    FrequencyTime = frequency.MeasurementTimeFrequency.Value,
                    FrequencyTwo = (int)frequency.Frequency
                });
            }

            LineGraficFrequency.ItemsSource = frequencyOfRespiratories;
        }


        public static void ExportToExcelFrequencyOfRespiratoryMovements(List<FrequencyOfRespiratoryMovements> frequencies)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            string fileName = $"FrequencyOfRespiratoryMovementsOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx"; 
            string filePath = System.IO.Path.Combine(downloadsPath, fileName);

            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("FrequencyOfRespiratoryMovementsData");

                
                worksheet.Cells[1, 1].Value = "Момент занесения";
                worksheet.Cells[1, 2].Value = "Частота дыхательных движений (вдох/мин)";

                if (frequencies.Any()) 
                {
                    
                    int row = 2;
                    foreach (var frequencie in frequencies)
                    {
                        worksheet.Cells[row, 1].Value = frequencie.MeasurementTimeFrequency;
                        worksheet.Cells[row, 2].Value = frequencie.Frequency;

                        row++;
                    }

                    
                    worksheet.Column(1).Style.Numberformat.Format = "hh:mm:ss dd-mm-yyyy";
                    worksheet.Cells[1, 1, 1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[1, 1, 1, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    
                    worksheet.Cells[2, 1, row - 1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[2, 1, row - 1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells[2, 3, row - 1, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[2, 3, row - 1, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                }

                // Автоподбор ширины столбцов
                worksheet.Cells.AutoFitColumns();

                
                package.Save();
            }
            CustomMessageBox.Show($"Файл сохранен в папке \"Загрузки\": {filePath}", "Успех", MessageWindowImage.Information, MessageWindowButton.Ok);
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Проверка на цифру
            Regex regex = new Regex("^[0-9]+$");

            e.Handled = !regex.IsMatch(e.Text);
        }

        private void ButtonExportToExcelFrequencyOfRespiratoryMovements_Click(object sender, RoutedEventArgs e)
        {
            if (PeriodComboBox.SelectedValue.ToString() == "Сегодня")
            {
                var currentDate = DateTime.Now.Date;
                var info = DatabaseContext.DbContext.Context.FrequencyOfRespiratoryMovements.ToList().
                    Where(x => x.Users == _currentUser && x.MeasurementTimeFrequency > currentDate);
                List<FrequencyOfRespiratoryMovements> frequencies = new List<FrequencyOfRespiratoryMovements>();
                foreach (var perem in info)
                {
                    frequencies.Add(new FrequencyOfRespiratoryMovements
                    {
                        MeasurementTimeFrequency = perem.MeasurementTimeFrequency,
                        Frequency = perem.Frequency

                    });
                }
                frequencies = frequencies.OrderBy(l => l.MeasurementTimeFrequency).ToList();
                ExportToExcelFrequencyOfRespiratoryMovements(frequencies);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                var startDate = DateTime.Now.Date.AddDays(-6);
                var endDate = DateTime.Now;

                var info = DatabaseContext.DbContext.Context.FrequencyOfRespiratoryMovements.ToList()
                    .Where(x => x.Users == _currentUser && x.MeasurementTimeFrequency >= startDate && x.MeasurementTimeFrequency <= endDate);

                List<FrequencyOfRespiratoryMovements> frequencies = new List<FrequencyOfRespiratoryMovements>();
                foreach (var perem in info)
                {
                    frequencies.Add(new FrequencyOfRespiratoryMovements
                    {
                        MeasurementTimeFrequency = perem.MeasurementTimeFrequency,
                        Frequency = perem.Frequency

                    });
                }
                frequencies = frequencies.OrderBy(l => l.MeasurementTimeFrequency).ToList();
                ExportToExcelFrequencyOfRespiratoryMovements(frequencies);

            }
        }

        public static void ExportToWordFrequencyOfRespiratoryMovements(List<FrequencyOfRespiratoryMovements> frequencies)
        {
            using (WordDocument document = new WordDocument())
            {
                WSection section = document.AddSection() as WSection;
                WParagraph paragraph = section.HeadersFooters.Header.AddParagraph() as WParagraph;
                paragraph.AppendText("FrequencyOfRespiratoryMovements Data").CharacterFormat.FontSize = 14;
                paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;

                WTable table = section.AddTable() as WTable;
                table.ResetCells(frequencies.Count + 1, 2); 

                
                string[] headers = { "Момент занесения", "Температура" };
                for (int i = 0; i < headers.Length; i++)
                {
                    table[0, i].AddParagraph().AppendText(headers[i]);
                    table[0, i].CellFormat.VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment.Middle;
                    
                }

                
                for (int i = 0; i < frequencies.Count; i++)
                {
                    FrequencyOfRespiratoryMovements frequency = frequencies[i];
                    table[i + 1, 0].AddParagraph().AppendText(frequency.MeasurementTimeFrequency.ToString());
                    table[i + 1, 1].AddParagraph().AppendText(frequency.Frequency.ToString());
                }

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

                string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
                string fileName = $"FrequencyOfRespiratoryMovementsOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.docx";
                string filePath = System.IO.Path.Combine(downloadsPath, fileName);
                document.Save(filePath);
                CustomMessageBox.Show($"Файл сохранен в папке \"Загрузки\": {filePath}", "Успех", MessageWindowImage.Information, MessageWindowButton.Ok);
            }
        }

        private void ButtonExportToWordFrequencyOfRespiratoryMovements_Click(object sender, RoutedEventArgs e)
        {
            if (PeriodComboBox.SelectedValue.ToString() == "Сегодня")
            {
                var currentDate = DateTime.Now.Date;
                var info = DatabaseContext.DbContext.Context.FrequencyOfRespiratoryMovements.ToList().
                    Where(x => x.Users == _currentUser && x.MeasurementTimeFrequency > currentDate);
                List<FrequencyOfRespiratoryMovements> frequencies = new List<FrequencyOfRespiratoryMovements>();
                foreach (var perem in info)
                {
                    frequencies.Add(new FrequencyOfRespiratoryMovements
                    {
                        MeasurementTimeFrequency = perem.MeasurementTimeFrequency,
                        Frequency = perem.Frequency

                    });
                }
                frequencies = frequencies.OrderBy(l => l.MeasurementTimeFrequency).ToList();
                ExportToWordFrequencyOfRespiratoryMovements(frequencies);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                var startDate = DateTime.Now.Date.AddDays(-6);

                var endDate = DateTime.Now;

                var info = DatabaseContext.DbContext.Context.FrequencyOfRespiratoryMovements.ToList()
                    .Where(x => x.Users == _currentUser && x.MeasurementTimeFrequency >= startDate && x.MeasurementTimeFrequency <= endDate);

                List<FrequencyOfRespiratoryMovements> frequencies = new List<FrequencyOfRespiratoryMovements>();
                foreach (var perem in info)
                {
                    frequencies.Add(new FrequencyOfRespiratoryMovements
                    {
                        MeasurementTimeFrequency = perem.MeasurementTimeFrequency,
                        Frequency = perem.Frequency

                    });
                }
                frequencies = frequencies.OrderBy(l => l.MeasurementTimeFrequency).ToList();
                ExportToWordFrequencyOfRespiratoryMovements(frequencies);

            }

        }


        public void SaveFrequencyOfRespiratoryMovements()
        {
            var frequencyGoal = _currentUser.FrequencyGoal;
            DateTime today = DateTime.Today;
            DateTime currentTime = DateTime.Now;
            DateTime? newFrequencyTime = FrequencyTimePicker.Value;

            int frequencyConsumed;
            if (!int.TryParse(FrequencyTextBox.Text, out frequencyConsumed))
            {
                CustomMessageBox.Show("Некорректное значение для частоты дыхания", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (newFrequencyTime == null)
            {
                CustomMessageBox.Show("Пожалуйста, заполните временной интервал", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (frequencyConsumed > 60 || frequencyConsumed < 5)
            {
                CustomMessageBox.Show("Человек не может иметь такой показатель частоты дыхания", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (newFrequencyTime.Value.Date > today)
            {
                CustomMessageBox.Show("Нельзя вводить данные на будущие даты", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            DatabaseContext.DbContext.Context.FrequencyOfRespiratoryMovements.Add(new FrequencyOfRespiratoryMovements
            {
                UserID = _currentUser.ID,
                Frequency = frequencyConsumed,
                MeasurementTimeFrequency = newFrequencyTime.Value,
            });

            DatabaseContext.DbContext.Context.SaveChanges();
            FrequencyTimePicker.Text = "";
            ClearField.ClearTextBoxes(this);

            if (newFrequencyTime.Value.Date == today)
            {
                var todayFrequencies = DatabaseContext.DbContext.Context.FrequencyOfRespiratoryMovements
                    .Where(m => m.UserID == _currentUser.ID &&
                                DbFunctions.TruncateTime(m.MeasurementTimeFrequency) == today)
                    .ToList();

                var averageFrequencyForToday = todayFrequencies.Any() ? todayFrequencies.Average(x => x.Frequency) : 0;

                if (averageFrequencyForToday == frequencyGoal)
                {
                    ShowNotification("Дыхание", "Вы достигли своей цели за сегодня!");
                }
                else if (averageFrequencyForToday < frequencyGoal)
                {
                    ShowNotification("Дыхание", "Продолжайте работу над своей целью!");
                }
                else
                {
                    ShowNotification("Дыхание", "Вы превысили свою цель!");
                }
            }
        }


        private void OpenFirstLink(object sender, RoutedEventArgs e)
        {
            string url = "https://www.tyzine.ru/polezno-znat/dyhatelnaja-gimnastika";

            Process.Start(url);
        }

        private void OpenSecondLink(object sender, RoutedEventArgs e)
        {
            string url = "https://www.tyzine.ru/polezno-znat/pravilnoe-dyhanie";

            Process.Start(url);
        }


        private void ButtonSaveFrequencyOfRespiratoryMovements_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFrequencyOfRespiratoryMovements();
                ChartUpdateFrequencyOfRespiratoryMovements();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при сохранении данных: {ex.Message}","Ошибка", MessageWindowImage.Error, MessageWindowButton.Ok);
            }
        }
    }

}

