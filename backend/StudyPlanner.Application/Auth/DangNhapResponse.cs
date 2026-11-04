using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.Common;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Auth;

public sealed record DangNhapResponse(string TokenTruyCap, string LoaiToken, DateTimeOffset HetHanLuc, NguoiDungResponse NguoiDung);

