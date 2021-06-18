using System;

namespace WinRepoSearch.Contracts.Services
{
    public interface IPageService
    {
        Type GetPageType(string key);
    }
}
