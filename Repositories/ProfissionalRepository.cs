using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicaApi.Data;
using ClinicaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicaApi.Repositories
{
    public class ProfissionalRepository : IProfissionalRepository
    {
        private readonly ClinicaDbContext _context;

        public ProfissionalRepository(ClinicaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Profissional>> GetAllAsync()
        {
            return await _context.Profissionais.ToListAsync();
        }

        public async Task<Profissional?> GetByIdAsync(int id)
        {
            return await _context.Profissionais.FindAsync(id);
        }

        public async Task<Profissional> AddAsync(Profissional profissional)
        {
            _context.Profissionais.Add(profissional);
            await _context.SaveChangesAsync();
            return profissional;
        }

        public async Task UpdateAsync(Profissional profissional)
        {
            _context.Entry(profissional).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var profissional = await _context.Profissionais.FindAsync(id);
            if (profissional != null)
            {
                _context.Profissionais.Remove(profissional);
                await _context.SaveChangesAsync();
            }
        }
    }
}
