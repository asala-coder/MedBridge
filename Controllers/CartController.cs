using MedBridge.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace MedBridge.Controllers
{

    [Route("api/cart")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto model)
        {
            try
            {
                if (model.Quantity <= 0)
                    return BadRequest("Quantity must be greater than 0.");

                string userId = GetUserId();
                if (userId == null)
                    return Unauthorized("Invalid user.");

                var product = await _context.Products.FindAsync(model.ProductId);
                if (product == null)
                    return NotFound("Product not found.");

                if (product.StockQuantity <= 0)
                    return BadRequest("Product is out of stock.");

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart { UserId = userId, CartItems = new List<CartItem>() };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == model.ProductId);
                if (cartItem == null)
                {
                    cartItem = new CartItem { ProductId = model.ProductId, Quantity = model.Quantity };
                    cart.CartItems.Add(cartItem);
                }
                else
                {
                    cartItem.Quantity += model.Quantity;
                }

                await _context.SaveChangesAsync();
                return Ok(new { Message = "Product added to cart", Cart = cart });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                string userId = GetUserId();
                if (userId == null)
                    return Unauthorized("Invalid user.");

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.CartItems.Any())
                    return Ok(new { Message = "Cart is empty" });

                return Ok(cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }

        [HttpDelete("delete/{productId}")]
        public async Task<IActionResult> DeleteFromCart(int productId)
        {
            try
            {
                string userId = GetUserId();
                if (userId == null)
                    return Unauthorized("Invalid user.");

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                    return NotFound("Cart not found.");

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                if (cartItem == null)
                    return NotFound("Product not found in cart.");

                cart.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Product removed from cart" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartItemDto model)
        {
            try
            {
                if (model.Quantity <= 0)
                    return BadRequest("Quantity must be greater than 0.");

                string userId = GetUserId();
                if (userId == null)
                    return Unauthorized("Invalid user.");

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                    return NotFound("Cart not found.");

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == model.ProductId);
                if (cartItem == null)
                    return NotFound("Product not found in cart.");

                cartItem.Quantity = model.Quantity;
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Quantity updated", Cart = cart });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                string userId = GetUserId();
                if (userId == null)
                    return Unauthorized("Invalid user.");

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                    return NotFound("Cart not found.");

                cart.CartItems.Clear();
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Cart cleared" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }
    }

}