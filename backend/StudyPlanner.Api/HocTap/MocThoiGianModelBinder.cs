using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace StudyPlanner.Api.HocTap;

// Query binding otherwise silently accepts server-local datetimes with no explicit offset.
public sealed class MocThoiGianModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext context)
    {
        var supplied = context.ValueProvider.GetValue(context.ModelName);
        if (supplied == ValueProviderResult.None) return Task.CompletedTask;
        context.ModelState.SetModelValue(context.ModelName,supplied); var value=supplied.FirstValue;
        if (!string.IsNullOrEmpty(value) && Regex.IsMatch(value,@"T.*(?:[zZ]|[+-]\d{2}:\d{2})$") && DateTimeOffset.TryParse(value,CultureInfo.InvariantCulture,DateTimeStyles.None,out var date))
            context.Result=ModelBindingResult.Success(date.ToUniversalTime());
        else context.ModelState.TryAddModelError(context.ModelName,"Thời điểm phải có offset: Z hoặc +07:00.");
        return Task.CompletedTask;
    }
}
