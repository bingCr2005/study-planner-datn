using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace StudyPlanner.Api.HocTap;
public sealed class MocThoiGianModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
        context.Metadata.ModelType == typeof(DateTimeOffset) || context.Metadata.ModelType == typeof(DateTimeOffset?) ? new MocThoiGianModelBinder() : null;
}
