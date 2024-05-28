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
            decimal liquidConsumed;
            if (!decimal.TryParse(WaterLevelTextBox.Text, out liquidConsumed))
            {
                CustomMessageBox.Show("Некорректное значение для жидкости", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            DateTime today = DateTime.Today;
            DateTime? newLiquidTime = LiquidTimeDateTimePicker.Value;

            if (newLiquidTime == null)
            {
                CustomMessageBox.Show("Пожалуйста, заполните временной интервал", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (newLiquidTime.Value.Date > today)
            {
                CustomMessageBox.Show("Нельзя вводить данные на будущие даты", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (string.IsNullOrWhiteSpace(WaterTypeTextBox.Text))
            {
                CustomMessageBox.Show("Пожалуйста, введите напиток", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (liquidConsumed > 9999)
            {
                CustomMessageBox.Show("Ограничение в 9999 миллилитров", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            DatabaseContext.DbContext.Context.Liquids.Add(new Liquids
            {
                UserID = _currentUser.ID,
                LiquidLevel = liquidConsumed,
                LiquidType = WaterTypeTextBox.Text,
                DrinkingTime = newLiquidTime.Value
            });

            DatabaseContext.DbContext.Context.SaveChanges();

            LiquidTimeDateTimePicker.Text = "";
            ClearField.ClearTextBoxes(this);

            if (newLiquidTime.Value.Date == today)
            {
                var liquidsForToday = DatabaseContext.DbContext.Context.Liquids
                    .Where(m => m.UserID == _currentUser.ID &&
                                DbFunctions.TruncateTime(m.DrinkingTime) == today)
                    .ToList();

                decimal totalLiquidsForToday = liquidsForToday.Sum(m => m.LiquidLevel);

                if (totalLiquidsForToday == _currentUser.LiquidGoal)
                {
                    ShowNotification("Жидкость", "Вы достигли свою цель по жидкости за сегодня!");
                }
                else if (totalLiquidsForToday > _currentUser.LiquidGoal)
                {
                    ShowNotification("Жидкость", "Вы превысили свою цель по жидкости за сегодня!");
                }
                else
                {
                    ShowNotification("Жидкость", "Продолжайте работать над своей целью!");
                }
            }
        }


        private void LetterOnlyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //Проверка на буквенные символы
            Regex regex = new Regex("^[a-zA-Zа-яА-Я]+$");
            e.Handled = !regex.IsMatch(e.Text);
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

        private void OpenFirstLink(object sender, RoutedEventArgs e)
        {
            string url = "https://medaboutme.ru/articles/kak_podderzhivat_optimalnyy_pitevoy_rezhim/";

            Process.Start(url);
        }

        private void OpenSecondLink(object sender, RoutedEventArgs e)
        {
            string url = "https://profilaktica.ru/for-population/profilaktika-zabolevaniy/vse-o-pravilnom-pitanii/skolko-vody-nuzhno-pit-v-den/";

            Process.Start(url);
        }

        private void OpenThirdLink(object sender, RoutedEventArgs e)
        {
            string url = "https://med-prof.ru/o-tsentre/novosti/zakalivanie-vodoj/";

            Process.Start(url);
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
                CustomMessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageWindowImage.Error, MessageWindowButton.Ok);
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

        public static void ExportToExcelWater(List<Liquids> liquids)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            string fileName = $"LiquidOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx"; 
            string filePath = System.IO.Path.Combine(downloadsPath, fileName);

            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("LiquidData");

                
                worksheet.Cells[1, 1].Value = "Момент измерения";
                worksheet.Cells[1, 2].Value = "Тип жидкости";
                worksheet.Cells[1, 3].Value = "Количество миллилитров";

                if (liquids.Any())
                {
                   
                    int row = 2;
                    foreach (var liquid in liquids)
                    {
                        worksheet.Cells[row, 1].Value = liquid.DrinkingTime;
                        worksheet.Cells[row, 2].Value = liquid.LiquidType;
                        worksheet.Cells[row, 3].Value = liquid.LiquidLevel;

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
                liquids = liquids.OrderBy(l => l.DrinkingTime).ToList();
                ExportToExcelWater(liquids);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                
                var startDate = DateTime.Now.Date.AddDays(-6);

                
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
                liquids = liquids.OrderBy(l => l.DrinkingTime).ToList();

                ExportToExcelWater(liquids);
            }

        }

        public static void ExportToWordMeal(List<Liquids> liquids)
        {
            
            using (WordDocument document = new WordDocument())
            {
                
                WSection section = document.AddSection() as WSection;
                WParagraph paragraph = section.HeadersFooters.Header.AddParagraph() as WParagraph;
                paragraph.AppendText("Liquid Data").CharacterFormat.FontSize = 14;
                paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;

                
                WTable table = section.AddTable() as WTable;
                table.ResetCells(liquids.Count + 1, 3); 

                
                string[] headers = { "Момент измерения", "Тип жидкости", "Количество миллилитров" };
                for (int i = 0; i < headers.Length; i++)
                {
                    table[0, i].AddParagraph().AppendText(headers[i]);
                    table[0, i].CellFormat.VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment.Middle;
                    
                }

                
                for (int i = 0; i < liquids.Count; i++)
                {
                    Liquids liquid = liquids[i];
                    table[i + 1, 0].AddParagraph().AppendText(liquid.DrinkingTime.ToString());
                    table[i + 1, 1].AddParagraph().AppendText(liquid.LiquidType);
                    table[i + 1, 2].AddParagraph().AppendText(liquid.LiquidLevel.ToString());
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
                string fileName = $"LiquidOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.docx";
                string filePath = System.IO.Path.Combine(downloadsPath, fileName);
                document.Save(filePath);

                CustomMessageBox.Show($"Файл сохранен в папке \"Загрузки\": {filePath}", "Успех", MessageWindowImage.Information, MessageWindowButton.Ok);
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
                liquids = liquids.OrderBy(l => l.DrinkingTime).ToList();
                ExportToWordMeal(liquids);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                
                var startDate = DateTime.Now.Date.AddDays(-6);

                
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
                liquids = liquids.OrderBy(l => l.DrinkingTime).ToList();
                ExportToWordMeal(liquids);
            }

        }
    }
}

