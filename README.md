# FitManager

Sistema web para gestão de academias. Centraliza o cadastro de alunos, planos e instrutores, o controle de
matrículas e mensalidades e a montagem das fichas de treino, com acesso separado para a recepção e para os
instrutores.

## Funcionalidades

| Módulo | Perfil | O que faz |
|---|---|---|
| Alunos | Administrador | Cadastro completo, pesquisa por nome ou CPF (com validação de CPF) e paginação |
| Planos | Administrador | Cadastro de planos com duração e valor mensal; plano já utilizado é inativado, preservando o histórico |
| Matrículas | Administrador | Matrícula de aluno em plano, com cálculo automático do término; cancelamento |
| Pagamentos | Administrador | Registro de mensalidades, histórico e mensalidades em aberto |
| Instrutores | Administrador | Cadastro de instrutores, que já cria o login de acesso de cada um |
| Fichas de treino | Instrutor | Montagem das fichas com exercícios, séries, repetições, carga e descanso |
| Minha conta | Todos | Troca do e-mail de acesso e da senha |

Outros destaques:

- **Painel inicial** por perfil: indicadores, mensalidades em aberto, matrículas a vencer e alunos sem ficha de treino.
- **Formas de pagamento:** dinheiro, Pix, cartão de débito, cartão de crédito, boleto, cheque e **GymPass**, oferecido apenas nos planos marcados como "Aceita GymPass".
- **Segurança:** login com ASP.NET Core Identity, perfis de acesso, proteção contra CSRF e bloqueio após tentativas inválidas.
- **Interface** responsiva em português, com máscaras de CPF e telefone e confirmação antes de exclusões.

## Tecnologias

| Camada | Tecnologia |
|---|---|
| Linguagem / plataforma | C# com .NET 10 |
| Back-end | ASP.NET Core MVC |
| Front-end | Razor Views, HTML, CSS e Bootstrap 5 |
| Acesso a dados | Entity Framework Core (migrations) |
| Banco de dados | Microsoft SQL Server |
| Autenticação | ASP.NET Core Identity |

## Como executar

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (Express, Developer ou LocalDB)

### Configurar a conexão com o banco

A connection string fica em `FitManager/appsettings.json`, na chave `ConnectionStrings:DefaultConnection`.
O padrão é o SQL Server Express. Troque apenas o trecho `Server=` conforme a sua instalação:

| Instalação | Valor de `Server=` |
|---|---|
| SQL Server Express (padrão do projeto) | `localhost\\SQLEXPRESS` |
| SQL Server Developer (instância padrão) | `localhost` |
| LocalDB (instalado com o Visual Studio) | `(localdb)\\MSSQLLocalDB` |

### Executar

```
cd FitManager
dotnet run
```

Acesse http://localhost:5080. Na primeira execução, o sistema cria o banco `FitManager`, aplica as migrations,
grava os dados iniciais (instrutores e exercícios) e cria os usuários de demonstração.

### Usuários de demonstração

| Perfil | E-mail | Senha |
|---|---|---|
| Administrador | admin@fitmanager.com | Admin@123 |
| Instrutor | instrutor@fitmanager.com | Instrutor@123 |

Os usuários de demonstração são criados apenas quando o banco ainda não tem nenhum usuário.

### Alterações no modelo de dados

Ao mudar uma entidade, gere uma nova migration (ela é aplicada automaticamente na próxima execução):

```
dotnet tool restore
cd FitManager
dotnet ef migrations add NomeDaAlteracao
```

## Regras de negócio

| Regra | Onde está implementada |
|---|---|
| RN01 – CPF obrigatório, válido e único | `CpfValidoAttribute`, `AlunosController.ValidarRegrasAsync`, índice único em `ALUNO.Cpf` |
| RN02 – Uma matrícula ativa por aluno | `MatriculasController.Create` |
| RN03 – Data de término = início + duração do plano | `Matricula.CalcularDataFim` |
| RN04 – Valor mensal copiado do plano na matrícula | `MatriculasController.Create` |
| RN05 – Aluno com matrícula ativa não é excluído; plano utilizado é inativado | `AlunosController.DeleteConfirmed`, `PlanosController.DeleteConfirmed` |
| RN06 – Pagamento só para matrícula ativa; um pagamento por mês | `PagamentosController.Create`, índice único em `PAGAMENTO` |
| RN07 – Ficha só para aluno com matrícula ativa e com ao menos um exercício | `TreinosController.ValidarAsync` |
| RN08 – GymPass só como forma de pagamento de plano que aceita GymPass | `FormasPagamento.Permitidas`, `PagamentosController.Create` |
| RN09 – CREF e e-mail do instrutor únicos; instrutor com fichas é inativado e tem o acesso bloqueado | `InstrutoresController` |
| RN10 – O instrutor só altera e exclui as fichas que ele elaborou | `TreinosController.PodeAlterarAsync` |

## Estrutura do projeto

```
FitManager/
├── Controllers/    Recebem as requisições e aplicam as regras de negócio
├── Models/         Entidades do domínio
├── ViewModels/     Modelos usados apenas pelas telas
├── Views/          Telas (Razor)
├── Data/           AppDbContext, dados iniciais e inicialização do banco
├── Migrations/     Histórico do esquema do banco (Entity Framework Core)
├── Helpers/        Paginação, CPF e mensagens do Identity em português
├── Validation/     Validação de CPF
├── wwwroot/        CSS e JavaScript
└── Program.cs      Configuração da aplicação
```

Tabelas do banco: `ALUNO`, `PLANO`, `MATRICULA`, `PAGAMENTO`, `INSTRUTOR`, `EXERCICIO`, `TREINO` e
`ITEM_TREINO`, além das tabelas do ASP.NET Core Identity (`AspNet*`).

Bootstrap, Bootstrap Icons e jQuery são carregados por CDN, portanto as telas precisam de acesso à internet.

## Autor

Desenvolvido por **Davi Demiciano Giovani**.

© Davi Demiciano Giovani. Todos os direitos reservados.
