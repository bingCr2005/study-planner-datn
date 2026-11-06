using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StudyPlanner.Application.Academic;

public sealed class AcademicListRequest
{
    [StringLength(200)] public string? TuKhoa { get; init; }
    [Range(1, int.MaxValue)] public int Trang { get; init; } = 1;
    [Range(1, 100)] public int KichThuocTrang { get; init; } = 20;
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SemesterRequest : IValidatableObject
{
    [Required, StringLength(200)] public string Ten { get; init; } = "";
    [Required] public DateOnly? NgayBatDau { get; init; }
    [Required] public DateOnly? NgayKetThuc { get; init; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (NgayKetThuc < NgayBatDau)
            yield return new("Ngày kết thúc phải từ ngày bắt đầu trở đi.", [nameof(NgayKetThuc)]);
    }
}

// MaHocKy comes only from the POST route; PUT cannot move a course.
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CourseRequest
{
    [Required, StringLength(200)] public string Ten { get; init; } = "";
    [StringLength(50)] public string? KyHieu { get; init; }
    [Range(1, int.MaxValue)] public int? SoTinChi { get; init; }
    [StringLength(200)] public string? GiangVien { get; init; }
    [RegularExpression("^#[0-9A-Fa-f]{6}$")] public string? MauSac { get; init; }
}

public sealed record SemesterResponse(int Ma, string Ten, DateOnly NgayBatDau, DateOnly NgayKetThuc,
    DateTimeOffset NgayTao, DateTimeOffset NgayCapNhat);
public sealed record CourseResponse(int Ma, int MaHocKy, string Ten, string? KyHieu, int? SoTinChi,
    string? GiangVien, string? MauSac, DateTimeOffset NgayTao, DateTimeOffset NgayCapNhat);
public sealed record DeletionPreviewResponse(int SoMonHoc, int SoBuoiHoc, int SoNhiemVuBoLienKet);
