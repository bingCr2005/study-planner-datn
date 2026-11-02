namespace StudyPlanner.Domain.Entities;

public class Cohort
{
    public int Ma { get; set; }
    public string Ten { get; set; } = string.Empty;
    public int NamBatDau { get; set; }
    public int NamKetThuc { get; set; }
    public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
}

