using DAL.Context;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services
{
    public class SalesinvoiceService
    {
        #region Fields

        private readonly RioContext _context;

        #endregion

        #region Ctor

        public SalesinvoiceService(RioContext context)
        {
            _context = context;
        }

        #endregion

        #region Methods

        public async Task CreateSalesInvoiceAsync(SalesInvoice salesInvoice)
        {
            await _context.SalesInvoices.AddAsync(salesInvoice);
            await _context.SaveChangesAsync();
        }

        public async Task<SalesInvoice> GetSalesinvoiceByApplicationSrNoAsync(int applicationSrNo)
        {
            var salesinvoiceSrNo = await _context.SalesInvoices.FirstOrDefaultAsync(x => x.ApplicationSrNo == applicationSrNo);

            return salesinvoiceSrNo;
        }

        public async Task UpdateSalesinvoiceAsync(SalesInvoice salesInvoice)
        {
            _context.SalesInvoices.Update(salesInvoice);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSalesinvoiceAsync(SalesInvoice salesInvoice)
        {
            _context.SalesInvoices.Remove(salesInvoice);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
