namespace QuanLyTiemChung.Web.ViewModels
{
    public class InventoryViewModel
    {
        public int VaccineId { get; set; }
        public string VaccineName { get; set; } = string.Empty;
                public string GenericName { get; set; } = string.Empty; 
        public int Quantity { get; set; }
    }
}