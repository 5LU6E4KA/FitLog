using FitLog.ClearFields;
using FitLog.Controls;
using FitLog.Entities;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static FitLog.Controls.CustomMessageBox;

namespace FitLog.Pages
{
    public class TemperaturesTwo
    {
        public DateTime TemperatureTime { get; set; }
        public double Temperature { get; set; }
    }

    public partial class TemperaturePage : Page
    {
        private Users _currentUser;
        private DateTime _currentPulseChartDate = DateTime.Now;
        public List<string> Period { get; set; }
        public List<string> Place { get; set; }


        private List<string> period = new List<string>
        {
            "Сегодня", "Неделя"
        };

        public TemperaturePage(Users user)
        {
            InitializeComponent();
            Period = period;

            _currentUser = user;
            ChartUpdateTemperature();
            PeriodComboBox.SelectedIndex = 0;
            DataContext = this;
        }

        private void ChartUpdateTemperature()
        {
            var currentDate = DateTime.Now.Date;
            DateTemperatureTextBlock.Text = _currentPulseChartDate.ToString("dd.MM.yyyy");
            var info = DatabaseContext.DbContext.Context.Temperatures
                .ToList()
                .Where(x => x.Users == _currentUser && x.MeasurementTimeTemperature > currentDate);

            List<TemperaturesTwo> temperatures = new List<TemperaturesTwo>();
            foreach (var temperatureMeasurement in info)
            {
                temperatures.Add(new TemperaturesTwo
                {
                    TemperatureTime = temperatureMeasurement.MeasurementTimeTemperature.Value,
                    Temperature = (double)temperatureMeasurement.BodyTemperature
                });
            }

            LineGraficTemperature.ItemsSource = temperatures;
        }


        public static void ExportToExcelTemperature(List<Temperatures> temperatures)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            string fileName = $"TemperatureOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx";
            string filePath = System.IO.Path.Combine(downloadsPath, fileName);

            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("TemperatureData");

                
                worksheet.Cells[1, 1].Value = "Момент занесения";
                worksheet.Cells[1, 2].Value = "Температура";

