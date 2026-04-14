# Documentação e Explicação da Arquitetura: Web API Clínica

Esta documentação foi gerada para auxiliar no entendimento do código e servir de apoio para apresentações acadêmicas. A Web API foi construída seguindo as melhores práticas do **Padrão de Repositório (Repository Pattern)** e **Entity Framework Core**.

Abaixo está o detalhamento técnico do que cada camada e trecho de código faz no projeto.

---

## 1. Camada de Modelos (`Models`)
Os modelos representam as entidades (tabelas) do seu banco de dados e as regras básicas de negócio associadas a elas.

**Exemplo: `Paciente.cs`**
```csharp
public class Paciente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    // ...
    
    public int Idade
    {
        get
        {
            var today = DateTime.Today;
            var age = today.Year - DataNascimento.Year;
            if (DataNascimento.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}
```
> [!NOTE]
> **Explicação do Código:**
> Definimos as propriedades básicas do paciente. O grande destaque é a propriedade computada **`Idade`**. Ela não possui um modificador `set` (isso significa que você não salva a idade diretamente no banco). Toda vez que você pedir a idade na API, o código fará o cálculo dinâmico baseado na data atual e na propriedade `DataNascimento`, verificando precisamente se o paciente já fez aniversário no ano vigente. O `[JsonIgnore]` usado em coleções (como `Consultas`) evita a falha de "referência circular", impedindo loops infinitos quando a API vai exibir a lista de volta para o cliente.

---

## 2. Camada de Banco de Dados (`Data`)
Serve como a interface e ponte de comunicação entre o seu código C# e o banco de dados (neste caso, o SQLite).

**Arquivo: `ClinicaDbContext.cs`**
```csharp
public class ClinicaDbContext : DbContext
{
    public DbSet<Paciente> Pacientes { get; set; }
    public DbSet<Profissional> Profissionais { get; set; }
    public DbSet<Consulta> Consultas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Consulta>()
            .HasOne(c => c.Paciente)
            .WithMany(p => p.Consultas)
            .HasForeignKey(c => c.PacienteId);
        //...
    }
}
```
> [!NOTE]
> **Explicação do Código:**
> * **`DbSet<>`:** Representam as tabelas físicas no banco. São através dessas propriedades que podemos buscar (`Select`), salvar (`Insert`), atualizar (`Update`) e deletar (`Delete`) os registros.
> * **`OnModelCreating`:** Este trecho mapeia e configura rigorosamente os relacionamentos usando as regras do Entity Framework Core. Estamos traduzindo a arquitetura modelada: *"Uma Consulta pertence a estritamente UM Paciente (`HasOne`), em contrapartida um Paciente pode registrar VÁRIAS Consultas (`WithMany`). A ligação entre as duas tabelas ocorre pelo `PacienteId` (Chave Estrangeira)"*.

---

## 3. Padrão de Repositório (`Repositories`)
Seguindo o princípio de separação de responsabilidades (Repository Pattern), toda a lógica complexa de busca e salvamento no banco de dados não fica no Controlador.

**Exemplo: `ConsultaRepository.cs`**
```csharp
public async Task<IEnumerable<Consulta>> GetAllAsync()
{
    return await _context.Consultas
        .Include(c => c.Paciente)
        .Include(c => c.Profissional)
        .ToListAsync();
}

public async Task<bool> ExisteConflitoHorarioAsync(int profissionalId, int pacienteId, DateTime dataHora)
{
    return await _context.Consultas.AnyAsync(c => 
        (c.ProfissionalId == profissionalId || c.PacienteId == pacienteId) && 
        c.DataHora == dataHora && 
        c.Status != StatusConsulta.Cancelada);
}
```

> [!TIP]
> **Explicação do Código:**
> * **`Include()`:** Conhecido como carregamento antecipado (*Eager Loading*). Quando listamos todas as consultas, o comando diz para o ORM preencher os objetos completos de `Paciente` e `Profissional` utilizando tabelas adjacentes (Joins automáticos), evitando expor apenas seus IDs vazios.
> * **`ExisteConflitoHorarioAsync`:** Trata-se da implementação vital de regra de negócio do sistema. O banco confere a existência de qualquer consulta já registrada com os dados informados (ou para o mesmo médico OU paciente), no mesmo momento específico (`DataHora`). Esta regra inteligente também ignora possíveis consultas que já ostentam o status `Cancelada`.

---

## 4. Camada de Controladores (`Controllers`)
Os Controladores orquestram o tráfego e requisições Web. Eles recebem o pedido (via URL Restful), acionam as validações do Repositório e devolvem os respectivos HTTP Status codes da Web.

**Exemplo Principal: `ConsultasController.cs`**
```csharp
[HttpPost]
public async Task<ActionResult<Consulta>> PostConsulta(Consulta consulta)
{
    bool conflito = await _repository.ExisteConflitoHorarioAsync(consulta.ProfissionalId, consulta.PacienteId, consulta.DataHora);
    
    if (conflito) {
        return BadRequest("Conflito de horário! O profissional ou paciente já possui consulta agendada...");
    }

    await _repository.AddAsync(consulta);
    return CreatedAtAction(nameof(GetConsulta), new { id = consulta.Id }, consulta);
}
```

> [!NOTE]
> **Explicação do Código:**
> * **`[HttpPost]`:** Verbo semântico que determina a rota para **Criar/Adicionar** dados. 
> * **Fluxo de Validação:** A grande vantagem destas camadas fica clara aqui. Antes de aprovar e salvar o agendamento (`AddAsync`), o código verifica se o validador de regra de negócio aponta conflito. Caso encontre um choque de agenda, a criação é sumariamente bloqueada, devolvendo ao frontend do usuário um HTTP Error code `400 (Bad Request)` e a mensagem explícita. Se tudo fluir positivamente, um HTTP Success code `201 (Created)` será disparado confirmando o salvamento.

---

## 5. Configuração / Inicialização (`Program.cs`)
Onde todo o aplicativo web é instanciado, construído e iniciado.

```csharp
// Adiciona conversor de Enum de Texto
builder.Services.AddControllers().AddJsonOptions(options => 
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Configuração do Banco de Dados SQLite Local
builder.Services.AddDbContext<ClinicaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Injeção de Dependências 
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
```

> [!NOTE]
> **Explicação do Código:**
> * **Conversor de JSON (`JsonStringEnumConverter`):** Instruímos nossa API nas conversões de *Enums*, essenciais em campos como `Status`. Se não incluíssemos essa configuração, a API esperaria e devolveria o status "Agendada" como numeral ex.: `0`. O `Converters` permite que a leitura e salvamento possam ser interpretados textualmente.
> * **Injeção de Dependência (`AddScoped`):** Essencial para o escopo e performance arquitetural. Informa a classe injetora nativa do ASP.NET: *"Toda vez que a aplicação exigir no controlador os contratos declarados em `IPacienteRepository`, providencie a instancição transparente da minha classe implementada `PacienteRepository`."*

> [!IMPORTANT]
> **Para defesa ou apresentação do TCC/Projeto:**
> Abordar tecnicamente a separação entre suas camadas lógicas e o uso do calculador automático do campo (Idade) te garantirá aprovação nas disciplinas de *Padrões de Software (Solid, Design Patterns)*. Focar na coesão do repositório em vez da simples injeção direta de banco no Controller também expõe clareza de projeto de longo prazo.
