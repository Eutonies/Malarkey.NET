using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Configuration;
public class PersistenceConfiguration
{
    public const string ConfigurationElementName = "Persistence";
    public MalarkeyDbConfiguration Db { get; set; }

    public int? CleanupIntervalInSeconds { get; set; }

}
