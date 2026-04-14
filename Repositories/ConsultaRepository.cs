using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicaApi.Data;
using ClinicaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicaApi.Repositories
{
    // O Repositório encapsula a lógica de comunicação direta com o Banco de Dados.
    // Assim, os controladores ficam mais limpos e separados da tecnologia de dados (EF Core).
    public class ConsultaRepository : IConsultaRepository
    {
        private readonly ClinicaDbContext _context;

        // Injeção de dependência do DbContext via construtor
        public ConsultaRepository(ClinicaDbContext context)
        {
            _context = context;
        }

        // Recupera a lista completa de todas as consultas agendadas.
        public async Task<IEnumerable<Consulta>> GetAllAsync()
        {
            return await _context.Consultas
                .Include(c => c.Paciente)     // Include obriga o Entity Framework a fazer uma junção (Join)
                .Include(c => c.Profissional) // com a tabela de Profissionais e Pacientes para preencher seus dados.
                .ToListAsync();
        }

        // Método padrão de resgate via chave ID
        public async Task<Consulta?> GetByIdAsync(int id)
        {
            return await _context.Consultas
                .Include(c => c.Paciente)
                .Include(c => c.Profissional)
                .FirstOrDefaultAsync(c => c.Id == id); // Procura e retorna a primeira consulta se o ID for igual
        }

        // Salva a nova consulta no Banco de Dados
        public async Task<Consulta> AddAsync(Consulta consulta)
        {
            _context.Consultas.Add(consulta); // Adiciona na memória local do EF Core
            await _context.SaveChangesAsync(); // Transforma o comando em SQL (INSERT INTO) que salva de fato no banco 
            return consulta;
        }

        // Atualiza uma consulta
        public async Task UpdateAsync(Consulta consulta)
        {
            _context.Entry(consulta).State = EntityState.Modified; // Diz ao EF Core que os campos desse registro mudaram
            await _context.SaveChangesAsync(); // Finaliza fazendo o UPDATE base
        }

        // Exclui e destrói os dados permanentemente (DELETE SQL)
        public async Task DeleteAsync(int id)
        {
            var consulta = await _context.Consultas.FindAsync(id);
            if (consulta != null)
            {
                _context.Consultas.Remove(consulta);
                await _context.SaveChangesAsync();
            }
        }

        // REGRA DE NEGÓCIO DA CLÍNICA
        // Verifica se há alguma sobreposição ou agendamento duplo já existente.
        public async Task<bool> ExisteConflitoHorarioAsync(int profissionalId, int pacienteId, DateTime dataHora)
        {
            // Qualquer registro no banco (AnyAsync) onde seja o MESMO profissional OU o MESMO paciente
            // Na MESMA hora e data pretendida
            // E a consulta já agendada ainda conta se o status NÃO for "Cancelada".
            return await _context.Consultas.AnyAsync(c => 
                (c.ProfissionalId == profissionalId || c.PacienteId == pacienteId) && 
                c.DataHora == dataHora && 
                c.Status != StatusConsulta.Cancelada);
        }
    }
}
