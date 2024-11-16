using Microsoft.AspNetCore.Mvc.Diagnostics;
using Play.Catalog.Service.Entites;

namespace Play.Catalog.Service.Extension
{
    public static class Extension
    {
        public static ItemDto AsDto(this Item item)
        {
            return new ItemDto(item.Id, item.Name, item.Description, item.Price, item.CreateDate);
        }
    }
}