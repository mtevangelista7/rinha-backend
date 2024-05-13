using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using rinha_backend.models;
using rinha_backend.Models;
using rinha_backend.repository;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace rinha_backend.endpoints
{
    public static class ClienteEndpoint
    {
        public static void MapClienteEndpoint(this WebApplication app)
        {
            app.MapPost("clientes/{id}/transacoes", async (HttpContext context, int id) => await AdicionaTransacao(context, id));
        }

        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)")]
        [RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)")]
        private static async Task<IResult> AdicionaTransacao(HttpContext context, int id)
        {
            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();

            Transacao transacao;

            try
            {
                transacao = JsonSerializer.Deserialize<Transacao>(body) ?? throw new Exception();
            }
            catch (Exception)
            {
                return Results.UnprocessableEntity();
            }

            if (transacao.Valor < 0)
            {
                return Results.UnprocessableEntity();
            }

            if (String.IsNullOrWhiteSpace(transacao.Descricao) || transacao.Descricao.Length > 10)
            {
                return Results.UnprocessableEntity();
            }

            if (transacao.Tipo != 'c' || transacao.Tipo != 'd')
            {
                return Results.UnprocessableEntity();
            }

            ClienteRepository clienteRepository = new("");
            var result = await clienteRepository.RealizaTransacao(id);

            return Results.Ok(result);
        }
    }
}
