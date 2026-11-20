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
        NhiemVu N(int id,int? parent=null,int progress=0)=>new(){Ma=id,MaNguoiDung=1,MaNhiemVuCha=parent,TieuDe="Việc "+id,TienDo=progress,
            TrangThai=progress==100?TrangThaiNhiemVu.Completed:progress==0?TrangThaiNhiemVu.NotStarted:TrangThaiNhiemVu.InProgress,
            NgayHoanThanh=progress==100?now:null};
        var root=N(1);root.HanHoanThanh=now.AddDays(2);var a=N(2,1,60);var b=N(3,1);var nodes=new List<NhiemVu>{root,a,b};
        root.ThoiLuongDuKienPhut=120;a.ThoiLuongDuKienPhut=60;
        QuyTacCay.KiemTra(nodes);QuyTacCay.BoUocLuongCha(nodes);
        Check(root.ThoiLuongDuKienPhut==null&&a.ThoiLuongDuKienPhut==60,"only leaves retain their own estimates");
        QuyTacCay.TongHopTienDo(nodes,now);
        Check(root.TienDo==30&&root.TrangThai==TrangThaiNhiemVu.InProgress,"parent uses the mean of immediate children");
        Check(QuyTacCay.HanHieuLuc(a,nodes.ToDictionary(n=>n.Ma))==(root.HanHoanThanh,1),"inherited deadline and source");
        a.HanHoanThanh=now.AddDays(1);Check(QuyTacCay.HanHieuLuc(a,nodes.ToDictionary(n=>n.Ma))==(a.HanHoanThanh,2),"own deadline wins");
        a.HanHoanThanh=now.AddDays(3);Reject("TASK_DEADLINE_CONFLICT",()=>QuyTacCay.KiemTra(nodes));a.HanHoanThanh=null;
        root.MaNhiemVuCha=a.Ma;Reject("TASK_CYCLE",()=>QuyTacCay.KiemTra(nodes));root.MaNhiemVuCha=null;
        a.MaMonHoc=4;Reject("TASK_SCOPE_CONFLICT",()=>QuyTacCay.KiemTra(nodes));a.MaMonHoc=null;
        root.TienDo=100;root.TrangThai=TrangThaiNhiemVu.Completed;root.NgayHoanThanh=now;
        b.TienDo=40;QuyTacCay.TongHopTienDo(nodes,now);
        Check(root.TienDo==50&&root.TrangThai==TrangThaiNhiemVu.InProgress&&root.NgayHoanThanh==null,"editing a child reopens completed ancestors");
        a.TienDo=100;b.TienDo=100;QuyTacCay.TongHopTienDo(nodes,now);
        Check(root.TienDo==100&&root.TrangThai==TrangThaiNhiemVu.Completed&&root.NgayHoanThanh==now,"all completed children complete their parent");
        QuyTacCay.TongHopTienDo(nodes,now.AddHours(1));
        Check(root.NgayHoanThanh==now,"unchanged completion preserves its timestamp");
        b.TienDo=99;QuyTacCay.TongHopTienDo(nodes,now.AddHours(2));
        Check(root.TienDo==99&&root.TrangThai!=TrangThaiNhiemVu.Completed&&root.NgayHoanThanh==null,"99.5 percent does not round to completed");
        b.TienDo=100;QuyTacCay.TongHopTienDo(nodes,now.AddHours(3));
        Check(root.NgayHoanThanh==now.AddHours(3),"recompletion records a new completion timestamp");
        var nested=new List<NhiemVu>{N(10),N(11,10),N(12,10),N(13,11,100),N(14,11)};
        QuyTacCay.TongHopTienDo(nested,now);
        Check(nested.Single(n=>n.Ma==11).TienDo==50&&nested[0].TienDo==25,"three-level progress propagates bottom-up");
        Check(nested[0].TienDo!=33,"grandchildren are not flattened into the root denominator");
        nested.Single(n=>n.Ma==14).TienDo=100;QuyTacCay.TongHopTienDo(nested,now);
        Check(nested[0].TienDo==50,"completed branches remain part of the parent mean");
        var forest=new List<NhiemVu>{N(20),N(21,20,20),N(22),N(23,22,80)};
        QuyTacCay.TongHopTienDo(forest,now);
        Check(forest[0].TienDo==20&&forest[2].TienDo==80,"independent roots are recalculated in the same forest");
        Check(forest[1].TienDo==20&&forest[3].TienDo==80,"manual progress of leaves is preserved");
        var deep=Enumerable.Range(1,10).Select(id=>N(id,id==1?null:id-1)).ToList();QuyTacCay.KiemTra(deep);Check(true,"10 levels allowed");
        QuyTacCay.KiemTra(deep, new HashSet<int> { 5 });
        Check(true, "new level 5 is allowed alongside an existing deeper branch");
        Reject("TASK_DEPTH_LIMIT", () => QuyTacCay.KiemTra(deep, new HashSet<int> { 6 }));
        deep.Add(N(11,10));Reject("TASK_TREE_LIMIT",()=>QuyTacCay.KiemTra(deep));
        var wide=Enumerable.Range(1,1001).Select(id=>N(id,id==1?null:1)).ToList();Reject("TASK_TREE_LIMIT",()=>QuyTacCay.KiemTra(wide));
        var leaf=N(99,null,25);leaf.ThoiLuongDuKienPhut=200;Check(leaf.ThoiLuongConLaiPhut==150,"remaining estimate");
        leaf.ThoiLuongDuKienPhut=3;leaf.TienDo=50;Check(leaf.ThoiLuongConLaiPhut==2,"round remaining upward");
        Console.WriteLine($"PASS: {count} graph/deadline/progress assertions");
    }
}
