using ProvidingFood2.Model;

namespace ProvidingFood2.Repository
{
    public interface IAdminEventRepository
    {
        Task<int> CreateEventAsync(Event ev);
        Task AddItemAsync(int eventId, EventItem item);

        Task<IEnumerable<Event>> GetAllAsync();
        Task<Event> GetByIdAsync(int id);

        Task UpdateAsync(int id, Event ev);
        Task DeleteAsync(int id);

        Task<IEnumerable<EventItem>> GetItemsAsync(int eventId);

        Task UpdateItemAsync(int itemId, EventItem item);
        Task DeleteItemAsync(int itemId);
        Task<EventItem> GetItemByIdAsync(int itemId);
        Task<IEnumerable<EventItem>> GetItemsByEventIdAsync(int eventId);
        Task ActivateAsync(int id);
        Task DeactivateAsync(int id);
    }
}
