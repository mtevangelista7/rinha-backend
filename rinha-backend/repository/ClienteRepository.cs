using Npgsql;
using rinha_backend.models;

namespace rinha_backend.repository
{
    public class ClienteRepository(string connectionString)
    {
        public async Task<TransacaoCliente> RealizaTransacao(int id)
        {
            await using var dataSource = NpgsqlDataSource.Create(connectionString);

            await using var cmd = dataSource.CreateCommand("SELECT MINHA PROCEDURE AQUI ($1, $2)");
            cmd.Parameters.AddWithValue("param");
            cmd.Parameters.AddWithValue("param2");

            await using var reader = await cmd.ExecuteReaderAsync();

            TransacaoCliente transacaoCliente = new TransacaoCliente();

            if (!await reader.ReadAsync())
            {
                transacaoCliente.Saldo = reader.GetInt32(0);
                transacaoCliente.Limite = reader.GetInt32(1);
            }

            return transacaoCliente;
        }
    }
}
