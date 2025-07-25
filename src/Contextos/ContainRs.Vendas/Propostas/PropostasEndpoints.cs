﻿using ContainRs.Contracts;
using ContainRs.Vendas.Locacoes;
using Microsoft.AspNetCore.Mvc;
using System.Transactions;

namespace ContainRs.Vendas.Propostas;

public static class PropostasEndpoints
{
    public const string ENDPOINT_NAME_GET_PROPOSTA = "GetProposta";

    public static IEndpointRouteBuilder MapPropostasEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup(EndpointConstants.ROUTE_PEDIDOS)
            .WithTags(EndpointConstants.TAG_LOCACAO)
            .WithOpenApi();

        group
            .MapPostProposta()
            .MapGetPropostas()
            .MapGetPropostaById()
            .MapPatchAcceptProposta()
            .MapPatchRejectProposta()
            .MapPostComentarioProposta();

        return builder;
    }

    public static RouteGroupBuilder MapPostProposta(this RouteGroupBuilder builder)
    {
        builder
            .MapPost("{id:guid}/proposals", async (
                [FromRoute] Guid id,
                [FromForm] PropostaRequest request,
                [FromServices] IRepository<PedidoLocacao> repoSolicitacao,
                [FromServices] IRepository<Proposta> repoProposta
                ) =>
            {
                // salvar arquivo no sistema de arquivos configurado (não será feito neste curso)

                var solicitacao = await repoSolicitacao
                    .GetFirstAsync(s => s.Id == id, s => s.Id);
                if (solicitacao is null) return Results.NotFound();

                var proposta = new Proposta()
                {
                    Id = Guid.NewGuid(),
                    ClienteId = solicitacao.ClienteId,
                    ValorTotal = new ValorMonetario(request.ValorTotal),
                    DataCriacao = DateTime.Now,
                    DataExpiracao = request.DataExpiracao,
                    NomeArquivo = request?.Arquivo?.FileName == null ? "valor teste" : request?.Arquivo?.FileName,
                    SolicitacaoId = solicitacao.Id
                };

                await repoProposta.AddAsync(proposta);

                return Results.CreatedAtRoute(
                    ENDPOINT_NAME_GET_PROPOSTA,
                    new { Id = proposta.SolicitacaoId, PropostaId = proposta.Id },
                    PropostaResponse.From(proposta));
            })
            // deveria ser Comercial, mas para não criarmos usuários com papéis diferentes, usaremos o papel (role) Suporte
            .RequireAuthorization(policy => policy.RequireRole("Suporte"))
            .DisableAntiforgery()
            .WithSummary("Vendedor envia proposta de locação")
            .Produces(StatusCodes.Status404NotFound)
            .Produces<PropostaResponse>(StatusCodes.Status201Created);
        return builder;
    }

    public static RouteGroupBuilder MapGetPropostaById(this RouteGroupBuilder builder)
    {
        builder.MapGet("{id:guid}/proposals/{propostaId:guid}", async (
            [FromRoute] Guid id,
            [FromRoute] Guid propostaId,
            [FromServices] IHttpContextAccessor accessor,
            [FromServices] IRepository<Proposta> repository) =>
        {
            var clienteId = accessor.HttpContext?.User.Claims
                .Where(c => c.Type.Equals("ClienteId"))
                .Select(c => c.Value)
                .FirstOrDefault();

            if (clienteId is null) return Results.Unauthorized();

            var proposta = await repository
                .GetFirstAsync(
                    p => p.Id == propostaId 
                        && p.SolicitacaoId == id
                        && p.ClienteId == Guid.Parse(clienteId),
                    p => p.Id);
            if (proposta is null) return Results.NotFound();

            return Results.Ok(PropostaResponse.From(proposta));
        })
        .WithName(ENDPOINT_NAME_GET_PROPOSTA)
        .RequireAuthorization(policy => policy.RequireRole("Cliente"))
        .WithSummary("Cliente consulta detalhes de uma proposta de locação")
        .Produces(StatusCodes.Status404NotFound)
        .Produces<PropostaResponse>(StatusCodes.Status200OK);

        return builder;
    }

    public static RouteGroupBuilder MapGetPropostas(this RouteGroupBuilder builder)
    {
        builder.MapGet("{id:guid}/proposals", async (
            [FromRoute] Guid id,
            [FromServices] IHttpContextAccessor accessor,
            [FromServices] IRepository<PedidoLocacao> repository) =>
        {
            var clienteId = accessor.HttpContext?.User.Claims
                .Where(c => c.Type.Equals("ClienteId"))
                .Select(c => c.Value)
                .FirstOrDefault();

            if (clienteId is null) return Results.Unauthorized();

            var solicitacao = await repository
                .GetFirstAsync(
                    s => s.Id == id && s.ClienteId == Guid.Parse(clienteId),
                    s => s.Id);
            if (solicitacao is null) return Results.NotFound();

            return Results.Ok(solicitacao.Propostas.Select(p => PropostaResponse.From(p)));
        })
        .RequireAuthorization(policy => policy.RequireRole("Cliente"))
        .WithSummary("Cliente consulta as propostas para uma solicitação de locação")
        .Produces(StatusCodes.Status404NotFound)
        .Produces<IEnumerable<PropostaResponse>>(StatusCodes.Status200OK);

        return builder;
    }

    public static RouteGroupBuilder MapPatchAcceptProposta(this RouteGroupBuilder builder)
    {
        builder.MapPatch("{id:guid}/proposals/{propostaId:guid}/accept", async (
            [FromRoute] Guid id,
            [FromRoute] Guid propostaId,
            [FromServices] IPropostaService propostaService) =>
        {

            var casoUso = new AprovarProposta(id, propostaId);
            var proposta = await propostaService.AprovarAsync(casoUso);

            if (proposta is null) return Results.NotFound();

            return Results.Ok(PropostaResponse.From(proposta));
        })
        .WithSummary("Cliente aceita proposta de locação.")
        .RequireAuthorization(policy => policy.RequireRole("Cliente"))
        .Produces(StatusCodes.Status404NotFound)
        .Produces<PropostaResponse>(StatusCodes.Status200OK);

        return builder;
    }

    public static RouteGroupBuilder MapPatchRejectProposta(this RouteGroupBuilder builder)
    {
        builder.MapPatch("{id:guid}/proposals/{propostaId:guid}/reject", async (
            [FromRoute] Guid id,
            [FromRoute] Guid propostaId,
            [FromServices] IRepository<Proposta> repository) =>
        {

            var proposta = await repository
                .GetFirstAsync(
                    p => p.Id == propostaId && p.SolicitacaoId == id,
                    p => p.Id);
            if (proposta is null) return Results.NotFound();

            proposta.Situacao = SituacaoProposta.Recusada;
            await repository.UpdateAsync(proposta);

            return Results.Ok(PropostaResponse.From(proposta));
        })
        .WithSummary("Cliente rejeita proposta de locação.")
        .RequireAuthorization(policy => policy.RequireRole("Cliente"))
        .Produces(StatusCodes.Status404NotFound)
        .Produces<PropostaResponse>(StatusCodes.Status200OK);

        return builder;
    }

    public static RouteGroupBuilder MapPostComentarioProposta(this RouteGroupBuilder builder)
    {
        builder.MapPost("{id:guid}/proposals/{propostaId:guid}/comment", async (
            [FromRoute] Guid id,
            [FromRoute] Guid propostaId,
            [FromBody] ComentarioRequest request,
            HttpContext context,
            [FromServices] IPropostaService propostaService) =>
        {

            string? quem = context.User.Identity?.Name;
            if (quem is null) return Results.Unauthorized();

            var casoUso = new ComentarProposta(id, propostaId, request.Comentario, quem);
            var proposta = await propostaService.ComentarAsync(casoUso);

            if (proposta is null) return Results.NotFound();

            return Results.Ok(PropostaResponse.From(proposta));
        })
        .WithSummary("Vendedor/Cliente comenta proposta.")
        .RequireAuthorization(policy => policy.RequireRole("Cliente", "Suporte"))
        .Produces(StatusCodes.Status404NotFound)
        .Produces<PropostaResponse>(StatusCodes.Status200OK);

        return builder;
    }
}
