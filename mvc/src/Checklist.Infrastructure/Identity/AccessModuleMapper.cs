using Checklist.Application.Common;
using Checklist.Infrastructure.Data.Models;

namespace Checklist.Infrastructure.Identity;

internal static class AccessModuleMapper
{
    public static string ToCode(MvcAccessModule module)
    {
        return module switch
        {
            MvcAccessModule.OperationalSupervision => AccessModuleCodes.OperationalSupervision,
            MvcAccessModule.WorkSafety => AccessModuleCodes.WorkSafety,
            MvcAccessModule.MaterialInspection => AccessModuleCodes.MaterialInspection,
            _ => throw new ArgumentOutOfRangeException(nameof(module), module, "Access module is not mapped.")
        };
    }
}
