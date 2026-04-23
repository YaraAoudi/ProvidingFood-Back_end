using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Service;

namespace ProvidingFood2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AdminEventsController : ControllerBase
    {
        private readonly AdminEventService _service;

        public AdminEventsController(AdminEventService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateEventDto dto)
        {
            var id = await _service.CreateEventAsync(dto);
            return Ok(new { id });
        }

        
        [HttpPost("{id}/items")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddItem(int id, CreateEventItemDto dto)
        {
            await _service.AddItemAsync(id, dto);
            return Ok();
        }

       
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        
        [HttpGet("{id}/items")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetItems(int id)
        {
            return Ok(await _service.GetItemsAsync(id));
        }

      
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, UpdateEventDto dto)
        {
            await _service.UpdateAsync(id, dto);
            return Ok();
        }

        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok();
        }

        [HttpPut("items/{itemId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateItem(int itemId, [FromBody] UpdateEventItemDto dto)
        {
            await _service.UpdateItemAsync(itemId, dto);
            return Ok("Item updated successfully");
        }

        
        [HttpDelete("items/{itemId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteItem(int itemId)
        {
            await _service.DeleteItemAsync(itemId);
            return Ok("Item deleted successfully");
        }

        [HttpGet("event/{eventId}/items")]
        public async Task<IActionResult> GetEventItems(int eventId)
        {
            var items = await _service.GetItemsByEventIdAsync(eventId);
            return Ok(items);
        }

        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Activate(int id)
        {
            await _service.ActivateAsync(id);
            return Ok("Event activated");
        }

   
        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(int id)
        {
            await _service.DeactivateAsync(id);
            return Ok("Event deactivated");
        }
    }
}
