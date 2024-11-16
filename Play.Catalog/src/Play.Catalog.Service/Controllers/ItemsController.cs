using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Entites;
using Play.Catalog.Service.Extension;
using Play.Common;
using Play.Catalog.Contracts;
namespace Play.Catalog.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {

        private readonly IRepository<Item> _itemRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        //private static int requestCount = 0;
        public ItemsController(IRepository<Item> itemRepository, IPublishEndpoint publishEndpoint)
        {
            _itemRepository = itemRepository;
            _publishEndpoint = publishEndpoint;
        }



        // private static readonly List<ItemDto> items = new List<ItemDto>()
        // {
        //     new ItemDto(Guid.NewGuid(),"potion","test des",5,DateTimeOffset.UtcNow),
        //     new ItemDto(Guid.NewGuid(),"hasan","hasan  test des",5,DateTimeOffset.UtcNow),
        // };

        [HttpGet("Get")]
        public async Task<ActionResult<IEnumerable<ItemDto>>> Get()
        {
            // requestCount++;
            // Console.WriteLine($"request {requestCount} starting...");
            // if (requestCount <= 2)
            // {
            //     Console.WriteLine($"request {requestCount} Delaying...");
            //     await Task.Delay(TimeSpan.FromSeconds(10));
            // }
            // if (requestCount <= 4)
            // {
            //     Console.WriteLine($"request {requestCount} 500 (internal server error)...");
            //     return StatusCode(500);
            // }
            var result = (await _itemRepository.GetAllAsync()).Select(item => item.AsDto());

            //Console.WriteLine($"request {requestCount} 200 (ok)...");

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ItemDto> GetById(Guid id)
        {
            var result = await _itemRepository.GetAsync(id);
            return result.AsDto();
        }

        [HttpPost]
        [Route("Post")]
        public async Task<ActionResult<ItemDto>> Post(CreateItemDto createItemDto)
        {
            var item = new Item
            {
                Name = createItemDto.Name,
                Description = createItemDto.Description,
                Price = createItemDto.Price,
                CreateDate = DateTimeOffset.UtcNow
            };
            await _itemRepository.CreateAsync(item);
            await _publishEndpoint.Publish(new CatalogItemCrated(item.Id,item.Name,item.Description));
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPut]
        [Route("Put")]
        public async Task<IActionResult> Put(Guid Id, UpdateItemDto updateItemDto)
        {
            var existingItem = await _itemRepository.GetAsync(Id);
            if (existingItem == null)
            {
                return NotFound();
            }
            existingItem.Name = updateItemDto.Name;
            existingItem.Description = updateItemDto.Description;
            existingItem.Price = updateItemDto.Price;
            await _itemRepository.UpdateAsync(existingItem);
            await _publishEndpoint.Publish(new CatalogItemUpdated(existingItem.Id,existingItem.Name,existingItem.Description));

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var item = await _itemRepository.GetAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            await _itemRepository.RemoveAsync(item.Id);
            await _publishEndpoint.Publish(new CatalogItemDeleted(item.Id));

            return NoContent();
        }
    }
}