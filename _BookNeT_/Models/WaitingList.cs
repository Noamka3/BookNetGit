//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace _BookNeT_.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class WaitingList
    {
        public int WaitingID { get; set; }
        public int BookID { get; set; }
        public int UserID { get; set; }
        public int Position { get; set; }
        public Nullable<bool> NotificationSent { get; set; }
    
        public virtual Books Books { get; set; }
        public virtual Users Users { get; set; }
    }
}