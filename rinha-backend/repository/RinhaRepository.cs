using System.Text;
using Npgsql;
using rinha_backend.models;
using rinha_backend.Models;

namespace rinha_backend.repository
{
    public class RinhaRepository(string connectionString)
    {
        public async Task<RespostaTransacao> RealizaOperacao(int idCliente, Transacao transacao)
        {
            await using var dataSource = NpgsqlDataSource.Create(connectionString);
            var function = transacao.Tipo == 'c' ? "creditar" : "debitar";
            await using var cmd = dataSource.CreateCommand($"SELECT {function}($1, $2, $3)");

            cmd.Parameters.AddWithValue(idCliente);
            cmd.Parameters.AddWithValue(transacao.Valor);
            cmd.Parameters.AddWithValue(transacao.Descricao);

            await using var reader = await cmd.ExecuteReaderAsync();
            var transacaoCliente = new RespostaTransacao();

            if (!await reader.ReadAsync())
            {
                transacaoCliente.Saldo = reader.GetInt32(0);
            }

            return transacaoCliente;
        }

        public async Task<bool> ClienteExiste(int id)
        {
            await using var dataSource = NpgsqlDataSource.Create(connectionString);

            await using var cmd = dataSource.CreateCommand("SELECT * FROM CLIENTES WHERE ID = ($1)");
            cmd.Parameters.AddWithValue(id);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<Extrato> RetornaExtrato(int id)
        {
            await using var dataSource = NpgsqlDataSource.Create(connectionString);
            var sb = new StringBuilder();

            sb.Append(" SELECT t.VALOR, t.TIPO, t.DESCRICAO, t.REALIZADO_EM, c.Total, c.Limite ");
            sb.Append(" FROM Transacao t ");
            sb.Append(" INNER JOIN Clientes c ON t.id_cliente = c.id ");
            sb.Append(" WHERE t.ID_CLIENTE = ($1) ");
            sb.Append(" ORDER BY t.REALIZADA_EM DESC ");
            sb.Append(" LIMIT 10 ");

            await using var cmd = dataSource.CreateCommand(sb.ToString());
            cmd.Parameters.AddWithValue(id);

            await using var reader = await cmd.ExecuteReaderAsync();

            var listTrasacao = new List<Transacao>();

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

                listTrasacao.Add(transacao);
            }

            extrato.Saldo.DataExtrato = DateTime.Now;
            extrato.UltimasTransacoes = listTrasacao;

            return extrato;
        }
    }
}