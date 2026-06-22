using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Common;

public static class DatabaseErrorDetector
{
    public static bool IsDuplicateKey(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException
            && (sqlException.Number == 2627 || sqlException.Number == 2601);
    }
}
