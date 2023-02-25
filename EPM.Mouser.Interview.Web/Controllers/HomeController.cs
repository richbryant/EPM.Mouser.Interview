using EPM.Mouser.Interview.Data;
using EPM.Mouser.Interview.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPM.Mouser.Interview.Web.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public HomeController(IWarehouseRepository warehouseRepository)
            => _warehouseRepository = warehouseRepository;


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _warehouseRepository.List().ConfigureAwait(false);
            var model = products.Select(x => new ProductViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    InStockQuantity = x.InStockQuantity,
                    ReservedQuantity = x.ReservedQuantity
                }).ToList();
            return View(model);
        }

        [HttpGet("Product")]
        public async Task<IActionResult> Product(long id)
        {
            var item = await _warehouseRepository.Get(id);
            if (item is null) return BadRequest();
            var model = new ProductViewModel
            {
                Id = item.Id,
                Name = item.Name,
                InStockQuantity = item.InStockQuantity,
                ReservedQuantity = item.ReservedQuantity
            };
            return View(model);
        }

    }
}
