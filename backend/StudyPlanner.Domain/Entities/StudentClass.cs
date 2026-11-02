namespace StudyPlanner.Domain.Entities;

public class StudentClass
{
    public int Ma { get; set; }
    public int MaKhoa { get; set; }
    public int MaKhoaHoc { get; set; }
    public string Ten { get; set; } = string.Empty;
    public Faculty Faculty { get; set; } = null!;
    public Cohort Cohort { get; set; } = null!;
    public ICollection<User> Users { get; set; } = new List<User>();
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
}

