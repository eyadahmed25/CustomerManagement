using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CustomerManagement.Services.BusinessLogic;
using CustomerManagement.Services.DTOs;
using System.Linq;

namespace CustomerManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetAll([FromQuery] bool onlyActive = true)
        {
            try
            {
                var customers = await _customerService.GetAllAsync(onlyActive);
                var customerList = customers.ToList();
                if (!customerList.Any())
                {
                    return NotFound("No customers found.");
                }
                return Ok(customerList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDTO>> GetById(Guid id)
        {
            try
            {
                var customer = await _customerService.GetByIdAsync(id);
                if (customer == null)
                {
                    return NotFound($"Customer with ID {id} not found.");
                }
                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CustomerDTO>> Create([FromBody] AddCustomerDTO dto)
        {
            try
            {
                var createdCustomer = await _customerService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = createdCustomer.CustomerID }, createdCustomer);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(argEx.Message);
            }
            catch (InvalidOperationException invOpEx)
            {
                return BadRequest(invOpEx.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CustomerDTO>> Update(Guid id, [FromBody] AddCustomerDTO dto)
        {
            try
            {
                var updatedCustomer = await _customerService.UpdateAsync(id, dto);
                if (updatedCustomer == null)
                {
                    return NotFound($"Customer with ID {id} not found.");
                }
                return Ok(updatedCustomer);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(argEx.Message);
            }
            catch (InvalidOperationException invOpEx)
            {
                return BadRequest(invOpEx.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _customerService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound($"Customer with ID {id} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
