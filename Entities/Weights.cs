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
    
    public partial class Weights
    {
        public int ID { get; set; }
        public Nullable<int> UserID { get; set; }
        public decimal BodyWeight { get; set; }
        public Nullable<System.DateTime> MeasurementTimeWeight { get; set; }
    
        public virtual Users Users { get; set; }
    }
}
