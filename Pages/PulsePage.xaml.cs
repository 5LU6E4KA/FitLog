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

namespace FitLog.Pages
{
    public class PulsesTwo
    {
        public DateTime Time { get; set; }
        public double PulseGraf { get; set; }
    }

    public partial class PulsePage : Page
    {
        private Users _currentUser;
        private DateTime _currentPulseChartDate = DateTime.Now;
        public List<string> Period { get; set; }
        public PulsePage(Users user)
        {
            InitializeComponent();
            _currentUser = user;
            ChartUpdatePulse();
            Period = period;
            PeriodComboBox.SelectedIndex = 0;
            DataContext = this;
        }

        private List<string> period = new List<string>
        {
            "Сегодня", "Неделя"
        };

        private void ChartUpdatePulse()
        {
            var currentDate = DateTime.Now.Date;
            CurrentDatePulseTextBlock.Text = _currentPulseChartDate.ToString("dd.MM.yyyy");
            var info = DatabaseContext.DbContext.Context.Pulses.ToList().
                Where(x => x.Users == _currentUser && x.MeasurementTimePulse > currentDate);
            List<PulsesTwo> pulses = new List<PulsesTwo>();
            foreach (var pulseMeasurement in info)
            {
                pulses.Add(new PulsesTwo
                {
                    Time = pulseMeasurement.MeasurementTimePulse.Value,
                    PulseGraf = (double)pulseMeasurement.FrequencyOfContractions
                });
            }

            LineGraficPulse.ItemsSource = pulses;
        }


        public static void ExportToExcelPulse(List<Pulses> pulses)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            string fileName = $"PulseOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx"; // Добавляем временный штамп к имени файла
            string filePath = System.IO.Path.Combine(downloadsPath, fileName);

            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("PulseData");

                // Добавляем заголовки
                worksheet.Cells[1, 1].Value = "Момент занесения";
                worksheet.Cells[1, 2].Value = "Пульс";

                if (pulses.Any()) // Проверяем, есть ли данные
                {
                    // Добавляем данные
                    int row = 2;
                    foreach (var pulse in pulses)
                    {
                        worksheet.Cells[row, 1].Value = pulse.MeasurementTimePulse;
                        worksheet.Cells[row, 2].Value = pulse.FrequencyOfContractions;
                        
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

        private void ButtonExportToExcelPulse_Click(object sender, RoutedEventArgs e)
        {
            if (PeriodComboBox.SelectedValue.ToString() == "Сегодня")
            {
                var currentDate = DateTime.Now.Date;
                var info = DatabaseContext.DbContext.Context.Pulses.ToList().
                    Where(x => x.Users == _currentUser && x.MeasurementTimePulse > currentDate);
                List<Pulses> pulses = new List<Pulses>();
                foreach (var perem in info)
                {
                    pulses.Add(new Pulses
                    {
                        MeasurementTimePulse = perem.MeasurementTimePulse,
                        FrequencyOfContractions = perem.FrequencyOfContractions

                    });
                }

                ExportToExcelPulse(pulses);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                // Начальная дата - 6 дней назад от текущего дня
                var startDate = DateTime.Now.Date.AddDays(-6);

                // Конечная дата - текущий момент
                var endDate = DateTime.Now;

                var info = DatabaseContext.DbContext.Context.Pulses.ToList()
                    .Where(x => x.Users == _currentUser && x.MeasurementTimePulse >= startDate && x.MeasurementTimePulse <= endDate);

                List<Pulses> pulses = new List<Pulses>();
                foreach (var perem in info)
                {
                    pulses.Add(new Pulses
                    {
                        MeasurementTimePulse = perem.MeasurementTimePulse,
                        FrequencyOfContractions = perem.FrequencyOfContractions

                    });
                }

                ExportToExcelPulse(pulses);
            }

        }

        public static void ExportToWordPulse(List<Pulses> pulses)
        {
            // Создаем новый документ Word
            using (WordDocument document = new WordDocument())
            {
                // Добавляем раздел с заголовком
                WSection section = document.AddSection() as WSection;
                WParagraph paragraph = section.HeadersFooters.Header.AddParagraph() as WParagraph;
                paragraph.AppendText("Pulse Data").CharacterFormat.FontSize = 14;
                paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;

                // Добавляем таблицу
                WTable table = section.AddTable() as WTable;
                table.ResetCells(pulses.Count + 1, 2); // +1 для заголовков

                // Добавляем заголовки таблицы
                string[] headers = { "Момент занесения", "Пульс"};
                for (int i = 0; i < headers.Length; i++)
                {
                    table[0, i].AddParagraph().AppendText(headers[i]);
                    table[0, i].CellFormat.VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment.Middle;
                    //table[0, i].CellFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;
                }

                // Добавляем данные в таблицу
                for (int i = 0; i < pulses.Count; i++)
                {
                    Pulses pulse = pulses[i];
                    table[i + 1, 0].AddParagraph().AppendText(pulse.MeasurementTimePulse.ToString());
                    table[i + 1, 1].AddParagraph().AppendText(pulse.FrequencyOfContractions.ToString());
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

        private void ButtonExportToWordPulse_Click(object sender, RoutedEventArgs e)
        {
            if (PeriodComboBox.SelectedValue.ToString() == "Сегодня")
            {
                var currentDate = DateTime.Now.Date;
                var info = DatabaseContext.DbContext.Context.Pulses.ToList().
                    Where(x => x.Users == _currentUser && x.MeasurementTimePulse > currentDate);
                List<Pulses> pulses = new List<Pulses>();
                foreach (var perem in info)
                {
                    pulses.Add(new Pulses
                    {
                        MeasurementTimePulse = perem.MeasurementTimePulse,
                        FrequencyOfContractions = perem.FrequencyOfContractions

                    });
                }

                ExportToWordPulse(pulses);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                // Начальная дата - 6 дней назад от текущего дня
                var startDate = DateTime.Now.Date.AddDays(-6);

                // Конечная дата - текущий момент
                var endDate = DateTime.Now;

                var info = DatabaseContext.DbContext.Context.Pulses.ToList()
                    .Where(x => x.Users == _currentUser && x.MeasurementTimePulse >= startDate && x.MeasurementTimePulse <= endDate);

                List<Pulses> pulses = new List<Pulses>();
                foreach (var perem in info)
                {
                    pulses.Add(new Pulses
                    {
                        MeasurementTimePulse = perem.MeasurementTimePulse,
                        FrequencyOfContractions = perem.FrequencyOfContractions

                    });
                }

                ExportToWordPulse(pulses);
            }

        }
        

        public void SavePulse()
        {

            if (string.IsNullOrWhiteSpace(PulseTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите пульс");
                return;
            }

            if (Convert.ToInt32(PulseTextBox.Text) > 300 || Convert.ToInt32(PulseTextBox.Text) < 0)
            {
                MessageBox.Show("Данное значение пульса невозможно у человека");
                return;
            }

            DatabaseContext.DbContext.Context.Pulses.Add(new FitLog.Entities.Pulses
            {
                UserID = _currentUser.ID,
                FrequencyOfContractions = Convert.ToInt32(PulseTextBox.Text),
                MeasurementTimePulse = PulseTimePicker.Value
            });

            DatabaseContext.DbContext.Context.SaveChanges();

            //ClearField.ClearTextBoxes(this);

        }

        private void ButtonSavePulse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SavePulse();
                ChartUpdatePulse();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}");
            }
        }
    }
}
