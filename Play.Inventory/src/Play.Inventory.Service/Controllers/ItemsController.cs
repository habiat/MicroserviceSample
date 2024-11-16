using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<InventoryItem> _inventoryItemRepository;
        private readonly CatalogClients _catalogClients;
        public ItemsController(IRepository<InventoryItem> inventoryItemRepository, CatalogClients catalogClients)
        {
            _inventoryItemRepository = inventoryItemRepository;
            _catalogClients = catalogClients;

        }

        [HttpGet("Get")]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> Get(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            var catalogItems = await _catalogClients.GetCatalogItemsAsync();
            var inventoryItemEntities = await _inventoryItemRepository.GetAllAsync(item => item.UserId == userId);
            var inventoryItemDtos = inventoryItemEntities.Select(inventoryItem =>
            {
                var catalogItem = catalogItems.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
                return inventoryItem.AsDto(catalogItem.Name,catalogItem.Description);
            });

            // var items = (await _inventoryItemRepository.GetAllAsync(items => items.UserId == userId))
            // .Select(item => item.AsDto());

            return Ok(inventoryItemDtos);
        }

        [HttpPost]
        [Route("Post")]
        public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
        {
            var inventoryItem = await _inventoryItemRepository.GetAsync(item => item.UserId == grantItemsDto.UserId &&
                                                             item.CatalogItemId == grantItemsDto.CatalogItemId);
            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem()
                {
                    CatalogItemId = grantItemsDto.CatalogItemId,
                    UserId = grantItemsDto.UserId,
                    Quantity = grantItemsDto.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow,
                };
                await _inventoryItemRepository.CreateAsync(inventoryItem);
            }

            else
            {
                inventoryItem.Quantity += grantItemsDto.Quantity;
                await _inventoryItemRepository.UpdateAsync(inventoryItem);
            }
            return Ok();
        }

    }
}