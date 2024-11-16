using System;
using System.ComponentModel.DataAnnotations;

namespace Play.Catalog.Service
{
 public record ItemDto(Guid Id,string Name,string Description,decimal Price,DateTimeOffset CreateDate);
 public record CreateItemDto([Required]string Name,string Description,[Range(0,1000000)]decimal Price);
 public record UpdateItemDto(Guid Id,string Name,string Description,[Range(0,1000000)]decimal Price);
}