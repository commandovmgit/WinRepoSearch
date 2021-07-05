using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WinRepoSearch.ViewModels
{
    public class SettingsViewModel : ObservableRecipient
    {
        //private readonly IThemeSelectorService _themeSelectorService;
        //private ElementTheme _elementTheme;

        //public ElementTheme ElementTheme
        //{
        //    get { return _elementTheme; }

        //    set { SetProperty(ref _elementTheme, value); }
        //}

        private string _versionDescription;

        public string VersionDescription
        {
            get { return _versionDescription; }

            set { SetProperty(ref _versionDescription, value); }
        }

        private ICommand _switchThemeCommand;

        public ICommand SwitchThemeCommand
        {
            get
            {
                if (_switchThemeCommand == null)
                {
                    _switchThemeCommand = new RelayCommand<string>(
                        async (param) =>
                        {
                            //if (ElementTheme != param)
                            //{
                            //    ElementTheme = param;
                            //    await _themeSelectorService.SetThemeAsync(param);
                            //}
                        });
                }

                return _switchThemeCommand;
            }
        }

//#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
//#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
//        public SettingsViewModel(IThemeSelectorService themeSelectorService)
//#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
//#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
//        {
//            _themeSelectorService = themeSelectorService;
//            _elementTheme = _themeSelectorService.Theme;
//            VersionDescription = GetVersionDescription();
//        }

        private string GetVersionDescription()
        {
            var appName = "AppDisplayName";//.GetLocalized();
            //var package = Package.Current;
            //var packageId = package.Id;
            //var version = packageId.Version;

            return $"{appName}"; // - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
}
