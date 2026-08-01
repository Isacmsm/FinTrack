# 💰 FinTrack

**Controle financeiro pessoal completo — receitas, despesas, investimentos e muito mais.**

<p align="center">
  <img src="wwwroot/images/carteira.png" alt="FinTrack" width="120" />
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet" alt=".NET 10" />
  <img src="https://img.shields.io/badge/EF%20Core-10-512BD4?logo=dotnet" alt="EF Core 10" />
  <img src="https://img.shields.io/badge/SQL%20Server-2025%20(Docker)-CC2927?logo=microsoftsqlserver" alt="SQL Server" />
  <img src="https://img.shields.io/badge/Bootstrap-5.3-7952B3?logo=bootstrap" alt="Bootstrap 5.3" />
  <img src="https://img.shields.io/badge/jQuery-3.x-0769AD?logo=jquery" alt="jQuery" />
  <img src="https://img.shields.io/badge/Status-Em%20Desenvolvimento-yellow" alt="Status" />
</p>

---

## 📋 Sobre o Projeto

O FinTrack é um aplicativo web de controle financeiro pessoal que permite ao usuário ter uma visão completa das suas finanças: receitas, despesas, investimentos, dívidas, cartões de crédito e muito mais.

O projeto também serve como laboratório de estudo para a transição do **.NET Framework 4.8** para o **.NET 10**, aplicando boas práticas e padrões modernos do ASP.NET Core.

---

## 🛠️ Stack Técnica

| Camada | Tecnologia |
|--------|-----------|
| **Back-end** | .NET 10 · ASP.NET Core Razor Pages (single-file) |
| **Banco de dados** | SQL Server 2025 (Docker) |
| **Acesso a dados** | Entity Framework Core 10 · Migrations |
| **Autenticação** | Sessão + `IPasswordHasher` (PBKDF2). Sem ASP.NET Identity |
| **Front-end** | Bootstrap 5.3 (CDN) · jQuery · AJAX |
| **Fontes** | Outfit (títulos) · Plus Jakarta Sans (corpo) |
| **Paleta** | Tons de verde/esmeralda |

> O projeto **não tem passo de build de front-end**: o Bootstrap vem por CDN, então não há `npm`,
> `node_modules` nem CSS compilado. Rodar é `dotnet run`.

---

## ✅ Funcionalidades e Status

### 🟢 Implementado

| Funcionalidade | Descrição |
|----------------|-----------|
| **Landing Page** | Página inicial com seções de funcionalidades, CTA e design responsivo |
| **Login** | Autenticação via AJAX, sessão, mensagens de erro por campo |
| **Registro de usuário** | Criação de conta com validação de nome, e-mail e senha |
| **Hash de senha** | `IPasswordHasher` (PBKDF2, salt por usuário). Contas antigas em texto puro são convertidas automaticamente no primeiro login |
| **Verificação de sessão** | `VerificaSessaoFilter` protege toda a pasta `/App`, redirecionando quem não está logado |
| **Layout autenticado** | Header + sidebar com 6 grupos de menu colapsáveis e dropdown de perfil, via componentes do Bootstrap (sem JS próprio) |
| **Layout de Acesso** | Layout compartilhado para login/registro com glass-morphism e animações |
| **EF Core + Migrations** | DbContext e entidades mapeadas para as 6 tabelas; schema versionado em migrations |
| **Validação em C#** | Classe `Validador`, que acumula erros e devolve um por campo |
| **Utilitários JS** | `mostrarErros()` e `limparErros()` em `site.js` |
| **Util.cs** | `ExecutarHandler` / `ExecutarHandlerAsync` para padronizar o try/catch dos handlers |
| **Estrutura do Banco** | 6 tabelas com FKs (Usuario, Categoria, Transacao, Recorrente, Divida, Banco) |

### 🟡 Em Desenvolvimento

| Funcionalidade | Descrição |
|----------------|-----------|
| **Dashboard** | A página `/App` existe, mas por enquanto é um placeholder — sem gráficos ou dados |
| **Navegação da sidebar** | Os itens de menu já estão montados, mas as páginas de destino ainda não existem |
| **Logout** | O link `/ControleAcesso/Sair` está no menu de perfil, a página ainda não foi criada |

### 🔴 Planejado

#### Core
| Funcionalidade | Descrição |
|----------------|-----------|
| **Dashboard** | Painel principal com gráficos de gastos, receitas, saldo e evolução mensal |
| **Transações** | Tela para cadastro e listagem de receitas e despesas com filtros |
| **Categorias** | CRUD de categorias personalizáveis (alimentação, transporte, lazer, etc.) |

