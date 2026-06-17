using IndustryTrade.BuildingBlocks.Application.Specifications;
using Microsoft.EntityFrameworkCore;

namespace IndustryTrade.BuildingBlocks.Infrastructure.Persistence;

/// <summary>Translates a <see cref="Specification{T}"/> into an EF Core <see cref="IQueryable{T}"/>.</summary>
public static class SpecificationEvaluator
{
    public static IQueryable<T> Apply<T>(IQueryable<T> query, Specification<T> spec)
        where T : class
    {
        if (spec.Criteria is not null)
            query = query.Where(spec.Criteria);

        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

        if (spec.OrderBy is not null)
            query = query.OrderBy(spec.OrderBy);
        else if (spec.OrderByDescending is not null)
            query = query.OrderByDescending(spec.OrderByDescending);

        if (spec.IsPagingEnabled)
            query = query.Skip(spec.Skip ?? 0).Take(spec.Take ?? 10);

        return query;
    }
}
