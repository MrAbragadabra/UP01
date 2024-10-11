//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MewingLab.DB
{
    using System;
    using System.Collections.Generic;
    
    public partial class patients
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public patients()
        {
            this.orders = new HashSet<orders>();
            this.results = new HashSet<results>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public string second_name { get; set; }
        public string last_name { get; set; }
        public System.DateTime born_date { get; set; }
        public string passport_series { get; set; }
        public string passport_number { get; set; }
        public string email { get; set; }
        public string insurance_number { get; set; }
        public int id_insurance_type { get; set; }
        public int id_insurance_company { get; set; }
    
        public virtual insurance_company insurance_company { get; set; }
        public virtual insurance_type insurance_type { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<orders> orders { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<results> results { get; set; }

        public override string ToString()
        {
            return $"{second_name} {name} {last_name}";
        }
    }
}
