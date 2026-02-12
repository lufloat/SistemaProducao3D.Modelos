using System.Threading.Tasks;

namespace Business_Logic.Serviços.Sync
{
    public interface ISyncService
    {
        Task SincronizarMesAsync(int ano, int mes);

        Task SincronizarPeriodoAsync(
            int anoInicio,
            int mesInicio,
            int anoFim,
            int mesFim);

        Task SincronizarJobAsync(string uuid);
    }
}
