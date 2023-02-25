namespace EPM.Mouser.Interview.Web.ViewModels
{
    public class UpdateQuantityRequestModel
    {
        /// <summary>
        /// Gets or sets the Id of the product to update.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the quantity change being requested.
        /// </summary>
        public int Quantity { get; set; }
    }
}
