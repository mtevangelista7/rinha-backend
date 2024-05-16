﻿using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using rinha_backend.models;
using rinha_backend.Models;
using rinha_backend.repository;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace rinha_backend.endpoints
{
    public static class ClienteEndpoint
    {
        // isso aqui funciona só por conta da rinha se mudar um já era
        private static readonly int[] limite = [0, 100000, 80000, 1000000, 10000000, 500000];

        public static void MapClienteEndpoint(this WebApplication app)
        {
            app.MapPost("clientes/{id:int}/transacoes",
                async (HttpContext context, int id) => await AdicionaTransacao(context, id));
            app.MapGet("clientes/{id:int}/extrato",
                async (int id) => await RecuperaExtrato(id));
        }

        private static async Task<IResult> AdicionaTransacao(HttpContext context, int id)
        {
            try
            {
                RinhaRepository rinhaRepository = new();

                if (!await rinhaRepository.ClienteExiste(id))
                {
                    return Results.NotFound("Não achou o cliente");
                }

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

                if (string.IsNullOrWhiteSpace(transacao.Descricao) || transacao.Descricao.Length > 10)
                {
                    return Results.UnprocessableEntity();
                }

                if (!transacao.Tipo.Equals('c') && !transacao.Tipo.Equals('d'))
                {
                    return Results.UnprocessableEntity();
                }

                var result = await rinhaRepository.RealizaOperacao(id, transacao);
                result.Limite = limite[id];
                return Results.Ok(result);
            }
            catch (Exception exception)
            {
                return Results.UnprocessableEntity(exception.Message);
            }
        }

        private static async Task<IResult> RecuperaExtrato(int id)
        {
            RinhaRepository rinhaRepository = new();

            if (!await rinhaRepository.ClienteExiste(id))
            {
                return Results.NotFound($"Não achou o cliente {id}");
            }

            var extrato = await rinhaRepository.RetornaExtrato(id);

            return Results.Ok(extrato);
        }
    }
}