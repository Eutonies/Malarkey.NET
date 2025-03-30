using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Cleanup;

public interface IPersistenceCleaner
{
    public Task PerformCleanup();


}
