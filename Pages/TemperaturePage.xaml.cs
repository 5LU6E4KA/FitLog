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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using FitLog.Entities;
using static FitLog.DateClass.DateInfo;
using System.ComponentModel;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Drawing.Chart;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.Drawing;
using OfficeOpenXml.Packaging.Ionic.Zlib;
using Syncfusion.UI.Xaml.Charts;

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

        private List<string> _place = new List<string>
        {
            "Не указано", "Подмышка", "Лоб", "Запястье", "Височная артерия"
        };
        private List<string> period = new List<string>
        {
            "Сегодня", "Неделя"
        };

        public TemperaturePage(Users user)
        {
            InitializeComponent();
            Place = _place;
            Period = period;
            PlaceComboBox.SelectedIndex = 0;
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
            string fileName = $"TemperatureOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx"; // Добавляем временный штамп к имени файла
            string filePath = System.IO.Path.Combine(downloadsPath, fileName);

            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("TemperatureData");

                // Добавляем заголовки
                worksheet.Cells[1, 1].Value = "Момент занесения";
                worksheet.Cells[1, 2].Value = "Температура";

                if (temperatures.Any()) // Проверяем, есть ли данные
                {
                    // Добавляем данные
                    int row = 2;
                    foreach (var temperature in temperatures)
                    {
                        worksheet.Cells[row, 1].Value = temperature.MeasurementTimeTemperature;
                        worksheet.Cells[row, 2].Value = temperature.BodyTemperature;

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

                ExportToExcelTemperature(temperatures);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                // Начальная дата - 6 дней назад от текущего дня
                var startDate = DateTime.Now.Date.AddDays(-6);

                // Конечная дата - текущий момент
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

                ExportToExcelTemperature(temperatures);
            }

        }

        public static void ExportToWordTemperature(List<Temperatures> temperatures)
        {
            // Создаем новый документ Word
            using (WordDocument document = new WordDocument())
            {
                // Добавляем раздел с заголовком
                WSection section = document.AddSection() as WSection;
                WParagraph paragraph = section.HeadersFooters.Header.AddParagraph() as WParagraph;
                paragraph.AppendText("Temperature Data").CharacterFormat.FontSize = 14;
                paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;

                // Добавляем таблицу
                WTable table = section.AddTable() as WTable;
                table.ResetCells(temperatures.Count + 1, 2); // +1 для заголовков

                // Добавляем заголовки таблицы
                string[] headers = { "Момент занесения", "Температура" };
                for (int i = 0; i < headers.Length; i++)
                {
                    table[0, i].AddParagraph().AppendText(headers[i]);
                    table[0, i].CellFormat.VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment.Middle;
                    //table[0, i].CellFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;
                }

                // Добавляем данные в таблицу
                for (int i = 0; i < temperatures.Count; i++)
                {
                    Temperatures temperature = temperatures[i];
                    table[i + 1, 0].AddParagraph().AppendText(temperature.MeasurementTimeTemperature.ToString());
                    table[i + 1, 1].AddParagraph().AppendText(temperature.BodyTemperature.ToString());
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
                string fileName = $"TemperatureOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.docx";
                string filePath = System.IO.Path.Combine(downloadsPath, fileName);
                document.Save(filePath);

                MessageBox.Show($"Файл сохранен в папке \"Загрузки\": {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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

                ExportToWordTemperature(temperatures);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                // Начальная дата - 6 дней назад от текущего дня
                var startDate = DateTime.Now.Date.AddDays(-6);

                // Конечная дата - текущий момент
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

                ExportToWordTemperature(temperatures);
            }

        }


        public void SaveTemperature()
        {

            if (string.IsNullOrWhiteSpace(TemperatureTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите температуру тела");
                return;
            }

            if (Convert.ToDecimal(TemperatureTextBox.Text) > 42 || Convert.ToDecimal(TemperatureTextBox.Text) < 0)
            {
                MessageBox.Show("Человек не может иметь такую температуру");
                return;
            }

            DatabaseContext.DbContext.Context.Temperatures.Add(new Temperatures
            {
                UserID = _currentUser.ID,
                BodyTemperature = Convert.ToDecimal(TemperatureTextBox.Text),
                MeasurementTimeTemperature = TemperatureTimeDateTimePicker.Value,
                MeasurementPlace = PlaceComboBox.SelectedItem?.ToString()
            });

            DatabaseContext.DbContext.Context.SaveChanges();

            //ClearField.ClearTextBoxes(this);

            PlaceComboBox.SelectedIndex = 0;
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
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}");
            }
        }
    }


}

