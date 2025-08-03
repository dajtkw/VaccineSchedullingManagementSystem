using QuanLyTiemChung.Web.Models;

namespace QuanLyTiemChung.Web.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
    Task MarkAllAsReadAsync(int userId);
    Task MarkAsReadAsync(long notificationId);
}