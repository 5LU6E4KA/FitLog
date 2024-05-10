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
using System.Numerics;

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
            DataContext = this;
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
            string fileName = $"GlucoseOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx"; // Добавляем временный штамп к имени файла
            string filePath = System.IO.Path.Combine(downloadsPath, fileName);

            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("GlucoseData");

                // Добавляем заголовки
                worksheet.Cells[1, 1].Value = "Момент занесения";
                worksheet.Cells[1, 2].Value = "Уровеь глюкозы (ммоль/л)";

                if (glucoses.Any()) // Проверяем, есть ли данные
                {
                    // Добавляем данные
                    int row = 2;
                    foreach (var glucose in glucoses)
                    {
                        worksheet.Cells[row, 1].Value = glucose.MeasurementTimeGlucose;
                        worksheet.Cells[row, 2].Value = glucose.GlucoseLevel;

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

                ExportToExcelGlucose(glucoses);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                // Начальная дата - 6 дней назад от текущего дня
                var startDate = DateTime.Now.Date.AddDays(-6);

                // Конечная дата - текущий момент
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

                ExportToExcelGlucose(glucoses);
            }

        }

        public static void ExportToWordGlucose(List<Glucoses> glucoses)
        {
            // Создаем новый документ Word
            using (WordDocument document = new WordDocument())
            {
                // Добавляем раздел с заголовком
                WSection section = document.AddSection() as WSection;
                WParagraph paragraph = section.HeadersFooters.Header.AddParagraph() as WParagraph;
                paragraph.AppendText("Glucose Data").CharacterFormat.FontSize = 14;
                paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;

                // Добавляем таблицу
                WTable table = section.AddTable() as WTable;
                table.ResetCells(glucoses.Count + 1, 2); // +1 для заголовков

                // Добавляем заголовки таблицы
                string[] headers = { "Момент занесения", "Температура" };
                for (int i = 0; i < headers.Length; i++)
                {
                    table[0, i].AddParagraph().AppendText(headers[i]);
                    table[0, i].CellFormat.VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment.Middle;
                    //table[0, i].CellFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;
                }

                // Добавляем данные в таблицу
                for (int i = 0; i < glucoses.Count; i++)
                {
                    Glucoses glucose = glucoses[i];
                    table[i + 1, 0].AddParagraph().AppendText(glucose.MeasurementTimeGlucose.ToString());
                    table[i + 1, 1].AddParagraph().AppendText(glucose.GlucoseLevel.ToString());
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
                string fileName = $"GlucoseOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.docx";
                string filePath = System.IO.Path.Combine(downloadsPath, fileName);
                document.Save(filePath);

                MessageBox.Show($"Файл сохранен в папке \"Загрузки\": {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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

                ExportToWordGlucose(glucoses);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                // Начальная дата - 6 дней назад от текущего дня
                var startDate = DateTime.Now.Date.AddDays(-6);

                // Конечная дата - текущий момент
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

                ExportToWordGlucose(glucoses);
            }

        }


        public void SaveGlucose()
        {

            if (string.IsNullOrWhiteSpace(GlucoseTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите температуру тела");
                return;
            }

            if (Convert.ToDecimal(GlucoseTextBox.Text) > 40 || Convert.ToDecimal(GlucoseTextBox.Text) < 1)
            {
                MessageBox.Show("Человек не может иметь такую глюкозу");
                return;
            }

            DatabaseContext.DbContext.Context.Glucoses.Add(new Glucoses
            {
                UserID = _currentUser.ID,
                GlucoseLevel = Convert.ToDecimal(GlucoseTextBox.Text),
                MeasurementTimeGlucose = GlucoseTimePicker.Value,
            });

            DatabaseContext.DbContext.Context.SaveChanges();

            //ClearField.ClearTextBoxes(this);

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
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}");
            }
        }
    }
}
