using System.Text.Json.Serialization;

namespace StudyPlanner.Application.HocTap;

public sealed record GhimNhiemVuRequest([property: JsonRequired] bool DaGhim);