                if (temperatures.Any()) 
                {
                    
                    int row = 2;
                    foreach (var temperature in temperatures)
                    {
                        worksheet.Cells[row, 1].Value = temperature.MeasurementTimeTemperature;
                        worksheet.Cells[row, 2].Value = temperature.BodyTemperature;

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

        private void ButtonExportToExcelTemperature_Click(object sender, RoutedEventArgs e)
        {
            if (PeriodComboBox.SelectedValue.ToString() == "Сегодня")
            {
                var currentDate = DateTime.Now.Date;
                var info = DatabaseContext.DbContext.Context.Temperatures.ToList().
                    Where(x => x.Users == _currentUser && x.MeasurementTimeTemperature > currentDate);
                List<Temperatures> temperatures = new List<Temperatures>();
                foreach (var perem in info)
                {
                    temperatures.Add(new Temperatures
                    {
                        MeasurementTimeTemperature = perem.MeasurementTimeTemperature,
                        BodyTemperature = perem.BodyTemperature

                    });
                }
                temperatures = temperatures.OrderBy(l => l.MeasurementTimeTemperature).ToList();
                ExportToExcelTemperature(temperatures);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                
                var startDate = DateTime.Now.Date.AddDays(-6);

               
                var endDate = DateTime.Now;

                var info = DatabaseContext.DbContext.Context.Temperatures.ToList()
                    .Where(x => x.Users == _currentUser && x.MeasurementTimeTemperature >= startDate && x.MeasurementTimeTemperature <= endDate);

                List<Temperatures> temperatures = new List<Temperatures>();
                foreach (var perem in info)
                {
                    temperatures.Add(new Temperatures
                    {
                        MeasurementTimeTemperature = perem.MeasurementTimeTemperature,
                        BodyTemperature = perem.BodyTemperature

                    });
                }
                temperatures = temperatures.OrderBy(l => l.MeasurementTimeTemperature).ToList();
                ExportToExcelTemperature(temperatures);
            }

        }

        public static void ExportToWordTemperature(List<Temperatures> temperatures)
        {
            
            using (WordDocument document = new WordDocument())
            {
                
                WSection section = document.AddSection() as WSection;
                WParagraph paragraph = section.HeadersFooters.Header.AddParagraph() as WParagraph;
                paragraph.AppendText("Temperature Data").CharacterFormat.FontSize = 14;
                paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;

                
                WTable table = section.AddTable() as WTable;
                table.ResetCells(temperatures.Count + 1, 2); 

               
                string[] headers = { "Момент занесения", "Температура" };
                for (int i = 0; i < headers.Length; i++)
                {
                    table[0, i].AddParagraph().AppendText(headers[i]);
                    table[0, i].CellFormat.VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment.Middle;
                   
                }

                
                for (int i = 0; i < temperatures.Count; i++)
                {
                    Temperatures temperature = temperatures[i];
                    table[i + 1, 0].AddParagraph().AppendText(temperature.MeasurementTimeTemperature.ToString());
                    table[i + 1, 1].AddParagraph().AppendText(temperature.BodyTemperature.ToString());
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
                string fileName = $"TemperatureOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.docx";
                string filePath = System.IO.Path.Combine(downloadsPath, fileName);
                document.Save(filePath);

                CustomMessageBox.Show($"Файл сохранен в папке \"Загрузки\": {filePath}", "Успех", MessageWindowImage.Information, MessageWindowButton.Ok);
            }
        }

        private void ButtonExportToWordTemperature_Click(object sender, RoutedEventArgs e)
        {
            if (PeriodComboBox.SelectedValue.ToString() == "Сегодня")
            {
                var currentDate = DateTime.Now.Date;
                var info = DatabaseContext.DbContext.Context.Temperatures.ToList().
                    Where(x => x.Users == _currentUser && x.MeasurementTimeTemperature > currentDate);
                List<Temperatures> temperatures = new List<Temperatures>();
                foreach (var perem in info)
                {
                    temperatures.Add(new Temperatures
                    {
                        MeasurementTimeTemperature = perem.MeasurementTimeTemperature,
                        BodyTemperature = perem.BodyTemperature

                    });
                }
                temperatures = temperatures.OrderBy(l => l.MeasurementTimeTemperature).ToList();
                ExportToWordTemperature(temperatures);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                
                var startDate = DateTime.Now.Date.AddDays(-6);

                
                var endDate = DateTime.Now;

                var info = DatabaseContext.DbContext.Context.Temperatures.ToList()
                    .Where(x => x.Users == _currentUser && x.MeasurementTimeTemperature >= startDate && x.MeasurementTimeTemperature <= endDate);

                List<Temperatures> temperatures = new List<Temperatures>();
                foreach (var perem in info)
                {
                    temperatures.Add(new Temperatures
                    {
                        MeasurementTimeTemperature = perem.MeasurementTimeTemperature,
                        BodyTemperature = perem.BodyTemperature

                    });
                }
                temperatures = temperatures.OrderBy(l => l.MeasurementTimeTemperature).ToList();
                ExportToWordTemperature(temperatures);
            }

        }


        public void SaveTemperature()
        {
            DateTime today = DateTime.Today;

            var newTemperatureTime = TemperatureTimeDateTimePicker.Value;
            decimal temperatureConsumed;
            if (!decimal.TryParse(TemperatureTextBox.Text, out temperatureConsumed))
            {
                CustomMessageBox.Show("Некорректное значение для температуры тела", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (newTemperatureTime == default)
            {
                CustomMessageBox.Show("Пожалуйста, заполните временной интервал", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (newTemperatureTime.Value.Date > today)
            {
                CustomMessageBox.Show("Нельзя вводить данные на будущие даты", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (Convert.ToDecimal(TemperatureTextBox.Text) > 42 || Convert.ToDecimal(TemperatureTextBox.Text) < 0)
            {
                CustomMessageBox.Show("Человек не может иметь такую температуру", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            DatabaseContext.DbContext.Context.Temperatures.Add(new Temperatures
            {
                UserID = _currentUser.ID,
                BodyTemperature = Convert.ToDecimal(TemperatureTextBox.Text),
                MeasurementTimeTemperature = TemperatureTimeDateTimePicker.Value,
            });

            DatabaseContext.DbContext.Context.SaveChanges();

            TemperatureTimeDateTimePicker.Text = "";
            ClearField.ClearTextBoxes(this);

        }

        private void OpenFirstLink(object sender, RoutedEventArgs e)
        {
            string url = "https://65.mchs.gov.ru/deyatelnost/press-centr/novosti/4523467";

            Process.Start(url);
        }

        private void OpenSecondLink(object sender, RoutedEventArgs e)
        {
            string url = "https://www.medicina.ru/press-tsentr/statyi/chto-delat-pri-povyshenii-temperatury/";

            Process.Start(url);
        }

        private void ButtonSaveTemperature_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveTemperature();
                ChartUpdateTemperature();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageWindowImage.Error, MessageWindowButton.Ok);
            }
        }
    }


}

