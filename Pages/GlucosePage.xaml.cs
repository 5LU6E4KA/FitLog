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
    public class GlucosesTwo
    {
        public DateTime GlucoseTime { get; set; }
        public double Glucose { get; set; }
    }

    public partial class GlucosePage : Page
    {
        private Users _currentUser;
        private DateTime _currentPulseChartDate = DateTime.Now;
        public List<string> Period { get; set; }

        private List<string> period = new List<string>
        {
            "Сегодня", "Неделя"
        };

        public GlucosePage(Users user)
        {
            InitializeComponent();
            _currentUser = user;
            Period = period;
            ChartUpdateGlucose();
            PeriodComboBox.SelectedIndex = 0;
            GlucoseTextBox.AddHandler(DataObject.PastingEvent, new DataObjectPastingEventHandler(OnPasting));
            GlucoseTimePicker.AddHandler(DataObject.PastingEvent, new DataObjectPastingEventHandler(OnPasting));
            DataContext = this;
        }

        private void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            // Отменяем вставку любых данных
            e.CancelCommand();
        }

        private void ChartUpdateGlucose()
        {
            var currentDate = DateTime.Now.Date;
            DateGlucoseTextBlock.Text = _currentPulseChartDate.ToString("dd.MM.yyyy");
            var info = DatabaseContext.DbContext.Context.Glucoses
                .ToList()
                .Where(x => x.Users == _currentUser && x.MeasurementTimeGlucose > currentDate);

            List<GlucosesTwo> glucoses = new List<GlucosesTwo>();
            foreach (var glucose in info)
            {
                glucoses.Add(new GlucosesTwo
                {
                    GlucoseTime = glucose.MeasurementTimeGlucose.Value,
                    Glucose = (double)glucose.GlucoseLevel
                });
            }

            LineGraficGlucose.ItemsSource = glucoses;
        }


        public static void ExportToExcelGlucose(List<Glucoses> glucoses)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            string fileName = $"GlucoseOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx"; 
            string filePath = System.IO.Path.Combine(downloadsPath, fileName);

            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("GlucoseData");

                
                worksheet.Cells[1, 1].Value = "Момент занесения";
                worksheet.Cells[1, 2].Value = "Уровеь глюкозы (ммоль/л)";

                if (glucoses.Any()) 
                {
                    
                    int row = 2;
                    foreach (var glucose in glucoses)
                    {
                        worksheet.Cells[row, 1].Value = glucose.MeasurementTimeGlucose;
                        worksheet.Cells[row, 2].Value = glucose.GlucoseLevel;

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
            // Проверка на цифру и запятую
            Regex regex = new Regex("^[0-9,]+$");

            e.Handled = !regex.IsMatch(e.Text);
        }

        private void ButtonExportToExcelGlucose_Click(object sender, RoutedEventArgs e)
        {
            if (PeriodComboBox.SelectedValue.ToString() == "Сегодня")
            {
                var currentDate = DateTime.Now.Date;
                var info = DatabaseContext.DbContext.Context.Glucoses.ToList().
                    Where(x => x.Users == _currentUser && x.MeasurementTimeGlucose > currentDate);
                List<Glucoses> glucoses = new List<Glucoses>();
                foreach (var perem in info)
                {
                    glucoses.Add(new Glucoses
                    {
                        MeasurementTimeGlucose = perem.MeasurementTimeGlucose,
                        GlucoseLevel = perem.GlucoseLevel

                    });
                }
                glucoses = glucoses.OrderBy(l => l.MeasurementTimeGlucose).ToList();
                ExportToExcelGlucose(glucoses);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                
                var startDate = DateTime.Now.Date.AddDays(-6);

                
                var endDate = DateTime.Now;

                var info = DatabaseContext.DbContext.Context.Glucoses.ToList()
                    .Where(x => x.Users == _currentUser && x.MeasurementTimeGlucose >= startDate && x.MeasurementTimeGlucose <= endDate);

                List<Glucoses> glucoses = new List<Glucoses>();
                foreach (var perem in info)
                {
                    glucoses.Add(new Glucoses
                    {
                        MeasurementTimeGlucose = perem.MeasurementTimeGlucose,
                        GlucoseLevel = perem.GlucoseLevel

                    });
                }
                glucoses = glucoses.OrderBy(l => l.MeasurementTimeGlucose).ToList();
                ExportToExcelGlucose(glucoses);
            }

        }

        private void OpenFirstLink(object sender, RoutedEventArgs e)
        {
            string url = "https://mgbsmp.by/informatsiya/shkola-sakharnogo-diabeta/562-tema-7-kontrol-glyukozy-krovi";

            Process.Start(url);
        }

        private void OpenSecondLink(object sender, RoutedEventArgs e)
        {
            string url = "https://yaltastom.ru/info/sposoby-povysheniya-urovnya-glyukozy-pri-diabete";

            Process.Start(url);
        }

        public static void ExportToWordGlucose(List<Glucoses> glucoses)
        {
            
            using (WordDocument document = new WordDocument())
            {
                
                WSection section = document.AddSection() as WSection;
                WParagraph paragraph = section.HeadersFooters.Header.AddParagraph() as WParagraph;
                paragraph.AppendText("Glucose Data").CharacterFormat.FontSize = 14;
                paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;

                
                WTable table = section.AddTable() as WTable;
                table.ResetCells(glucoses.Count + 1, 2); 

                
                string[] headers = { "Момент занесения", "Температура" };
                for (int i = 0; i < headers.Length; i++)
                {
                    table[0, i].AddParagraph().AppendText(headers[i]);
                    table[0, i].CellFormat.VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment.Middle;
                   
                }

                
                for (int i = 0; i < glucoses.Count; i++)
                {
                    Glucoses glucose = glucoses[i];
                    table[i + 1, 0].AddParagraph().AppendText(glucose.MeasurementTimeGlucose.ToString());
                    table[i + 1, 1].AddParagraph().AppendText(glucose.GlucoseLevel.ToString());
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
                string fileName = $"GlucoseOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.docx";
                string filePath = System.IO.Path.Combine(downloadsPath, fileName);
                document.Save(filePath);

                CustomMessageBox.Show($"Файл сохранен в папке \"Загрузки\": {filePath}", "Успех", MessageWindowImage.Information, MessageWindowButton.Ok);
            }
        }

        private void ButtonExportToWordGlucose_Click(object sender, RoutedEventArgs e)
        {
            if (PeriodComboBox.SelectedValue.ToString() == "Сегодня")
            {
                var currentDate = DateTime.Now.Date;
                var info = DatabaseContext.DbContext.Context.Glucoses.ToList().
                    Where(x => x.Users == _currentUser && x.MeasurementTimeGlucose > currentDate);
                List<Glucoses> glucoses = new List<Glucoses>();
                foreach (var perem in info)
                {
                    glucoses.Add(new Glucoses
                    {
                        MeasurementTimeGlucose = perem.MeasurementTimeGlucose,
                        GlucoseLevel = perem.GlucoseLevel

                    });
                }
                glucoses = glucoses.OrderBy(l => l.MeasurementTimeGlucose).ToList();
                ExportToWordGlucose(glucoses);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                
                var startDate = DateTime.Now.Date.AddDays(-6);

                
                var endDate = DateTime.Now;

                var info = DatabaseContext.DbContext.Context.Glucoses.ToList()
                    .Where(x => x.Users == _currentUser && x.MeasurementTimeGlucose >= startDate && x.MeasurementTimeGlucose <= endDate);

                List<Glucoses> glucoses = new List<Glucoses>();
                foreach (var perem in info)
                {
                    glucoses.Add(new Glucoses
                    {
                        MeasurementTimeGlucose = perem.MeasurementTimeGlucose,
                        GlucoseLevel = perem.GlucoseLevel

                    });
                }
                glucoses = glucoses.OrderBy(l => l.MeasurementTimeGlucose).ToList();
                ExportToWordGlucose(glucoses);
            }

        }


        public void SaveGlucose()
        {

            DateTime today = DateTime.Today;
            var newGlucoseTime = GlucoseTimePicker.Value;

            decimal glucoseConsumed;
            if (!decimal.TryParse(GlucoseTextBox.Text, out glucoseConsumed))
            {
                CustomMessageBox.Show("Некорректное значение для глюкозы", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (newGlucoseTime == default)
            {
                CustomMessageBox.Show("Пожалуйста, заполните временной интервал", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (newGlucoseTime.Value.Date > today)
            {
                CustomMessageBox.Show("Нельзя вводить данные на будущие даты", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (Convert.ToDecimal(GlucoseTextBox.Text) > 40 || Convert.ToDecimal(GlucoseTextBox.Text) < 1)
            {
                CustomMessageBox.Show("Человек не может иметь такой уровень сахара", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            DatabaseContext.DbContext.Context.Glucoses.Add(new Glucoses
            {
                UserID = _currentUser.ID,
                GlucoseLevel = Convert.ToDecimal(GlucoseTextBox.Text),
                MeasurementTimeGlucose = GlucoseTimePicker.Value,
            });

            DatabaseContext.DbContext.Context.SaveChanges();

            GlucoseTimePicker.Text = "";
            ClearField.ClearTextBoxes(this);

        }

        private void ButtonSaveGlucose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveGlucose();
                ChartUpdateGlucose();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageWindowImage.Error, MessageWindowButton.Ok);
            }
        }
    }
}
