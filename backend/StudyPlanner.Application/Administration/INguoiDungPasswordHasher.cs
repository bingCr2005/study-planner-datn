using System.Linq.Expressions;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Administration;

public interface INguoiDungPasswordHasher
{
    string Hash(NguoiDung user, string password);
    bool Verify(NguoiDung user, string password);
}

