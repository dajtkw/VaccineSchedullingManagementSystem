// Factories/NotificationFactory.cs
using QuanLyTiemChung.Web.Models;
using System;

public static class NotificationFactory
{
    // Thông báo LỊCH HẸN được XÁC NHẬN
    public static Notification CreateAppointmentConfirmedNotification(Appointment appointment)
    {
        return new Notification
        {
            UserId = appointment.UserId,
            Message = $"Lịch hẹn tiêm vắc-xin '{appointment.Vaccine.TradeName}' của bạn vào lúc {appointment.ScheduledDateTime:HH:mm dd/MM/yyyy} đã được xác nhận.",
            NotificationType = "AppointmentConfirmed",
            Url = "/Appointment/Index"
        };
    }

    // Thông báo LỊCH HẸN đã bị HỦY
    public static Notification CreateAppointmentCancelledNotification(Appointment appointment)
    {
        return new Notification
        {
            UserId = appointment.UserId,
            Message = $"Rất tiếc, lịch hẹn tiêm vắc-xin '{appointment.Vaccine.TradeName}' của bạn vào lúc {appointment.ScheduledDateTime:HH:mm dd/MM/yyyy} đã bị hủy.",
            NotificationType = "AppointmentCancelled",
            Url = "/Appointment/Index"
        };
    }

    // Thông báo đã HOÀN THÀNH tiêm chủng
    public static Notification CreateVaccinationCompletedNotification(Appointment appointment, VaccinationRecord record)
    {
        return new Notification
        {
            UserId = appointment.UserId,
            Message = $"Bạn đã hoàn thành tiêm mũi {appointment.DoseNumber} vắc-xin '{appointment.Vaccine.TradeName}' vào lúc {record.ActualVaccinationTime:HH:mm dd/MM/yyyy}.",
            NotificationType = "VaccinationCompleted",
            // URL trỏ thẳng đến trang chứng nhận
            Url = $"/Appointment/VaccinationCertificate?appointmentId={appointment.Id}"
        };
    }

    // Thông báo NHẮC LỊCH hẹn
    public static Notification CreateAppointmentReminderNotification(Appointment appointment)
    {
        return new Notification
        {
            UserId = appointment.UserId,
            Message = $"Nhắc lịch: Bạn có lịch hẹn tiêm vắc-xin '{appointment.Vaccine.TradeName}' vào lúc {appointment.ScheduledDateTime:HH:mm dd/MM/yyyy}.",
            NotificationType = "AppointmentReminder",
            Url = "/Appointment/Index"
        };
    }

    public static Notification CreateNextDoseAvailableNotification(User user, Vaccine vaccine, int nextDoseNumber)
    {
        return new Notification
        {
            UserId = user.Id,
            Message = $"Bạn đã đủ điều kiện để đăng ký tiêm mũi {nextDoseNumber} của vắc-xin '{vaccine.TradeName}'. Hãy đặt lịch ngay!",
            NotificationType = "NextDoseAvailable",
            Url = "/Appointment/Create" // Điều hướng đến trang đăng ký
        };
    }

    public static Notification CreateNewAppointmentForStaffNotification(User staff, User citizen, Vaccine vaccine)
    {
        return new Notification
        {
            UserId = staff.Id,
            Message = $"Có lịch hẹn mới từ '{citizen.FullName}' cho vắc-xin '{vaccine.TradeName}'.",
            NotificationType = "NewAppointment",
            Url = "/MedicalStaff/Index" // Điều hướng đến trang quản lý lịch hẹn
        };
    }

}