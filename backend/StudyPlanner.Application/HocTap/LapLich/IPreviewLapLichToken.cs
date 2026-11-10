namespace StudyPlanner.Application.HocTap;

public interface IPreviewLapLichToken
{
    string Tao(NoiDungPreviewLapLich noiDung);
    NoiDungPreviewLapLich Doc(string token);
}
