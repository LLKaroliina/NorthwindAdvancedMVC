//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NorthwindAdvancedMVC.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProductsDailySales
    {
        public long rowid { get; set; }
        public Nullable<System.DateTime> OrderDate { get; set; }
        public string ProductName { get; set; }
        public Nullable<double> DailySales { get; set; }
    }
}
