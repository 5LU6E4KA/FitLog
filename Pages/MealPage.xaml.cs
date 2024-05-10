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
using System.Windows;
using System.Windows.Controls;
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
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}");
            }
        }

        public void SaveMeal()
        {
            decimal caloriesConsumed = Convert.ToInt32(CarloriesGainedTextBox.Text);
            DateTime today = DateTime.Today;


            if (string.IsNullOrWhiteSpace(CarloriesGainedTextBox.Text) || string.IsNullOrWhiteSpace(FoodProductTextBox.Text))
            {
                MessageBox.Show("Вы забыли указать количество калорий или съеденное блюдо");
                return;
            }

            if (Convert.ToDecimal(CarloriesGainedTextBox.Text) > 9999)
            {
                MessageBox.Show("Ограничение в 9999 килокалорий");
                return;
            }

            //if (new string[] { SugarTextBox.Text, CarbohydratesTextBox.Text, FatsTextBox.Text, ProteinTextBox.Text }.Any(x => CheckDecimal(x)))
            //{
            //    MessageBox.Show("Ограничение в 999 грамм");
            //    return;
            //}

            DatabaseContext.DbContext.Context.Meals.Add(new Meals
            {
                UserID = _currentUser.ID,
                AmountOfCalories = Convert.ToDecimal(CarloriesGainedTextBox.Text),
                Intake = EatingComboBox.SelectedItem?.ToString(),
                FoodProduct = FoodProductTextBox.Text,
                MealTime = FoodTimeDateTimePicker.Value
            });

            DatabaseContext.DbContext.Context.SaveChanges();

            var mealsForToday = DatabaseContext.DbContext.Context.Meals
            .Where(m => m.UserID == _currentUser.ID &&
                 DbFunctions.TruncateTime(m.MealTime) == today)
            .ToList();


            // Посчитать сумму калорий
            decimal totalCaloriesForToday = mealsForToday.Sum(m => m.AmountOfCalories);


            // Сравнение с целью пользователя
            if (totalCaloriesForToday == _currentUser.FoodGoal)
            {
                ShowNotification("Питание", "Вы достигли свою цель по калориям за сегодня!");
                // Здесь можно выполнить дополнительные действия в случае достижения цели
            }

            if (totalCaloriesForToday > _currentUser.FoodGoal)
            {
                ShowNotification("Питание", "Вы превысили свою цель по калориям за сегодня!");
                // Здесь можно выполнить дополнительные действия в случае достижения цели
            }

            //ClearField.ClearTextBoxes(this);

            EatingComboBox.SelectedIndex = 0;
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
            string fileName = $"MealOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx"; // Добавляем временный штамп к имени файла
            string filePath = System.IO.Path.Combine(downloadsPath, fileName);

            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("MealData");

                // Добавляем заголовки
                worksheet.Cells[1, 1].Value = "Время измерения";
                worksheet.Cells[1, 2].Value = "Продукт питания";
                worksheet.Cells[1, 3].Value = "Прием пищи";
                worksheet.Cells[1, 4].Value = "Количество килокалорий";

                if (meals.Any()) // Проверяем, есть ли данные
                {
                    // Добавляем данные
                    int row = 2;
                    foreach (var meal in meals)
                    {
                        worksheet.Cells[row, 1].Value = meal.MealTime;
                        worksheet.Cells[row, 2].Value = meal.FoodProduct;
                        worksheet.Cells[row, 3].Value = meal.Intake;
                        worksheet.Cells[row, 4].Value = meal.AmountOfCalories;
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

                ExportToExcelMeal(meals);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                // Начальная дата - 6 дней назад от текущего дня
                var startDate = DateTime.Now.Date.AddDays(-6);

                // Конечная дата - текущий момент
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

                ExportToExcelMeal(meals);
            }

        }

        public static void ExportToWordMeal(List<Meals> meals)
        {
            // Создаем новый документ Word
            using (WordDocument document = new WordDocument())
            {
                // Добавляем раздел с заголовком
                WSection section = document.AddSection() as WSection;
                WParagraph paragraph = section.HeadersFooters.Header.AddParagraph() as WParagraph;
                paragraph.AppendText("Meal Data").CharacterFormat.FontSize = 14;
                paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;

                // Добавляем таблицу
                WTable table = section.AddTable() as WTable;
                table.ResetCells(meals.Count + 1, 4); // +1 для заголовков

                // Добавляем заголовки таблицы
                string[] headers = { "Момент измерения", "Продукт питания", "Прием пищи", "Калорийность (Ккал)" };
                for (int i = 0; i < headers.Length; i++)
                {
                    table[0, i].AddParagraph().AppendText(headers[i]);
                    table[0, i].CellFormat.VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment.Middle;
                    //table[0, i].CellFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;
                }

                // Добавляем данные в таблицу
                for (int i = 0; i < meals.Count; i++)
                {
                    Meals meal = meals[i];
                    table[i + 1, 0].AddParagraph().AppendText(meal.MealTime.ToString());
                    table[i + 1, 1].AddParagraph().AppendText(meal.FoodProduct);
                    table[i + 1, 2].AddParagraph().AppendText(meal.Intake);
                    table[i + 1, 3].AddParagraph().AppendText(meal.AmountOfCalories.ToString());
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
                string fileName = $"MealOutput_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.docx";
                string filePath = System.IO.Path.Combine(downloadsPath, fileName);
                document.Save(filePath);

                MessageBox.Show($"Файл сохранен в папке \"Загрузки\": {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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

                ExportToWordMeal(meals);
            }
            else if (PeriodComboBox.SelectedValue.ToString() == "Неделя")
            {
                // Начальная дата - 6 дней назад от текущего дня
                var startDate = DateTime.Now.Date.AddDays(-6);

                // Конечная дата - текущий момент
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

                ExportToWordMeal(meals);
            }
        }
    }
}
