using EPM.Mouser.Interview.Models;

namespace EPM.Mouser.Interview.Web.ViewModels
{
    public class UpdateResponseModel
    {
        /// <summary>
        /// Gets or sets the error reason when Success is false.
        /// </summary>
        /// <value>
        /// The error reason.
        /// </value>
        public ErrorReason? ErrorReason { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the update was a success.
        /// </summary>
        /// <value>
        ///   <c>true</c> if success; otherwise, <c>false</c>.
        /// </value>
        public bool Success { get; set; }
    }
}
