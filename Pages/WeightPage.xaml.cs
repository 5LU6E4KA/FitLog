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
            string fileName = $"WeightOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx"; 
            string filePath = System.IO.Path.Combine(downloadsPath, fileName);

            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("WeightData");

                
                worksheet.Cells[1, 1].Value = "Момент занесения";
                worksheet.Cells[1, 2].Value = "Вес (кг)";

                if (weights.Any()) 
                {
                    
                    int row = 2;
                    foreach (var perem in weights)
                    {
                        worksheet.Cells[row, 1].Value = perem.MeasurementTimeWeight;
                        worksheet.Cells[row, 2].Value = perem.BodyWeight;

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
                weights = weights.OrderBy(l => l.MeasurementTimeWeight).ToList();
                ExportToExcelWeight(weights);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                
                var startDate = DateTime.Now.Date.AddDays(-6);

                
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
                weights = weights.OrderBy(l => l.MeasurementTimeWeight).ToList();
                ExportToExcelWeight(weights);
            }

        }

        public static void ExportToWordWeight(List<Weights> weights)
        {
            
            using (WordDocument document = new WordDocument())
            {
                
                WSection section = document.AddSection() as WSection;
                WParagraph paragraph = section.HeadersFooters.Header.AddParagraph() as WParagraph;
                paragraph.AppendText("Weight Data").CharacterFormat.FontSize = 14;
                paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;

                
                WTable table = section.AddTable() as WTable;
                table.ResetCells(weights.Count + 1, 2); 

                
                string[] headers = { "Момент занесения", "Температура" };
                for (int i = 0; i < headers.Length; i++)
                {
                    table[0, i].AddParagraph().AppendText(headers[i]);
                    table[0, i].CellFormat.VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment.Middle;
                    
                }

                
                for (int i = 0; i < weights.Count; i++)
                {
                    Weights weight = weights[i];
                    table[i + 1, 0].AddParagraph().AppendText(weight.MeasurementTimeWeight.ToString());
                    table[i + 1, 1].AddParagraph().AppendText(weight.BodyWeight.ToString());
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
                string fileName = $"WeightOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.docx";
                string filePath = System.IO.Path.Combine(downloadsPath, fileName);
                document.Save(filePath);

                CustomMessageBox.Show($"Файл сохранен в папке \"Загрузки\": {filePath}", "Успех", MessageWindowImage.Information, MessageWindowButton.Ok);
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
                weights = weights.OrderBy(l => l.MeasurementTimeWeight).ToList();
                ExportToWordWeight(weights);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                
                var startDate = DateTime.Now.Date.AddDays(-6);

                
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
                weights = weights.OrderBy(l => l.MeasurementTimeWeight).ToList();
                ExportToWordWeight(weights);
            }

        }

        public void SaveWeight()
        {
            var weightGoal = _currentUser.WeightGoal;
            DateTime today = DateTime.Today;

            DateTime? newWeightTime = WeightTimePicker.Value;
            decimal weightConsumed;

            if (!decimal.TryParse(WeightTextBox.Text, out weightConsumed))
            {
                CustomMessageBox.Show("Некорректное значение для веса", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (newWeightTime == null)
            {
                CustomMessageBox.Show("Пожалуйста, заполните временной интервал", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (weightConsumed > 650 || weightConsumed < 10)
            {
                CustomMessageBox.Show("Человек не может иметь такой вес", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (newWeightTime.Value.Date > today)
            {
                CustomMessageBox.Show("Нельзя вводить данные на будущие даты", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            DatabaseContext.DbContext.Context.Weights.Add(new Weights
            {
                UserID = _currentUser.ID,
                BodyWeight = weightConsumed,
                MeasurementTimeWeight = newWeightTime.Value,
            });

            DatabaseContext.DbContext.Context.SaveChanges();

            WeightTimePicker.Text = "";
            ClearField.ClearTextBoxes(this);

            if (newWeightTime.Value.Date == today)
            {
                if (weightConsumed == weightGoal)
                {
                    ShowNotification("Вес", "Вы достигли своей цели по весу за сегодня!");
                }
                else if (weightConsumed > weightGoal)
                {
                    ShowNotification("Вес", "Вы превысили свою цель по весу за сегодня!");
                }
                else
                {
                    ShowNotification("Вес", "Продолжайте работу над своей целью по весу!");
                }
            }
        }



        private void OpenFirstLink(object sender, RoutedEventArgs e)
        {
            string url = "https://vkusvill.ru/media/journal/osnovnye-printsipy-pravilnogo-nabora-vesa.html";

            Process.Start(url);
        }

        private void OpenSecondLink(object sender, RoutedEventArgs e)
        {
            string url = "https://vkusvill.ru/media/journal/osnovnye-printsipy-pravilnogo-pokhudeniya-chto-nuzhno-est-i-chego-opasatsya.html";

            Process.Start(url);
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
                CustomMessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageWindowImage.Error, MessageWindowButton.Ok);
            }
        }
    }
}
