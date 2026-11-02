using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyPlanner.Infrastructure.Migrations;

public partial class RenameBusinessFieldsToVietnamese : Migration
{
    // Chi doi ten cot: PostgreSQL giu gia tri, PK/FK, CHECK va index hien co.
    private static readonly (string Table, string Before, string After)[] Columns =
    [
        ("class_sessions", "Id", "ma"),
        ("class_sessions", "CourseId", "ma_mon_hoc"),
        ("class_sessions", "DayOfWeek", "thu_trong_tuan"),
        ("class_sessions", "StartTime", "gio_bat_dau"),
        ("class_sessions", "EndTime", "gio_ket_thuc"),
        ("class_sessions", "Room", "phong_hoc"),
        ("class_sessions", "CreatedAt", "ngay_tao"),
        ("class_sessions", "UpdatedAt", "ngay_cap_nhat"),
        ("cohorts", "Id", "ma"),
        ("cohorts", "Name", "ten"),
        ("cohorts", "StartYear", "nam_bat_dau"),
        ("cohorts", "EndYear", "nam_ket_thuc"),
        ("cohorts", "CreatedAt", "ngay_tao"),
        ("cohorts", "UpdatedAt", "ngay_cap_nhat"),
        ("courses", "Id", "ma"),
        ("courses", "SemesterId", "ma_hoc_ky"),
        ("courses", "Code", "ky_hieu"),
        ("courses", "Name", "ten"),
        ("courses", "Credits", "so_tin_chi"),
        ("courses", "Lecturer", "giang_vien"),
        ("courses", "Color", "mau_sac"),
        ("courses", "CreatedAt", "ngay_tao"),
        ("courses", "UpdatedAt", "ngay_cap_nhat"),
        ("faculties", "Id", "ma"),
        ("faculties", "SchoolId", "ma_truong"),
        ("faculties", "Code", "ky_hieu"),
        ("faculties", "Name", "ten"),
        ("faculties", "CreatedAt", "ngay_tao"),
        ("faculties", "UpdatedAt", "ngay_cap_nhat"),
        ("notifications", "Id", "ma"),
        ("notifications", "UserId", "ma_nguoi_dung"),
        ("notifications", "TaskId", "ma_nhiem_vu"),
        ("notifications", "Type", "loai"),
        ("notifications", "Content", "noi_dung"),
        ("notifications", "RemindAt", "thoi_diem_nhac"),
        ("notifications", "DeliveryStatus", "trang_thai_phat"),
        ("notifications", "SentAt", "ngay_phat"),
        ("notifications", "ReadAt", "ngay_doc"),
        ("notifications", "AttemptCount", "so_lan_thu"),
        ("notifications", "NextAttemptAt", "lan_thu_tiep_theo_luc"),
        ("notifications", "CreatedAt", "ngay_tao"),
        ("notifications", "UpdatedAt", "ngay_cap_nhat"),
        ("schools", "Id", "ma"),
        ("schools", "Code", "ky_hieu"),
        ("schools", "Name", "ten"),
        ("schools", "CreatedAt", "ngay_tao"),
        ("schools", "UpdatedAt", "ngay_cap_nhat"),
        ("semesters", "Id", "ma"),
        ("semesters", "UserId", "ma_nguoi_dung"),
        ("semesters", "Name", "ten"),
        ("semesters", "StartDate", "ngay_bat_dau"),
        ("semesters", "EndDate", "ngay_ket_thuc"),
        ("semesters", "CreatedAt", "ngay_tao"),
        ("semesters", "UpdatedAt", "ngay_cap_nhat"),
        ("student_classes", "Id", "ma"),
        ("student_classes", "FacultyId", "ma_khoa"),
        ("student_classes", "CohortId", "ma_khoa_hoc"),
        ("student_classes", "Name", "ten"),
        ("student_classes", "CreatedAt", "ngay_tao"),
        ("student_classes", "UpdatedAt", "ngay_cap_nhat"),
        ("task_histories", "Id", "ma"),
        ("task_histories", "TaskId", "ma_nhiem_vu"),
        ("task_histories", "UserId", "ma_nguoi_dung"),
        ("task_histories", "Action", "hanh_dong"),
        ("task_histories", "TaskTitle", "tieu_de_nhiem_vu"),
        ("task_histories", "OldData", "du_lieu_truoc"),
        ("task_histories", "NewData", "du_lieu_sau"),
        ("task_histories", "ChangedAt", "ngay_thay_doi"),
        ("task_items", "Id", "ma"),
        ("task_items", "UserId", "ma_nguoi_dung"),
        ("task_items", "CourseId", "ma_mon_hoc"),
        ("task_items", "ParentTaskId", "ma_nhiem_vu_cha"),
        ("task_items", "Title", "tieu_de"),
        ("task_items", "Description", "mo_ta"),
        ("task_items", "Type", "loai"),
        ("task_items", "DueDate", "han_hoan_thanh"),
        ("task_items", "Priority", "muc_do_uu_tien"),
        ("task_items", "Status", "trang_thai"),
        ("task_items", "EstimatedMinutes", "thoi_luong_du_kien_phut"),
        ("task_items", "Progress", "tien_do"),
        ("task_items", "ReminderHours", "nhac_truoc_gio"),
        ("task_items", "CompletedAt", "ngay_hoan_thanh"),
        ("task_items", "CreatedAt", "ngay_tao"),
        ("task_items", "UpdatedAt", "ngay_cap_nhat"),
        ("users", "Id", "ma"),
        ("users", "Email", "email"),
        ("users", "NormalizedEmail", "email_chuan_hoa"),
        ("users", "FullName", "ho_ten"),
        ("users", "PasswordHash", "mat_khau_bam"),
        ("users", "StudentCode", "ma_so_sinh_vien"),
        ("users", "ClassId", "ma_lop"),
        ("users", "Major", "nganh_hoc"),
        ("users", "Role", "vai_tro"),
        ("users", "IsActive", "dang_hoat_dong"),
        ("users", "TimeZone", "mui_gio"),
        ("users", "ReminderHours", "nhac_truoc_gio"),
        ("users", "CreatedAt", "ngay_tao"),
        ("users", "UpdatedAt", "ngay_cap_nhat"),
    ];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var column in Columns)
            migrationBuilder.RenameColumn(name: column.Before, table: column.Table, newName: column.After);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        for (var i = Columns.Length - 1; i >= 0; i--)
            migrationBuilder.RenameColumn(name: Columns[i].After, table: Columns[i].Table, newName: Columns[i].Before);
    }
}
