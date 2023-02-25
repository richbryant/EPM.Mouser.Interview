namespace EPM.Mouser.Interview.Web.ViewModels
{
    public class ProductViewModel
    {
        /// <summary>
        /// Product Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Product Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Quantity currently in the warehouse
        /// </summary>
        public int InStockQuantity { get; set; }

        /// <summary>
        /// Quantity reserved for existing orders
        /// </summary>
        public int ReservedQuantity { get; set; }
    }
}
