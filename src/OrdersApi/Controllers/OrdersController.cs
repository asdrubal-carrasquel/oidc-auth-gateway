using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrdersApi.Controllers;

[ApiController]
[Route("")]
[Authorize]
public class OrdersController : ControllerBase
{
    private static readonly List<Order> _orders = new()
    {
        new Order { Id = 1, CustomerName = "John Doe", Product = "Laptop", Amount = 1200.00m, Status = "Pending" },
        new Order { Id = 2, CustomerName = "Jane Smith", Product = "Mouse", Amount = 25.50m, Status = "Completed" },
        new Order { Id = 3, CustomerName = "Bob Johnson", Product = "Keyboard", Amount = 75.00m, Status = "Pending" }
    };

    [HttpGet]
    [Authorize(Roles = "User,Admin,Support")]
    public IActionResult GetOrders()
    {
        // Get user context from headers (propagated by gateway)
        var userId = Request.Headers["X-User-Id"].FirstOrDefault();
        var userName = Request.Headers["X-User-Name"].FirstOrDefault();
        var country = Request.Headers["X-User-Country"].FirstOrDefault();
        var department = Request.Headers["X-User-Department"].FirstOrDefault();
        var tenant = Request.Headers["X-User-Tenant"].FirstOrDefault();

        return Ok(new
        {
            orders = _orders,
            metadata = new
            {
                requestedBy = userName,
                userId = userId,
                country = country,
                department = department,
                tenant = tenant,
                timestamp = DateTime.UtcNow
            }
        });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "User,Admin,Support")]
    public IActionResult GetOrder(int id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            return NotFound();

        return Ok(order);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = Request.Headers["X-User-Id"].FirstOrDefault();
        var userName = Request.Headers["X-User-Name"].FirstOrDefault();

        var newOrder = new Order
        {
            Id = _orders.Count + 1,
            CustomerName = request.CustomerName,
            Product = request.Product,
            Amount = request.Amount,
            Status = "Pending",
            CreatedBy = userName,
            CreatedAt = DateTime.UtcNow
        };

        _orders.Add(newOrder);

        return CreatedAtAction(nameof(GetOrder), new { id = newOrder.Id }, newOrder);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateOrder(int id, [FromBody] UpdateOrderRequest request)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            return NotFound();

        order.Status = request.Status;
        order.UpdatedBy = Request.Headers["X-User-Name"].FirstOrDefault();
        order.UpdatedAt = DateTime.UtcNow;

        return Ok(order);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteOrder(int id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            return NotFound();

        _orders.Remove(order);
        return NoContent();
    }
}

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class UpdateOrderRequest
{
    public string Status { get; set; } = string.Empty;
}
