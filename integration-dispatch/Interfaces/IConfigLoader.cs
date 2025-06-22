using integration_dispatch.Model;
namespace integration_dispatch.Interfaces
{
    public interface IConfigLoader
    {
        Task<Config> LoadConfigAsync();
    }
}
