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
    
    public partial class Borrowing
    {
        public int BorrowID { get; set; }
        public int UserID { get; set; }
        public int BookID { get; set; }
        public System.DateTime BorrowDate { get; set; }
        public System.DateTime DueDate { get; set; }
        public string Status { get; set; }
    
        public virtual Books Books { get; set; }
        public virtual Users Users { get; set; }
    }
}
