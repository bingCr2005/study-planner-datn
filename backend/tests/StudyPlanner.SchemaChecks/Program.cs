using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using StudyPlanner.Infrastructure.Data;
// Chỉ kiểm tra metadata/DDL, không mở kết nối database.
var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql("Host=localhost;Database=schema_checks;Username=unused;Password=unused").Options;
using var db = new ApplicationDbContext(options);
var model=db.GetService<IDesignTimeModel>().Model;
var entities=model.GetEntityTypes().ToArray();
string[] tables=["schools","faculties","cohorts","student_classes","users","semesters","courses","class_sessions","task_items","notifications","task_histories"];
Check(entities.Length==11 && entities.Select(x=>x.GetTableName()).Order().SequenceEqual(tables.Order()),"11 expected tables");
Check(entities.Sum(x=>x.GetForeignKeys().Count())==13,"13 foreign keys");
Check(entities.All(x=>x.GetProperties().All(p=>!p.IsShadowProperty())),"No shadow columns");
Check(!db.Database.HasPendingModelChanges(),"Current model matches migration snapshot");
var sql=db.Database.GenerateCreateScript();
Check(sql.Contains("CK_task_items_state") && sql.Contains("ON DELETE SET NULL") && sql.Contains("ON DELETE RESTRICT"),"DDL keeps state and delete constraints");
Console.WriteLine("All schema checks passed without a database connection.");
static void Check(bool condition,string label) { if(!condition)throw new Exception(label);Console.WriteLine("PASS: "+label); }
