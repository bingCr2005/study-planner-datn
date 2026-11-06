using StudyPlanner.Application.HocTap;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
namespace StudyPlanner.CoreChecks;
public static class NhiemVuChecks
{
    public static void Run()
    {
        var now=new DateTimeOffset(2026,11,5,12,0,0,TimeSpan.Zero);var count=0;
        void Check(bool ok,string label){if(!ok)throw new Exception("FAIL graph: "+label);count++;}
        void Reject(string code,Action action){try{action();throw new Exception("Expected "+code);}catch(HocTapException e){Check(e.Code==code,code);}}
        NhiemVu N(int id,int? parent=null,int progress=0)=>new(){Ma=id,MaNguoiDung=1,MaNhiemVuCha=parent,TieuDe="Việc "+id,TienDo=progress};
        var root=N(1);root.HanHoanThanh=now.AddDays(2);var a=N(2,1,60);var b=N(3,1);var nodes=new List<NhiemVu>{root,a,b};
        QuyTacCay.KiemTra(nodes);QuyTacCay.TinhLai(nodes,now);Check(root.TienDo==30&&root.TrangThai==TrangThaiNhiemVu.InProgress,"average direct children");
        Check(QuyTacCay.HanHieuLuc(a,nodes.ToDictionary(n=>n.Ma))==(root.HanHoanThanh,1),"inherited deadline and source");
        a.HanHoanThanh=now.AddDays(1);Check(QuyTacCay.HanHieuLuc(a,nodes.ToDictionary(n=>n.Ma))==(a.HanHoanThanh,2),"own deadline wins");
        a.HanHoanThanh=now.AddDays(3);Reject("TASK_DEADLINE_CONFLICT",()=>QuyTacCay.KiemTra(nodes));a.HanHoanThanh=null;
        root.MaNhiemVuCha=a.Ma;Reject("TASK_CYCLE",()=>QuyTacCay.KiemTra(nodes));root.MaNhiemVuCha=null;
        a.MaMonHoc=4;Reject("TASK_SCOPE_CONFLICT",()=>QuyTacCay.KiemTra(nodes));a.MaMonHoc=null;
        a.TienDo=1;b.TienDo=0;QuyTacCay.TinhLai(nodes,now);Check(root.TienDo==1,"mixed rounded0 clamped1");
        a.TienDo=b.TienDo=100;QuyTacCay.TinhLai(nodes,now);Check(root.TienDo==100&&root.NgayHoanThanh==now,"completion timestamp");
        QuyTacCay.TinhLai(nodes,now.AddMinutes(10));Check(root.NgayHoanThanh==now,"preserve first completion time");
        b.TienDo=40;QuyTacCay.TinhLai(nodes,now);Check(root.TienDo==70&&root.NgayHoanThanh==null,"reopening child clears ancestor completion");
        var deep=Enumerable.Range(1,10).Select(id=>N(id,id==1?null:id-1)).ToList();QuyTacCay.KiemTra(deep);Check(true,"10 levels allowed");
        deep.Add(N(11,10));Reject("TASK_TREE_LIMIT",()=>QuyTacCay.KiemTra(deep));
        var wide=Enumerable.Range(1,1001).Select(id=>N(id,id==1?null:1)).ToList();Reject("TASK_TREE_LIMIT",()=>QuyTacCay.KiemTra(wide));
        var leaf=N(99,null,25);leaf.ThoiLuongDuKienPhut=200;Check(leaf.ThoiLuongConLaiPhut==150,"remaining estimate");
        leaf.ThoiLuongDuKienPhut=3;leaf.TienDo=50;Check(leaf.ThoiLuongConLaiPhut==2,"round remaining upward");
        Console.WriteLine($"PASS: {count} graph/deadline/progress assertions");
    }
}
