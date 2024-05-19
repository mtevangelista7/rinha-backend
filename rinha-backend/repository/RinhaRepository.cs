using System.Text;
using System.Text.Json;
using Npgsql;
using rinha_backend.models;
using rinha_backend.Models;

namespace rinha_backend.repository
{
    public static class RinhaRepository
    {
        public static async Task<RespostaTransacao> RealizaOperacao(int idCliente, Transacao transacao)
        {
            await using var dataSource = NpgsqlDataSource.Create(lazy.connectionString);
            
            var function = transacao.Tipo == 'c' ? "creditar" : "debitar";
            
            await using var cmd = dataSource.CreateCommand($"SELECT {function}(@idCliente, @valor, @descricao)");

            cmd.Parameters.AddWithValue("@idCliente", idCliente);
            cmd.Parameters.AddWithValue("@valor", transacao.Valor);
            cmd.Parameters.AddWithValue("@descricao", transacao.Descricao);

            await using var reader = await cmd.ExecuteReaderAsync();
            
            var transacaoCliente = new RespostaTransacao();

            if (!await reader.ReadAsync())
            {
                transacaoCliente.Saldo = reader.GetInt32(0);
            }

            return transacaoCliente;
        }

        public static async Task<Extrato> RetornaExtrato(int id)
        {
            await using var dataSource = NpgsqlDataSource.Create(lazy.connectionString);
            var sb = new StringBuilder();

            sb.Append(" SELECT * FROM RetornaExtrato(@id)");

            await using var cmd = dataSource.CreateCommand(sb.ToString());

            cmd.Parameters.AddWithValue("@id", id);

            await using var reader = await cmd.ExecuteReaderAsync();

            var extrato = new Extrato
            {
                Saldo = new Saldo()
            };

            while (await reader.ReadAsync())
            {
                var transacao = new Transacao
                {
                    Valor = reader.GetInt32(0),
                    Tipo = reader.GetChar(1),
                    Descricao = reader.GetString(2),
                    RealizadoEm = reader.GetDateTime(3)
                };

                extrato.Saldo.Total = reader.GetInt32(4);
                extrato.Saldo.Limite = reader.GetInt32(5);

                extrato.UltimasTransacoes.Add(transacao);
            }

            extrato.Saldo.DataExtrato = DateTime.Now;

            return extrato;
        }
    }
}