using System.Threading.Tasks;

namespace Api.Infrastructure
{
    public interface IBlackListChecker
    {
        Task<bool> Check(string value);
    }
}