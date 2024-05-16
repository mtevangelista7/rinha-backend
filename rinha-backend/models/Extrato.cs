using rinha_backend.Models;

namespace rinha_backend.models;

public class Extrato
{
    public Saldo Saldo { get; set; }
    public List<Transacao> UltimasTransacoes { get; set; } = new(10);
}