using System.Threading.Tasks;

namespace TheUrlist.Infrastructure
{
    public interface IBlackListChecker
    {
        Task<bool> Check(string value);
    }
}