using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventorySys.Repositories.Interfaces;

namespace SmartInventorySys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
    // ^^^ This line forces the app to use JWT for this controller, not Cookies
    public class ProductsApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductsApiController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/ProductsApi
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _unitOfWork.Product.GetAllAsync();
            return Ok(products); // Returns JSON data
        }
    }
}