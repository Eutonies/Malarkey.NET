﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Configuration;
public class MalarkeyClientCertificateConfiguration
{
    public string SourceType { get; set; }
    public string CertificateDiskPath { get; set; }
    public string CertificatePassword { get; set; }

}
