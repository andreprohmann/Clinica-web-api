using ClinicaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicaApi.Data
{
    // A classe DbContext é o coração do Entity Framework Core.
    // Ela age como uma ponte entre as nossas classes C# e o Banco de Dados.
    public class ClinicaDbContext : DbContext
    {
        // Construtor que recebe as opções de conexão (do Program.cs) e repassa para a classe base.
        public ClinicaDbContext(DbContextOptions<ClinicaDbContext> options) : base(options) { }

        // DbSet representa as tabelas no banco de dados.
        // Toda vez que usarmos _context.Pacientes, estaremos interagindo com a tabela "Pacientes".
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Profissional> Profissionais { get; set; }
        public DbSet<Consulta> Consultas { get; set; }

        // Mapeamento e configuração avançada do Banco de Dados (Fluent API)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configura a relação entre Consulta e Paciente (1 para Muitos)
            modelBuilder.Entity<Consulta>()
                .HasOne(c => c.Paciente)           // Uma Consulta possui apenas UM Paciente...
                .WithMany(p => p.Consultas)        // ... e UM Paciente pode ter VÁRIAS Consultas.
                .HasForeignKey(c => c.PacienteId); // A ligação no banco ocorre pelo campo PacienteId.

            // Configura a relação entre Consulta e Profissional (1 para Muitos)
            modelBuilder.Entity<Consulta>()
                .HasOne(c => c.Profissional)       // Uma Consulta possui apenas UM Profissional (Médico)...
                .WithMany(p => p.Consultas)        // ... e UM Profissional pode ter VÁRIAS Consultas.
                .HasForeignKey(c => c.ProfissionalId); // A ligação no banco ocorre pelo campo ProfissionalId.
        }
    }
}
