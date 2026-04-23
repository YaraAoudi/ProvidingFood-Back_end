using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using ProvidingFood2.Repository;

namespace ProvidingFood2.Service
{
    public class AdminEventService
    {
        private readonly IAdminEventRepository _repo;

        public AdminEventService(IAdminEventRepository repo)
        {
            _repo = repo;
        }

        public async Task<int> CreateEventAsync(CreateEventDto dto)
        {
            return await _repo.CreateEventAsync(new Event
            {
                Name = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = true
            });
        }

        public async Task AddItemAsync(int eventId, CreateEventItemDto dto)
        {
            await _repo.AddItemAsync(eventId, new EventItem
            {
                Name = dto.Name,
                Price = dto.Price
            });
        }

        public Task<IEnumerable<Event>> GetAllAsync()
            => _repo.GetAllAsync();

        public Task<Event> GetByIdAsync(int id)
            => _repo.GetByIdAsync(id);

        public async Task UpdateAsync(int id, UpdateEventDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);

            if (existing == null)
                throw new Exception("Event not found");

            var updated = new Event
            {
                Name = dto.Name ?? existing.Name,
                Description = dto.Description ?? existing.Description,
                StartDate = dto.StartDate ?? existing.StartDate,
                EndDate = dto.EndDate ?? existing.EndDate,
                IsActive = dto.IsActive ?? existing.IsActive
            };

            await _repo.UpdateAsync(id, updated);
        }

        public Task DeleteAsync(int id)
            => _repo.DeleteAsync(id);

        public Task<IEnumerable<EventItem>> GetItemsAsync(int eventId)
            => _repo.GetItemsAsync(eventId);

        public async Task UpdateItemAsync(int itemId, UpdateEventItemDto dto)
        {
            var existing = await _repo.GetItemByIdAsync(itemId);

            if (existing == null)
                throw new Exception("Item not found");

            var updated = new EventItem
            {
                Name = dto.Name ?? existing.Name,
                Price = dto.Price ?? existing.Price
            };

            await _repo.UpdateItemAsync(itemId, updated);
        }

        // 🗑️ DELETE
        public async Task DeleteItemAsync(int itemId)
        {
            var existing = await _repo.GetItemByIdAsync(itemId);

            if (existing == null)
                throw new Exception("Item not found");

            await _repo.DeleteItemAsync(itemId);
        }
        public async Task<IEnumerable<EventItem>> GetItemsByEventIdAsync(int eventId)
        {
            return await _repo.GetItemsByEventIdAsync(eventId);
        }


        public async Task ActivateAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);

            if (existing == null)
                throw new Exception("Event not found");

            await _repo.ActivateAsync(id);
        }

   
        public async Task DeactivateAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);

            if (existing == null)
                throw new Exception("Event not found");

            await _repo.DeactivateAsync(id);
        }

    }


}

