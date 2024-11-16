using System;
namespace Play.Catalog.Contracts
{
    public record CatalogItemCrated(Guid ItemId,string Name,string Description);
    public record CatalogItemUpdated(Guid ItemId,string Name,string Description);
    public record CatalogItemDeleted(Guid ItemId);
    
}