#### Planejamento e Controle
| Funcionalidade | Descrição |
|----------------|-----------|
| **Orçamento Mensal** | Planejar gastos de cada mês com metas por categoria |
| **Metas de Economia** | Definir metas de economia com barra de progresso |
| **Visão Geral** | Situação financeira atual (saldo, gastos do mês, receitas) |
| **Gastos Recorrentes** | Controle de assinaturas e parcelas com geração automática de transações via background service |

#### Cartão de Crédito
| Funcionalidade | Descrição |
|----------------|-----------|
| **Gerência de Cartões** | Cadastro de cartões com limite, bandeira e dia de vencimento |
| **Gastos no Cartão** | Registro de compras no crédito e controle de fatura |
| **Parcelas** | Acompanhamento de compras parceladas e impacto nas faturas futuras |

#### Dívidas
| Funcionalidade | Descrição |
|----------------|-----------|
| **Controle de Dívidas** | Cadastro de dívidas com valor total, parcelas, taxa de juros e banco |
| **Vínculo com Recorrentes** | Ao marcar uma dívida como "pagando", cria automaticamente um gasto recorrente |
| **Histórico** | Registro de dívidas quitadas com data de quitação e valores pagos |

#### Investimentos
| Funcionalidade | Descrição |
|----------------|-----------|
| **Controle de Investimentos** | Registro de aplicações (Tesouro Selic, CDB, etc.) com valor, data e rentabilidade |

#### Simuladores
| Funcionalidade | Descrição |
|----------------|-----------|
| **Salário-hora** | Calcula quanto você ganha por hora trabalhada |
| **À vista vs A prazo** | Compara o custo real de uma compra à vista ou parcelada |
| **Comprar vs Alugar** | Simula se vale mais a pena comprar ou alugar um imóvel |
| **Liberdade Financeira** | Calcula quanto você precisa investir para atingir sua meta |

#### Relatórios e Alertas
| Funcionalidade | Descrição |
|----------------|-----------|
| **DRE Pessoal** | Demonstrativo de resultados anual com detalhamento mensal |
| **Comparativo Mensal** | Comparar gastos entre meses diferentes |
| **Relatórios por Período** | Filtros por mensal, trimestral, anual e personalizado |
| **Alertas** | Notificações de gastos excessivos por categoria |

#### Extras
| Funcionalidade | Descrição |
|----------------|-----------|
| **Fluxo de Caixa Futuro** | Projeção dos próximos meses considerando recorrentes e parcelas |
| **Tags/Etiquetas** | Sistema de tags para transações além das categorias |
| **Importação CSV** | Importar extratos bancários |

---

## 🗄️ Banco de Dados

### Diagrama de Tabelas

```
Usuario (1) ──── (N) Transacao (N) ──── (1) Categoria
   │                                          │
   ├──────────── (N) Recorrente (N) ──────────┘
   │                    │
   ├──────────── (N) Divida
   │                    │
   │                    └──── (1) Banco
   │                    └──── (1) Recorrente
   │
   └──────────── (N) Categoria (categorias personalizadas)
```

### Tabelas
- **Usuario** — Dados de autenticação (nome, e-mail, senha com hash PBKDF2)
- **Categoria** — Tipos de receita/despesa (IdUser NULL = categoria padrão do sistema)
- **Transacao** — Registros de receitas e despesas
- **Recorrente** — Receitas/despesas que se repetem todo mês (salário, assinaturas)
- **Divida** — Controle de dívidas com vínculo automático a recorrentes
- **Banco** — Cadastro de bancos e instituições financeiras

### Migrations

O schema é versionado por EF Core migrations:

```bash
dotnet ef migrations add NomeDaMudanca   # depois de alterar uma entidade
dotnet ef database update                # aplica no banco
dotnet ef migrations list                # o que já foi aplicado
```

A migration `InitialCreate` é um **baseline**: foi gerada a partir do banco que já existia e
registrada em `__EFMigrationsHistory` sem recriar as tabelas.

### Stored Procedures (removidas)

O projeto nasceu com 24 stored procedures (`FT_[Área]_[Ação]`, com `BEGIN TRY/CATCH` e RAISERROR
50001 em JSON), no padrão da Log Tecnologia. Elas foram **dropadas do banco e removidas do
repositório** — o CRUD é LINQ sobre o DbContext e a validação de input vive no `Validador`.

O código original continua no histórico do git, se algum dia servir de referência:

