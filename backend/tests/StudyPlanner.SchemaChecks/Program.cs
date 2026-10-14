using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.Data;

// Only build metadata and SQL: never connect to or change a real database.
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseNpgsql("Host=localhost;Database=schema_checks;Username=unused;Password=unused")
    .Options;
using var context = new ApplicationDbContext(options);
var model = context.GetService<IDesignTimeModel>().Model;
var entities = model.GetEntityTypes().ToArray();
var expectedTables = new[]
{
    "schools", "faculties", "cohorts", "student_classes", "users",
    "semesters", "courses", "class_sessions", "task_items", "notifications", "task_histories"
};
Check(entities.Select(x => x.GetTableName()).Order().SequenceEqual(expectedTables.Order()),
    "Exactly the 11 ERD tables are mapped");
Check(entities.Sum(x => x.GetForeignKeys().Count()) == 13, "Exactly 13 foreign keys");
Check(entities.All(x => x.GetProperties().All(p => !p.IsShadowProperty())),
    "No accidental shadow columns from misspelled foreign keys");

var expectedDeletes = new (Type Dependent, string Property, Type Principal, DeleteBehavior Behavior)[]
{
    (typeof(Faculty), "SchoolId", typeof(School), DeleteBehavior.Restrict),
    (typeof(StudentClass), "FacultyId", typeof(Faculty), DeleteBehavior.Restrict),
    (typeof(StudentClass), "CohortId", typeof(Cohort), DeleteBehavior.Restrict),
    (typeof(User), "ClassId", typeof(StudentClass), DeleteBehavior.SetNull),
    (typeof(Semester), "UserId", typeof(User), DeleteBehavior.Cascade),
    (typeof(Course), "SemesterId", typeof(Semester), DeleteBehavior.Cascade),
    (typeof(ClassSession), "CourseId", typeof(Course), DeleteBehavior.Cascade),
    (typeof(TaskItem), "UserId", typeof(User), DeleteBehavior.Cascade),
    (typeof(TaskItem), "CourseId", typeof(Course), DeleteBehavior.SetNull),
    (typeof(TaskItem), "ParentTaskId", typeof(TaskItem), DeleteBehavior.Cascade),
    (typeof(Notification), "UserId", typeof(User), DeleteBehavior.Cascade),
    (typeof(Notification), "TaskId", typeof(TaskItem), DeleteBehavior.Cascade),
    (typeof(TaskHistory), "UserId", typeof(User), DeleteBehavior.SetNull)
};
foreach (var (dependent, property, principal, behavior) in expectedDeletes)
{
    var fk = Entity(dependent).GetForeignKeys().Single(x =>
        x.Properties.Count == 1 && x.Properties[0].Name == property);
    Check(fk.PrincipalEntityType.ClrType == principal && fk.DeleteBehavior == behavior,
        dependent.Name + "." + property + " delete behavior");
}
Check(Entity(typeof(TaskHistory)).GetForeignKeys().All(x =>
    x.Properties.All(p => p.Name != "TaskId")), "History survives task deletion without a Task FK");
Check(Entity(typeof(TaskHistory)).FindProperty("OldData")!.GetColumnType() == "jsonb",
    "History snapshots use jsonb");
Check(Entity(typeof(TaskItem)).FindProperty("CourseId")!.IsNullable &&
      Entity(typeof(TaskItem)).FindProperty("ParentTaskId")!.IsNullable, "Personal and root tasks allowed");
Check(Entity(typeof(Semester)).FindProperty("StartDate")!.GetColumnType() == "date" &&
      Entity(typeof(ClassSession)).FindProperty("StartTime")!.GetColumnType() == "time without time zone",
    "Recurring local calendar dates and times retain their types");
Check(Entity(typeof(User)).GetIndexes().Any(x => x.IsUnique &&
    Names(x) == "NormalizedEmail"), "Normalized email unique");
Check(Entity(typeof(User)).GetIndexes().Any(x => x.IsUnique &&
    Names(x) == "StudentCode" && x.GetFilter() == "\"StudentCode\" IS NOT NULL"),
    "Student code optional but unique when present");
Check(Entity(typeof(Notification)).GetIndexes().Any(x => x.IsUnique &&
    Names(x) == "TaskId,Type"), "One reminder per task and type");
Check(Entity(typeof(Notification)).GetIndexes().Count(x =>
    Names(x) == "UserId,SentAt" && x.GetFilter() != null) == 2,
    "Delivered and unread inbox indexes both preserved");
var priority = Entity(typeof(TaskItem)).FindProperty("Priority")!;
var reminderHours = Entity(typeof(User)).FindProperty("ReminderHours")!;
Check(Equals(reminderHours.Sentinel, 24) && !Equals(reminderHours.Sentinel, 0),
    "Explicit zero reminder hours is not replaced by DB default");
Check(Equals(priority.Sentinel, TaskPriority.Medium) &&
      !Equals(priority.Sentinel, TaskPriority.Low), "Explicit Low priority is not replaced by DB default");
foreach (var entity in entities)
{
    foreach (var property in entity.GetProperties().Where(x => x.ClrType.IsEnum))
        Check(property.GetTypeMapping().Converter!.ProviderClrType == typeof(string) &&
              property.GetMaxLength() == 32, entity.ClrType.Name + "." + property.Name + " string enum");
}

// Compile PostgreSQL DDL without a connection; real FK/CHECK enforcement needs PostgreSQL tests.
var sql = context.Database.GenerateCreateScript();
Check(expectedTables.All(table => sql.Contains("CREATE TABLE " + table + " (")),
    "PostgreSQL SQL generated for all tables");
Check(sql.Contains("CK_task_items_state") && sql.Contains("CK_notifications_read") &&
      sql.Contains("CK_class_sessions_times"), "Critical CHECK constraints included in SQL");
Check(sql.Contains("ON DELETE SET NULL") && sql.Contains("ON DELETE RESTRICT") &&
      sql.Contains("ON DELETE CASCADE"), "Delete behaviors included in PostgreSQL SQL");
Console.WriteLine("All schema checks passed. No database connection was opened.");

IEntityType Entity(Type type) => model.FindEntityType(type)!;
static string Names(IIndex index) => string.Join(",", index.Properties.Select(x => x.Name));
static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
    Console.WriteLine("PASS: " + message);
}
