using System.ComponentModel.DataAnnotations;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.Administration;

public sealed record PhanTrangResponse<T>(IReadOnlyList<T> DanhSach, int Trang, int KichThuocTrang, int TongSo);

