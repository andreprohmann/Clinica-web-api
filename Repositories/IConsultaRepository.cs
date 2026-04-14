using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicaApi.Models;

namespace ClinicaApi.Repositories
{
    public interface IConsultaRepository
    {
        Task<IEnumerable<Consulta>> GetAllAsync();
        Task<Consulta?> GetByIdAsync(int id);
        Task<Consulta> AddAsync(Consulta consulta);
        Task UpdateAsync(Consulta consulta);
        Task DeleteAsync(int id);
        Task<bool> ExisteConflitoHorarioAsync(int profissionalId, int pacienteId, DateTime dataHora);
    }
}
