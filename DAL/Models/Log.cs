namespace DAL.Models
{
    /// <summary>
    /// Represents a log record
    /// </summary>
    public class Log
    {
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the short message
        /// </summary>
        public string ShortMessage { get; set; }

        /// <summary>
        /// Gets or sets the full exception
        /// </summary>
        public string FullMessage { get; set; }

        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the page URL
        /// </summary>
        public string PageUrl { get; set; }

        /// <summary>
        /// Gets or sets the referrer URL
        /// </summary>
        public string ReferrerUrl { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

    }
}
