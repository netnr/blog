#if Full || ImportBase

global using System;
global using System.IO;
global using System.Linq;
global using System.Text;
global using System.Data;
global using System.Data.Common;
global using System.Diagnostics;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.Collections.Specialized;

#endif

#if Full || ImportWeb

global using Newtonsoft.Json;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;

#endif