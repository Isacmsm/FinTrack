using System;

namespace FinTrack.Models;

// Login de operador (painel /admin) é intencionalmente separado do Usuario:
// comprometer uma conta de usuário do app nunca deve dar acesso a logs e
// atividade de todo mundo. Sem CRUD/registro — a primeira conta nasce do
// bootstrap em Program.cs (ver seção Admin do appsettings).
public partial class Operador
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Senha { get; set; } = null!;

    public DateTime CriadoEm { get; set; }
}
