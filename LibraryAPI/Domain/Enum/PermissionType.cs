using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Domain.Enum
{
    public enum PermissionType
    {
        None = 0,
        Viewer = 1,
        Editor = 2,
        Owner = 3
    }
}
