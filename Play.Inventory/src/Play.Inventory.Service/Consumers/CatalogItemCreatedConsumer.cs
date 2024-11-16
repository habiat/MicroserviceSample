using MassTransit;
using Play.Catalog.Contracts;
using Play.Common;
using Play.Inventory.Service.Entities;
using System.Threading.Tasks;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemCreatedConsumer : IConsumer<CatalogItemCrated>
    {
        private readonly IRepository<CatalogItem> _repository;

        public CatalogItemCreatedConsumer(IRepository<CatalogItem> repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<CatalogItemCrated> context)
        {
            var message = context.Message;
            var item = await _repository.GetAsync(message.ItemId);

            if (item != null)
            {
                return;
            }
            item = new CatalogItem
            {
                Description = message.Description,
                Name = message.Name,
                Id = message.ItemId
            };
            await _repository.CreateAsync(item);
        }
    }
}