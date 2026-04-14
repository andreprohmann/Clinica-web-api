# Clínica Web API

## Visão Geral
A **Clínica Web API** é uma interface de programação de aplicações (API) desenvolvida em **C#** utilizando o **ASP.NET Core**. Este projeto foi criado com o objetivo de gerenciar os processos de uma clínica multiprofissional, abrangendo o cadastro de pacientes, profissionais de saúde e o agendamento de consultas. O sistema foi desenvolvido com foco em boas práticas de programação, aplicando conceitos de arquitetura robustos para facilitar a manutenção e escalabilidade.

## Tecnologias e Padrões Utilizados
- **C# / .NET**
- **ASP.NET Core Web API**
- **Entity Framework Core (EF Core)**: Como ORM (Object-Relational Mapper) para manipulação e estruturação do banco de dados.
- **SQLite**: Banco de dados relacional leve e embutido, facilitando o desenvolvimento local, testes e portabilidade acadêmica.
- **Repository Pattern (Padrão de Repositório)**: Isola a camada de acesso a dados da camada de negócios, proporcionando menor acoplamento e maior testabilidade.
- **Injeção de Dependência**: Centralizada no `Program.cs` para gerenciar o ciclo de vida dos repositórios e do contexto do banco (Scoped).
- **Swagger / OpenAPI**: Para documentação interativa e facilidade em testar os endpoints na web.

## Estrutura do Projeto
A arquitetura do projeto foi dividida em camadas lógicas para garantir a separação de responsabilidades (*Separation of Concerns - SoC*):

- **Models (`/Models`)**: Contém as classes que representam as entidades de domínio (Paciente, Profissional, Consulta e Enums). É onde ficam regras de negócios intrínsecas aos objetos, como o cálculo dinâmico da `Idade` na classe `Paciente`.
- **Data (`/Data`)**: Abriga o contexto de dados do Entity Framework (`ClinicaDbContext.cs`). Responsável por mapear as entidades em tabelas do banco de dados utilizando *Fluent API* e *Data Annotations* (Ex.: Relacionando e vinculando consultas e pacientes).
- **Repositories (`/Repositories`)**: Implementa o acesso ao banco de dados com lógicas customizadas. Centraliza consultas Entity complexas e regras críticas de negócio (Ex.: verificar se não existe sobreposição (*conflito*) de horário antes de efetuar o agendamento).
- **Controllers (`/Controllers`)**: Camada de entrada das requisições HTTP REST (`GET`, `POST`, `PUT`, `DELETE`). Direciona o tráfego, orquestrando as validações de negócios com o banco através dos Repositórios e devolvendo *HTTP Status Codes* (Ex.: `201 Created` ou `400 Bad Request`).

## Regras de Negócio e Destaques
1. **Verificação de Conflitos de Horário**: No momento de agendar uma `Consulta` via `POST`, a API aciona o `Repository` para verificar se o Médico ou o Paciente não têm um agendamento no exato mesmo horário no sistema, protegendo a agenda (ignorando consultas marcadas como *Canceladas*).
2. **Propriedades Computadas (Idade Dinâmica)**: A propriedade `Idade` de um paciente não é fixamente salva no banco de dados. Ela é calculada de forma dinâmica apenas na hora em que é solicitada, com base em sua `DataNascimento` frente à data atual.
3. **Conversão Limpa de Enums para JSON**: Uso da configuração JSON `JsonStringEnumConverter` para possibilitar a resposta compreensível da API em formato de texto para os estados (`Agendada`, `Concluida`, `Cancelada`) ao invés de exibir números sequenciais numéricos (0, 1, 2) indesejados no Frontend.
4. **Proteção Circular (*Circular Dependency*)**: Utilização de `[JsonIgnore]` para que na geração de dados Entity complexos as entidades relacionadas não sofram um ciclo infinito que causa a quebra do Payload de retorno.

## Como Executar Localmente

### Pré-requisitos
- [.NET SDK](https://dotnet.microsoft.com/download) instalado.
- Um editor ou IDE como Visual Studio, VS Code, Rider, etc.

### Passos:
1. Navegue pelo terminal até a pasta raiz do projeto (`Clinica-web-api`).
2. Se o projeto estiver sendo rodado pela primeira vez local e o banco estiver ausente ou desatualizado em formato SQLite (*clinica.db*), você pode acionar as migrations via Entity CLI:
   ```bash
   dotnet ef database update
   ```
3. Execute o comando para iniciar e rodar a API:
   ```bash
   dotnet run
   ```
4. O servidor será inicializado (usualmente na porta *5000* e *5001* ou as configuradas em `Properties/launchSettings.json`).
5. Acesse a interface interativa de UI para visualização e testes dos endpoints pelo seu navegador adicionando `/swagger` à sua URL de debug:
   `http://localhost:<porta>/swagger`

## Documentação Adicional
Preparamos um arquivo detalhando as escolhas minuciosas de engenharia projetada. 
Consulte fortemente o arquivo auxiliar: [`explicacao_arquitetura.md`](explicacao_arquitetura.md) presente na raiz da hierarquia do projeto que aprofunda nos conceitos e detalhamentos essenciais da lógica idealizadas e direcionadas para apresentação e defesas acadêmicas de TCC e Trabalhos na universidade.
