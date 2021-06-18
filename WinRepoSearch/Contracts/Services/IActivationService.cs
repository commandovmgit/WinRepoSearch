using System.Threading.Tasks;

namespace WinRepoSearch.Contracts.Services
{
    public interface IActivationService
    {
        Task ActivateAsync(object activationArgs);
    }
}
