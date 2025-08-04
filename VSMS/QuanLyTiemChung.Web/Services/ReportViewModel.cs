using System.Collections.Generic;

namespace QuanLyTiemChung.Web.ViewModels
{
    // Dữ liệu cho biểu đồ
    public class ChartDataViewModel
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<int> Data { get; set; } = new List<int>();
    }

    // ViewModel chính cho toàn bộ trang báo cáo
    public class ReportViewModel
    {
        public int TotalPending { get; set; }
        public int TotalConfirmed { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalCancelled { get; set; }
        public ChartDataViewModel AppointmentsByDay { get; set; }
        public ChartDataViewModel TopVaccines { get; set; }
    }
}