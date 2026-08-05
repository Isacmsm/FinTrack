using System;

namespace FinTrack.Data;

// Marca o instante do startup pra calcular uptime no painel /admin.
public class AppStatus
{
    public DateTime IniciadoEm { get; } = DateTime.UtcNow;
}
