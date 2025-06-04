using DAL.Context;
using DAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Net;

namespace BAL.Services
{
    public class LoggerService
    {
        #region Fields

        private readonly RioContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Ctor

        public LoggerService(RioContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Utility

        public virtual string GetUrlReferrer()
        {
            if (!IsRequestAvailable())
                return string.Empty;

            //URL referrer is null in some case (for example, in IE 8)
            return _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Referer];
        }

        protected virtual bool IsRequestAvailable()
        {
            if (_httpContextAccessor?.HttpContext == null)
                return false;

            try
            {
                if (_httpContextAccessor.HttpContext.Request == null)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public virtual string GetThisPageUrl(bool includeQueryString, bool? useSsl = null, bool lowercaseUrl = false)
        {
            if (!IsRequestAvailable())
                return string.Empty;

            //add local path to the URL
            var pageUrl = $"{_httpContextAccessor.HttpContext.Request.Path}";

            //add query string to the URL
            if (includeQueryString)
                pageUrl = $"{pageUrl}{_httpContextAccessor.HttpContext.Request.QueryString}";

            //whether to convert the URL to lower case
            if (lowercaseUrl)
                pageUrl = pageUrl.ToLowerInvariant();

            return pageUrl;
        }

        public virtual string GetCurrentIpAddress()
        {
            if (!IsRequestAvailable())
                return string.Empty;

            if (_httpContextAccessor.HttpContext.Connection?.RemoteIpAddress is not IPAddress remoteIp)
                return "";

            if (remoteIp.Equals(IPAddress.IPv6Loopback))
                return IPAddress.Loopback.ToString();

            return remoteIp.MapToIPv4().ToString();
        }

        #endregion

        #region Methods

        public async Task InsertLogAsync(string shortMessage, string fullMessage = "", CompanyMaster companyMaster = null)
        {
            var log = new Log
            {
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                IpAddress = GetCurrentIpAddress(),
                CustomerId = companyMaster?.ApplicationSrNo,
                PageUrl = GetThisPageUrl(true),
                ReferrerUrl = GetUrlReferrer(),
                CreatedOnUtc = DateTime.UtcNow
            };

            await _context.Logs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
