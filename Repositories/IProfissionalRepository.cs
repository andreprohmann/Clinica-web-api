using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicaApi.Models;

namespace ClinicaApi.Repositories
{
    public interface IProfissionalRepository
    {
        Task<IEnumerable<Profissional>> GetAllAsync();
        Task<Profissional?> GetByIdAsync(int id);
        Task<Profissional> AddAsync(Profissional profissional);
        Task UpdateAsync(Profissional profissional);
        Task DeleteAsync(int id);
    }
}
