using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinRepoSearch.Core.Contracts.Services
{
    public interface IStartup
    {
        IServiceProvider? ServiceProvider { get; set; }
    }
}