```bash
git show 05b5594:"Banco e Procedures/BancoTabelaProcedures.sql"
```

---

## 🚀 Como Rodar

### Pré-requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://docs.docker.com/engine/install/) (para o SQL Server)
- `dotnet-ef` — instale com `dotnet tool install -g dotnet-ef`

### Instalação

```bash
# Clone o repositório
git clone https://github.com/Isacmsm/FinTrack.git
cd FinTrack

# Suba o SQL Server (primeira vez: cria o container)
docker run -d --name sqlserver \
  -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=SuaSenhaForte123" \
  -p 1433:1433 -v sqldata:/var/opt/mssql \
  mcr.microsoft.com/mssql/server:2025-latest

# Nas próximas vezes, basta ligar:
# docker start sqlserver

# Configure a connection string (veja abaixo)

# Crie o banco a partir das migrations
dotnet ef database update

# Rode a aplicação
dotnet run
```

Aguarde uns 15-20 segundos após o `docker start` — o SQL Server demora a aceitar conexões mesmo
depois de o container aparecer como *up*. Para acompanhar: `docker logs -f sqlserver`.

### Configuração

O `appsettings.json` tem apenas um **placeholder** de connection string e não deve ser editado.
Crie o `appsettings.Development.json` (fora do versionamento) com as credenciais reais:

```json
{
  "ConnectionStrings": {
    "FinTrack": "Server=localhost;Database=FinTrack;User Id=SA;Password=SuaSenhaForte123;TrustServerCertificate=True;"
  }
}
```

> O `Properties/launchSettings.json` define `ASPNETCORE_ENVIRONMENT=Development`, que é o que faz o
> app carregar esse arquivo. Sem isso ele sobe como Production, lê o placeholder e o login falha com
> `Login failed for user 'YOUR_USER'`.

---

## 📁 Estrutura do Projeto

```
FinTrack/
├── wwwroot/
│   ├── images/                       # Ícones PNG (256x256)
│   ├── js/site.js                    # mostrarErros() e limparErros()
│   └── lib/jquery/
├── Models/                           # Entidades do EF Core
│   ├── Usuario.cs
│   ├── Categoria.cs
│   ├── Transacao.cs
│   ├── Recorrente.cs
│   ├── Divida.cs
│   └── Banco.cs
├── Data/
│   ├── FinTrackDbContext.cs          # DbContext e mapeamentos
│   ├── ErroValidacaoException.cs     # Validador + exceção de validação
│   └── Util.cs                       # ExecutarHandler / ExecutarHandlerAsync
├── Migrations/                       # Schema versionado
├── Filters/
│   └── VerificaSessaoFilter.cs       # Protege a pasta /App
├── Pages/
│   ├── Index.cshtml                  # Landing page
│   ├── Error.cshtml
│   ├── Shared/_Layout.cshtml         # Layout genérico
│   ├── ControleAcesso/
│   │   ├── Index.cshtml              # Login
│   │   ├── Registro.cshtml
│   │   └── _LayoutAcesso.cshtml      # Layout de login/registro
│   └── App/
│       ├── Index.cshtml              # Dashboard (placeholder)
│       └── _Layout.cshtml            # Header + sidebar
├── Properties/launchSettings.json
├── Program.cs
├── appsettings.json                  # Placeholder — não editar
└── README.md
```

---

## 🧩 Padrão de Handler

Lógica em `@functions { }` dentro do `.cshtml`, sem code-behind:

```csharp
public Task<IActionResult> OnPostNomeAsync([FromBody] Request request)
{
    return Util.ExecutarHandlerAsync(async () =>
    {
        new Validador()
            .Exigir(!string.IsNullOrWhiteSpace(request.Campo), "campo", "Informe o campo")
            .LancarSeInvalido();

        // ... lógica com o EF
        await Db.SaveChangesAsync();

        return new JsonResult(new { sucesso = true }) { StatusCode = 200 };
    });
}
```

O contrato de erro consumido pelo `site.js`:

| Situação | Resposta |
|----------|----------|
| Erro de campo | `400` · `{"errosInput":[{"nomeInput":"email","mensagem":"..."}]}` |
| Erro geral | `400` · `{"mensagem":"..."}` |
| Sucesso | `200` · `{"sucesso":true}` |

---

## 📸 Screenshots

> Em breve

---

## 👤 Autor

**Isac Macedo**

---

> Projeto desenvolvido como laboratório de aprendizado para .NET 10 e ASP.NET Core moderno.
