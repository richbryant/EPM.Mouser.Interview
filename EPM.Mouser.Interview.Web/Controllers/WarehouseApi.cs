using System.Diagnostics;
using EPM.Mouser.Interview.Data;
using EPM.Mouser.Interview.Models;
using Microsoft.AspNetCore.Mvc;

namespace EPM.Mouser.Interview.Web.Controllers
{
    [ApiController]
    [Route("api/warehouse")]
    public class WarehouseApi : Controller
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehouseApi(IWarehouseRepository warehouseRepository)
            => _warehouseRepository = warehouseRepository;


        /*
         *  Action: GET
         *  Url: api/warehouse/id
         *  This action should return a single product for an Id
         */
        [HttpGet("{id:long}")]
        public async Task<JsonResult> GetProduct(long id)
        {
            var item = await _warehouseRepository.Get(id);
            return Json(item);
        }

        /*
         *  Action: GET
         *  Url: api/warehouse
         *  This action should return a collection of products in stock
         *  In stock means In Stock Quantity is greater than zero and In Stock Quantity is greater than the Reserved Quantity
         */
        [HttpGet]
        public async Task<JsonResult> GetPublicInStockProducts()
        {
            var products = await _warehouseRepository.Query(x => x.InStockQuantity > 0 && x.InStockQuantity > x.ReservedQuantity);
            return Json(products);
        }


        /*
         *  Action: GET
         *  Url: api/warehouse/order
         *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
         *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
         *       {
         *           "id": 1,
         *           "quantity": 1
         *       }
         *
         *  This action should increase the Reserved Quantity for the product requested by the amount requested
         *
         *  This action should return failure (success = false) when:
         *     - ErrorReason.NotEnoughQuantity when: The quantity being requested would increase the Reserved Quantity to be greater than the In Stock Quantity.
         *     - ErrorReason.QuantityInvalid when: A negative number was requested
         *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */

        [HttpPost("order")]
        public async Task<JsonResult> OrderItem([FromBody] UpdateQuantityRequest request)
        {
            var result = new UpdateResponse();
            var item = await _warehouseRepository.Get(request.Id);

            if (!ValidateRequest(request, ref result, item))
            {
                return Json(result);
            }

            Debug.Assert(item != null, nameof(item) + " != null");

            if(item.ReservedQuantity + request.Quantity > item.InStockQuantity) 
            {
                result.ErrorReason = ErrorReason.NotEnoughQuantity;
                return Json(result);
            }

            item.ReservedQuantity += request.Quantity;
            await _warehouseRepository.UpdateQuantities(item);
            result.Success = true;
            return Json(result);
        }

        /*
         *  Url: api/warehouse/ship
         *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
         *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
         *       {
         *           "id": 1,
         *           "quantity": 1
         *       }
         *
         *
         *  This action should:
         *     - decrease the Reserved Quantity for the product requested by the amount requested to a minimum of zero.
         *     - decrease the In Stock Quantity for the product requested by the amount requested
         *
         *  This action should return failure (success = false) when:
         *     - ErrorReason.NotEnoughQuantity when: The quantity being requested would cause the In Stock Quantity to go below zero.
         *     - ErrorReason.QuantityInvalid when: A negative number was requested
         *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */

        [HttpPost("ship")]
        public async Task<JsonResult> ShipItem([FromBody] UpdateQuantityRequest request)
        {
            var result = new UpdateResponse();
            var item = await _warehouseRepository.Get(request.Id);

            if (!ValidateRequest(request, ref result, item))
            {
                return Json(result);
            }

            Debug.Assert(item != null, nameof(item) + " != null");
            if (item.InStockQuantity - request.Quantity < 0)
            {
                result.ErrorReason = ErrorReason.NotEnoughQuantity;
                return Json(result);
            }
            item.InStockQuantity -= request.Quantity;
            item.ReservedQuantity -= request.Quantity;
            result.Success = true;
            return Json(result);
        }

        /*
        *  Url: api/warehouse/restock
        *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
        *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
        *       {
        *           "id": 1,
        *           "quantity": 1
        *       }
        *
        *
        *  This action should:
        *     - increase the In Stock Quantity for the product requested by the amount requested
        *
        *  This action should return failure (success = false) when:
        *     - ErrorReason.QuantityInvalid when: A negative number was requested
        *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */

        [HttpPost("restock")]
        public async Task<JsonResult> RestockItem([FromBody] UpdateQuantityRequest request)
        {
            var result = new UpdateResponse();
            var item = await _warehouseRepository.Get(request.Id);

            if (!ValidateRequest(request, ref result, item))
            {
                return Json(result);
            }

            Debug.Assert(item != null, nameof(item) + " != null");
            item.InStockQuantity += request.Quantity;
            result.Success = true;
            return Json(result);
        }


        /*
        *  Url: api/warehouse/add
        *  This action should return a EPM.Mouser.Interview.Models.CreateResponse<EPM.Mouser.Interview.Models.Product>
        *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.Product in JSON format in the body of the request
        *       {
        *           "id": 1,
        *           "inStockQuantity": 1,
        *           "reservedQuantity": 1,
        *           "name": "product name"
        *       }
        *
        *
        *  This action should:
        *     - create a new product with:
        *          - The requested name - But forced to be unique - see below
        *          - The requested In Stock Quantity
        *          - The Reserved Quantity should be zero
        *
        *       UNIQUE Name requirements
        *          - No two products can have the same name
        *          - Names should have no leading or trailing whitespace before checking for uniqueness
        *          - If a new name is not unique then append "(x)" to the name [like windows file system does, where x is the next avaiable number]
        *
        *
        *  This action should return failure (success = false) and an empty Model property when:
        *     - ErrorReason.QuantityInvalid when: A negative number was requested for the In Stock Quantity
        *     - ErrorReason.InvalidRequest when: A blank or empty name is requested
        */
        [HttpPost("add")]
        public async Task<JsonResult> AddNewProduct([FromBody] Product product)
        {
            var result = new CreateResponse<Product>();
            if(string.IsNullOrEmpty(product.Name))
            {
                result.ErrorReason = ErrorReason.InvalidRequest;
                return Json(result);
            }
            if(product.InStockQuantity < 0)
            {
                result.ErrorReason = ErrorReason.QuantityInvalid;
                return Json(result);
            }

            product.Name= product.Name.Trim();

            var existing = (await _warehouseRepository.Query(x => x.Name == product.Name).ConfigureAwait(false))
                .Select(x => x.Name)
                .ToList();

            if(existing.Any()) 
            {
                try 
                {
                    var productNames = (await _warehouseRepository.List().ConfigureAwait(false))
                        .Select(x => x.Name)
                        .ToList();

                    product.UniquifyName(productNames);
                }
                catch (Exception) 
                {
                    result.ErrorReason = ErrorReason.InvalidRequest;
                    return Json(result);
                }
            }

            result.Model = await _warehouseRepository.Insert(product);
            result.Success = true;
            return Json(result);
        }

        private static bool ValidateRequest(UpdateQuantityRequest request, ref UpdateResponse result, Product? item)
        {
            if (request.Quantity < 0)
            {
                result.ErrorReason = ErrorReason.QuantityInvalid;
                return false;
            }

            if (item is not null) return true;

            result.ErrorReason = ErrorReason.InvalidRequest;
            return false;
        }
    }
}
