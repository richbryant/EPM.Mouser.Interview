using EPM.Mouser.Interview.Models;

namespace EPM.Mouser.Interview.Web.ViewModels
{
    public class CreateResponse<T> : UpdateResponse
    {
        /// <summary>
        /// Gets or sets the model created.
        /// </summary>
        public T? Model { get; set; }
    }
}
