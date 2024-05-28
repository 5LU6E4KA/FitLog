using FitLog.ClearFields;
using FitLog.Controls;
using FitLog.Entities;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Syncfusion.DocIO.DLS;
using Syncfusion.UI.Xaml.Charts;
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
    public class Calorie
    {
        public string Day { get; set; }
        public double Calories { get; set; }
    }

    public partial class MealPage : Page
    {
        private Users _currentUser;
        public List<string> Eating { get; set; }
        public List<string> Period { get; set; }
        public List<string> DayOfWeeks { get; set; }
        private DateTime _currentMealChartDate = DateTime.Now;

        public MealPage(Users user)
        {
            _currentUser = user;
            InitializeComponent();
            Eating = eating;
            Period = period;
            PeriodComboBox.SelectedIndex = 0;
            EatingComboBox.SelectedIndex = 0;
            ChartUpdateMeal(DateTime.Now);
            DataContext = this;
        }

        private List<string> eating = new List<string>
        {
            "Не указано", "Завтрак", "Обед", "Ужин", "Перекус"
        };
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

        private void ChartUpdateMeal(DateTime dateTime)
        {
            _currentMealChartDate = dateTime;
            StartDateMealTextBlock.Text = GetFirstDateOfWeek(dateTime, DayOfWeek.Monday).ToString("dd.MM.yyyy");
            FinishDateMealTextBlock.Text = GetFirstDateOfWeek(dateTime.AddDays(7), DayOfWeek.Monday).
                AddDays(-1).ToString("dd.MM.yyyy");
            var info = DatabaseContext.DbContext.Context.Meals.ToList().
                Where(x => x.Users == _currentUser && x.MealTime > GetFirstDateOfWeek(dateTime, DayOfWeek.Monday) &&
                x.MealTime < GetFirstDateOfWeek(dateTime.AddDays(7), DayOfWeek.Monday));

            List<Calorie> calories = new List<Calorie>();
            foreach (var item in dayOfWeeks)
            {
                var calorie = info.Where(x => x.MealTime.Value.DayOfWeek == item.Key);
                calories.Add(new Calorie { Day = item.Value, Calories = calorie.Count() == 0 ? 0 : (double)calorie.Sum(x => x.AmountOfCalories) });
            }
            ColGraficCalroies.ItemsSource = calories;
        }

        private void ButtonSaveCalories_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveMeal();
                ChartUpdateMeal(DateTime.Now);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageWindowImage.Error, MessageWindowButton.Ok);
            }
        }

        public void SaveMeal()
        {
            decimal caloriesConsumed;
            if (!decimal.TryParse(CarloriesGainedTextBox.Text, out caloriesConsumed))
            {
                CustomMessageBox.Show("Некорректное значение для калорийности", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            DateTime today = DateTime.Today;
            DateTime? newMealTime = FoodTimeDateTimePicker.Value;

            if (newMealTime == null)
            {
                CustomMessageBox.Show("Пожалуйста, заполните временной интервал", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (newMealTime.Value.Date > today)
            {
                CustomMessageBox.Show("Нельзя вводить данные на будущие даты", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (string.IsNullOrWhiteSpace(FoodProductTextBox.Text))
            {
                CustomMessageBox.Show("Вы забыли указать съеденное блюдо", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            if (caloriesConsumed > 9999 || caloriesConsumed < 0)
            {
                CustomMessageBox.Show("Ограничение от 0 до 9999 килокалорий", "Внимание", MessageWindowImage.Warning, MessageWindowButton.Ok);
                return;
            }

            DatabaseContext.DbContext.Context.Meals.Add(new Meals
            {
                UserID = _currentUser.ID,
                AmountOfCalories = caloriesConsumed,
                Intake = EatingComboBox.SelectedItem?.ToString(),
                FoodProduct = FoodProductTextBox.Text,
                MealTime = newMealTime.Value
            });

            DatabaseContext.DbContext.Context.SaveChanges();

            FoodTimeDateTimePicker.Text = "";
            ClearField.ClearTextBoxes(this);

            if (newMealTime.Value.Date == today)
            {
                var mealsForToday = DatabaseContext.DbContext.Context.Meals
                    .Where(m => m.UserID == _currentUser.ID &&
                                DbFunctions.TruncateTime(m.MealTime) == today)
                    .ToList();

                decimal totalCaloriesForToday = mealsForToday.Sum(m => m.AmountOfCalories);

                if (totalCaloriesForToday == _currentUser.FoodGoal)
                {
                    ShowNotification("Питание", "Вы достигли свою цель по калориям за сегодня!");
                }
                else if (totalCaloriesForToday > _currentUser.FoodGoal)
                {
                    ShowNotification("Питание", "Вы превысили свою цель по калориям за сегодня!");
                }
                else
                {
                    ShowNotification("Питание", "Продолжайте работать над своей целью!");
                }
            }

            EatingComboBox.SelectedIndex = 0;
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

        private void LetterOnlyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //Проверка на буквенные символы
            Regex regex = new Regex("^[a-zA-Zа-яА-Я]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void ButtonLeftCalories_Click(object sender, RoutedEventArgs e)
        {
            _currentMealChartDate = _currentMealChartDate.AddDays(-7);
            ChartUpdateMeal(_currentMealChartDate);
        }

        private void ButtonRightCalories_Click(object sender, RoutedEventArgs e)
        {
            _currentMealChartDate = _currentMealChartDate.AddDays(7);
            ChartUpdateMeal(_currentMealChartDate);
        }

        private void ButtonThisWeek_Click(object sender, RoutedEventArgs e)
        {
            _currentMealChartDate = DateTime.Now;
            ChartUpdateMeal(_currentMealChartDate);
        }

        private void OpenBasicsHealthyDiet(object sender, RoutedEventArgs e)
        {
            string url = "https://14.rospotrebnadzor.ru/content/2090/79455/";

            Process.Start(url);
        }

        private void OpenHealthySnacks(object sender, RoutedEventArgs e)
        {
            string url = "https://cgie.62.rospotrebnadzor.ru/content/1408/95790/";

            Process.Start(url);
        }

        private void OpenRecipes(object sender, RoutedEventArgs e)
        {
            string url = "https://yummybook.ru/category/zdorovoe-pitanie";

            Process.Start(url);
        }

        public static void ExportToExcelMeal(List<Meals> meals)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            string fileName = $"MealOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx"; 
            string filePath = System.IO.Path.Combine(downloadsPath, fileName);

            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("MealData");

                
                worksheet.Cells[1, 1].Value = "Время измерения";
                worksheet.Cells[1, 2].Value = "Продукт питания";
                worksheet.Cells[1, 3].Value = "Прием пищи";
                worksheet.Cells[1, 4].Value = "Количество килокалорий";

                if (meals.Any()) 
                {
                    
                    int row = 2;
                    foreach (var meal in meals)
                    {
                        worksheet.Cells[row, 1].Value = meal.MealTime;
                        worksheet.Cells[row, 2].Value = meal.FoodProduct;
                        worksheet.Cells[row, 3].Value = meal.Intake;
                        worksheet.Cells[row, 4].Value = meal.AmountOfCalories;
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

        private void ButtonExportToExcelMeal_Click(object sender, RoutedEventArgs e)
        {
            if (PeriodComboBox.SelectedValue.ToString() == "Сегодня")
            {
                var currentDate = DateTime.Now.Date;
                var info = DatabaseContext.DbContext.Context.Meals.ToList().
                    Where(x => x.Users == _currentUser && x.MealTime > currentDate);
                List<Meals> meals = new List<Meals>();
                foreach (var pulseMeasurement in info)
                {
                    meals.Add(new Meals
                    {
                        MealTime = pulseMeasurement.MealTime.Value,
                        FoodProduct = pulseMeasurement.FoodProduct,
                        Intake = pulseMeasurement.Intake,
                        AmountOfCalories = pulseMeasurement.AmountOfCalories
                    });
                }
                meals = meals.OrderBy(l => l.MealTime).ToList();
                ExportToExcelMeal(meals);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                
                var startDate = DateTime.Now.Date.AddDays(-6);

                
                var endDate = DateTime.Now;

                var info = DatabaseContext.DbContext.Context.Meals.ToList()
                    .Where(x => x.Users == _currentUser && x.MealTime >= startDate && x.MealTime <= endDate);

                List<Meals> meals = new List<Meals>();
                foreach (var meal in info)
                {
                    meals.Add(new Meals
                    {
                        MealTime = meal.MealTime.Value,
                        FoodProduct = meal.FoodProduct,
                        Intake = meal.Intake,
                        AmountOfCalories = meal.AmountOfCalories
                    });
                }
                meals = meals.OrderBy(l => l.MealTime).ToList();
                ExportToExcelMeal(meals);
            }

        }

        public static void ExportToWordMeal(List<Meals> meals)
        {
            
            using (WordDocument document = new WordDocument())
            {
                
                WSection section = document.AddSection() as WSection;
                WParagraph paragraph = section.HeadersFooters.Header.AddParagraph() as WParagraph;
                paragraph.AppendText("Meal Data").CharacterFormat.FontSize = 14;
                paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;

                
                WTable table = section.AddTable() as WTable;
                table.ResetCells(meals.Count + 1, 4); 

                // Добавляем заголовки таблицы
                string[] headers = { "Момент измерения", "Продукт питания", "Прием пищи", "Калорийность (Ккал)" };
                for (int i = 0; i < headers.Length; i++)
                {
                    table[0, i].AddParagraph().AppendText(headers[i]);
                    table[0, i].CellFormat.VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment.Middle;
                    
                }

                
                for (int i = 0; i < meals.Count; i++)
                {
                    Meals meal = meals[i];
                    table[i + 1, 0].AddParagraph().AppendText(meal.MealTime.ToString());
                    table[i + 1, 1].AddParagraph().AppendText(meal.FoodProduct);
                    table[i + 1, 2].AddParagraph().AppendText(meal.Intake);
                    table[i + 1, 3].AddParagraph().AppendText(meal.AmountOfCalories.ToString());
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
                string fileName = $"MealOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.docx";
                string filePath = System.IO.Path.Combine(downloadsPath, fileName);
                document.Save(filePath);

                CustomMessageBox.Show($"Файл сохранен в папке \"Загрузки\": {filePath}", "Успех", MessageWindowImage.Information, MessageWindowButton.Ok);
            }
        }

        private void ButtonExportToWordMeal_Click(object sender, RoutedEventArgs e)
        {
            if (PeriodComboBox.SelectedValue.ToString() == "Сегодня")
            {
                var currentDate = DateTime.Now.Date;
                var info = DatabaseContext.DbContext.Context.Meals.ToList().
                    Where(x => x.Users == _currentUser && x.MealTime > currentDate);
                List<Meals> meals = new List<Meals>();
                foreach (var pulseMeasurement in info)
                {
                    meals.Add(new Meals
                    {
                        MealTime = pulseMeasurement.MealTime.Value,
                        FoodProduct = pulseMeasurement.FoodProduct,
                        Intake = pulseMeasurement.Intake,
                        AmountOfCalories = pulseMeasurement.AmountOfCalories
                    });
                }
                meals = meals.OrderBy(l => l.MealTime).ToList();
                ExportToWordMeal(meals);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                
                var startDate = DateTime.Now.Date.AddDays(-6);

                
                var endDate = DateTime.Now;

                var info = DatabaseContext.DbContext.Context.Meals.ToList()
                    .Where(x => x.Users == _currentUser && x.MealTime >= startDate && x.MealTime <= endDate);

                List<Meals> meals = new List<Meals>();
                foreach (var meal in info)
                {
                    meals.Add(new Meals
                    {
                        MealTime = meal.MealTime.Value,
                        FoodProduct = meal.FoodProduct,
                        Intake = meal.Intake,
                        AmountOfCalories = meal.AmountOfCalories
                    });
                }
                meals = meals.OrderBy(l => l.MealTime).ToList();
                ExportToWordMeal(meals);
            }
        }

        
    }
}
