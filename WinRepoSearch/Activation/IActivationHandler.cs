using System.Threading.Tasks;

namespace WinRepoSearch.Activation
{
    public interface IActivationHandler
    {
        bool CanHandle(object args);

        Task HandleAsync(object args);
    }
}
