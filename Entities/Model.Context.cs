﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class FitLogEntities : DbContext
    {
        public FitLogEntities()
            : base("name=FitLogEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Glucoses> Glucoses { get; set; }
        public virtual DbSet<Liquids> Liquids { get; set; }
        public virtual DbSet<Meals> Meals { get; set; }
        public virtual DbSet<Pulses> Pulses { get; set; }
        public virtual DbSet<Sleeps> Sleeps { get; set; }
        public virtual DbSet<Temperatures> Temperatures { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Weights> Weights { get; set; }
    }
}
