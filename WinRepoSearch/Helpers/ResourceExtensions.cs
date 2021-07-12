using Microsoft.ApplicationModel.Resources;

namespace WinRepoSearch.Helpers
{
    internal static class ResourceExtensions
    {
        private static ResourceLoader _resLoader = new();

        public static string GetLocalized(this string resourceKey)
        {
            return _resLoader.GetString(resourceKey);
        }
    }
}
