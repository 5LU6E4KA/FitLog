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
            string fileName = $"PulseOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx";
            string filePath = System.IO.Path.Combine(downloadsPath, fileName);

            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("PulseData");

                
                worksheet.Cells[1, 1].Value = "Момент занесения";
                worksheet.Cells[1, 2].Value = "Пульс";

                if (pulses.Any())
                {
                  
                    int row = 2;
                    foreach (var pulse in pulses)
                    {
                        worksheet.Cells[row, 1].Value = pulse.MeasurementTimePulse;
                        worksheet.Cells[row, 2].Value = pulse.FrequencyOfContractions;

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

                
                worksheet.Cells.AutoFitColumns();

               
                package.Save();
            }
            CustomMessageBox.Show($"Файл сохранен в папке \"Загрузки\": {filePath}", "Успех", MessageWindowImage.Information, MessageWindowButton.Ok);
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            
            Regex regex = new Regex("^[0-9]+$");

            e.Handled = !regex.IsMatch(e.Text);
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
                pulses = pulses.OrderBy(l => l.MeasurementTimePulse).ToList();
                ExportToExcelPulse(pulses);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                
                var startDate = DateTime.Now.Date.AddDays(-6);

                
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
                pulses = pulses.OrderBy(l => l.MeasurementTimePulse).ToList();
                ExportToExcelPulse(pulses);
            }

        }

        public static void ExportToWordPulse(List<Pulses> pulses)
        {
            
            using (WordDocument document = new WordDocument())
            {
                
                WSection section = document.AddSection() as WSection;
                WParagraph paragraph = section.HeadersFooters.Header.AddParagraph() as WParagraph;
                paragraph.AppendText("Pulse Data").CharacterFormat.FontSize = 14;
                paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;

                
                WTable table = section.AddTable() as WTable;
                table.ResetCells(pulses.Count + 1, 2); 

                
                string[] headers = { "Момент занесения", "Пульс" };
                for (int i = 0; i < headers.Length; i++)
                {
                    table[0, i].AddParagraph().AppendText(headers[i]);
                    table[0, i].CellFormat.VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment.Middle;
                    
                }

                
                for (int i = 0; i < pulses.Count; i++)
                {
                    Pulses pulse = pulses[i];
                    table[i + 1, 0].AddParagraph().AppendText(pulse.MeasurementTimePulse.ToString());
                    table[i + 1, 1].AddParagraph().AppendText(pulse.FrequencyOfContractions.ToString());
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
                string fileName = $"SleepOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.docx";
                string filePath = System.IO.Path.Combine(downloadsPath, fileName);
                document.Save(filePath);

               CustomMessageBox.Show($"Файл сохранен в папке \"Загрузки\": {filePath}", "Успех", MessageWindowImage.Information, MessageWindowButton.Ok);
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
                pulses = pulses.OrderBy(l => l.MeasurementTimePulse).ToList();
                ExportToWordPulse(pulses);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                var startDate = DateTime.Now.Date.AddDays(-6);

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
                pulses = pulses.OrderBy(l => l.MeasurementTimePulse).ToList();
                ExportToWordPulse(pulses);
            }

        }

        private void OpenFirstLink(object sender, RoutedEventArgs e)
        {
            string url = "https://wer.ru/articles/kakoy-puls-schitaetsya-normalnym-dlya-cheloveka-togo-ili-inogo-vozrasta-svodnaya-tablitsa-znacheniy-/";

            Process.Start(url);
        }

        private void OpenSecondLink(object sender, RoutedEventArgs e)
        {
            string url = "https://dgp6-omsk.ru/ru/kak-privesti-puls-v-normu-bystro";

            Process.Start(url);
        }


        public void SavePulse()
        {
            DateTime today = DateTime.Today;

            var newPulseTime = PulseTimePicker.Value;

            int pulseConsumed;
            if (!int.TryParse(PulseTextBox.Text, out pulseConsumed))
            {
                CustomMessageBox.Show("Некорректное значение для пульса", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (newPulseTime == default)
            {
                CustomMessageBox.Show("Пожалуйста, заполните временной интервал", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (newPulseTime.Value.Date > today)
            {
                CustomMessageBox.Show("Нельзя вводить данные на будущие даты", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (Convert.ToInt32(PulseTextBox.Text) > 300 || Convert.ToInt32(PulseTextBox.Text) < 0)
            {
                CustomMessageBox.Show("Данное значение пульса невозможно у человека", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            DatabaseContext.DbContext.Context.Pulses.Add(new FitLog.Entities.Pulses
            {
                UserID = _currentUser.ID,
                FrequencyOfContractions = Convert.ToInt32(PulseTextBox.Text),
                MeasurementTimePulse = PulseTimePicker.Value
            });

            DatabaseContext.DbContext.Context.SaveChanges();

            PulseTimePicker.Text = "";
            ClearField.ClearTextBoxes(this);

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
                CustomMessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageWindowImage.Error, MessageWindowButton.Ok);
            }
        }
    }
}
