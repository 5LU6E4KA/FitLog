//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FitLog.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Meals
    {
        public int ID { get; set; }
        public Nullable<int> UserID { get; set; }
        public decimal AmountOfCalories { get; set; }
        public string FoodProduct { get; set; }
        public string Intake { get; set; }
        public Nullable<System.DateTime> MealTime { get; set; }
    
        public virtual Users Users { get; set; }
    }
}
