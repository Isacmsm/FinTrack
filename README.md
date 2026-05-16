# 💰 FinTrack

**Controle financeiro pessoal completo — receitas, despesas, investimentos e muito mais.**

<p align="center">
  <img src="wwwroot/images/carteira.png" alt="FinTrack" width="120" />
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet" alt=".NET 10" />
  <img src="https://img.shields.io/badge/SQL%20Server-Express-CC2927?logo=microsoftsqlserver" alt="SQL Server" />
  <img src="https://img.shields.io/badge/Tailwind%20CSS-4.2-06B6D4?logo=tailwindcss" alt="Tailwind CSS" />
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
| **Banco de dados** | SQL Server · Stored Procedures · ADO.NET puro |
| **Acesso a dados** | DaoService com injeção de dependência |
| **Front-end** | Tailwind CSS 4.2 · jQuery · AJAX |
| **Fontes** | Outfit (títulos) · Plus Jakarta Sans (corpo) |
| **Paleta** | Tons de verde/esmeralda |

---

## ✅ Funcionalidades e Status

### 🟢 Implementado

| Funcionalidade | Descrição |
|----------------|-----------|
| **Landing Page** | Página inicial apresentando o FinTrack com seções de funcionalidades, CTA e design responsivo |
| **Login funcional** | Autenticação via AJAX com handler C#, sessão, tratamento de erros de input e sistema |
| **Registro de usuário** | Formulário, handler C# e AJAX para criação de conta (`Pages/ControleAcesso/Registro.cshtml`) |
| **Layout de Acesso** | Layout compartilhado para login/registro (`_LayoutAcesso.cshtml`) |
| **Utilitários JS** | Funções `mostrarErros()` e `limparErros()` reutilizáveis em `site.js` |
| **Util.cs** | Helper `ExecutarHandler` para padronizar try/catch dos handlers |
| **Estrutura do Banco** | Todas as tabelas criadas (Usuario, Categoria, Transacao, Recorrente, Divida, Banco) |
| **Stored Procedures** | CRUD completo para todas as tabelas seguindo o padrão da Log |
| **DaoService** | Classe de acesso a dados com ADO.NET |
| **ErroExecucaoException** | Tratamento de erros de SPs com suporte ao error number 50001 (JSON) |
| **Tailwind CSS** | Configurado com CLI, scripts de build e watch |

### 🟡 Em Desenvolvimento

| Funcionalidade | Descrição |
|----------------|-----------|
| **Verificação de sessão** | Proteção de páginas internas (usuário logado) |

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
- **Usuario** — Dados de autenticação (nome, email, senha hash)
- **Categoria** — Tipos de receita/despesa (IdUser NULL = categoria padrão do sistema)
- **Transacao** — Registros de receitas e despesas
- **Recorrente** — Receitas/despesas que se repetem todo mês (salário, assinaturas)
- **Divida** — Controle de dívidas com vínculo automático a recorrentes
- **Banco** — Cadastro de bancos e instituições financeiras

### Stored Procedures (24 procedures)
Todas seguem o padrão com `BEGIN TRY / BEGIN CATCH`, validação de inputs com error number 50001 (JSON) e validações de sistema com severidade 16.

| Tabela | Sel | Ins | Upt | Del | Extras |
|--------|-----|-----|-----|-----|--------|
| Usuario | — | ✅ | ✅ | ✅ | Login |
| Categoria | ✅ | ✅ | ✅ | ✅ | — |
| Transacao | ✅ | ✅ | ✅ | ✅ | — |
| Recorrente | ✅ | ✅ | ✅ | ✅ | — |
| Divida | ✅ | ✅ | ✅ | ✅ | — |
| Banco | ✅ | ✅ | ✅ | ✅ | — |

---

## 🚀 Como Rodar

### Pré-requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) (Express ou superior)
- [Node.js](https://nodejs.org/) (para o Tailwind CSS)

### Instalação

```bash
# Clone o repositório
git clone https://github.com/seu-usuario/FinTrack.git
cd FinTrack

# Instale as dependências do Tailwind
npm install

# Gere o CSS
npm run css:build

# Configure a connection string em appsettings.Development.json (crie o arquivo se não existir)
# O appsettings.json já tem um placeholder — não editar diretamente

# Execute os scripts SQL para criar as tabelas e procedures

# Rode a aplicação
dotnet run
```

### Configuração do Banco

1. Crie o database `FinTrack` no SQL Server
2. Execute os scripts SQL para criar as tabelas
3. Execute os scripts SQL para criar as stored procedures
4. Configure o `sp_addmessage 50001` para o padrão de erros JSON:
```sql
EXEC sp_addmessage @msgnum = 50001, @severity = 10,
    @msgtext = N'{"NomeInput": "%s", "Mensagem": "%s"}',
    @lang = 'us_english', @replace = 'REPLACE'
```

### Scripts npm

| Comando | Descrição |
|---------|-----------|
| `npm run css:build` | Gera o CSS do Tailwind uma vez |
| `npm run css:watch` | Observa mudanças e regenera o CSS automaticamente |

---

## 📁 Estrutura do Projeto

```
FinTrack/
├── wwwroot/
│   ├── css/
│   │   ├── site.css              # Entrada do Tailwind
│   │   └── output.css            # CSS gerado (não editar)
│   ├── images/                   # Ícones PNG
│   ├── js/
│   └── lib/jquery/
├── Data/
│   ├── DaoService.cs             # Acesso a dados (padrão Log)
│   ├── ErroExecucaoException.cs  # Tratamento de erros de SPs
│   └── Util.cs                   # Helper para handlers (ExecutarHandler)
├── Pages/
│   ├── Shared/
│   │   └── _Layout.cshtml        # Layout principal
│   ├── ControleAcesso/
│   │   ├── Index.cshtml          # Login
│   │   ├── Registro.cshtml       # Registro de usuário
│   │   ├── _LayoutAcesso.cshtml  # Layout compartilhado (login/registro)
│   │   └── _ViewStart.cshtml     # Define layout da pasta
│   ├── Index.cshtml              # Landing page
│   └── Error.cshtml
├── Program.cs
├── appsettings.json
├── package.json
└── README.md
```

---

## 📸 Screenshots

> Em breve

---

## 👤 Autor

**Isac Macedo**

---

> Projeto desenvolvido como laboratório de aprendizado para .NET 10 e ASP.NET Core moderno.
