using ClinicaApi.Data;
using ClinicaApi.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

// Cria o construtor da aplicação Web (Web API)
var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÃO DE SERVIÇOS (Injeção de Dependências) ---

// Adiciona os Controladores (Controllers) à aplicação e configura o JSON
// A configuração JsonStringEnumConverter garante que Enums (ex: Especialidade, StatusConsulta)
// sejam salvos e retornados como texto (ex: "Agendada") ao invés de números (ex: 0).
builder.Services.AddControllers().AddJsonOptions(options => 
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Configurações do Swagger (Documentação interativa da API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configura o Entity Framework Core para conectar ao banco de dados SQLite local
// A string de conexão "DefaultConnection" deve estar no arquivo appsettings.json
builder.Services.AddDbContext<ClinicaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração do Padrão Repository (Injeção de Dependências)
// AddScoped: Cria uma instância nova do repositório por cada requisição HTTP.
// Sempre que o controlador pedir a Interface (I...), o ASP.NET entrega a classe concreta (...Repository)
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IProfissionalRepository, ProfissionalRepository>();
builder.Services.AddScoped<IConsultaRepository, ConsultaRepository>();

// Finaliza a inicialização e constrói a aplicação
var app = builder.Build();

// --- 2. MIDDLEWARES E PIPELINE HTTP ---

// Habilita o Swagger apenas em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redireciona chamadas HTTP para HTTPS de forma segura
app.UseHttpsRedirection();

// Middleware de autorização (se futuramente tivermos login)
app.UseAuthorization();

// Mapeia as rotas dos Controladores (ex: /api/pacientes)
app.MapControllers();

// Coloca a aplicação para rodar e escutar as portas configuradas
app.Run();
