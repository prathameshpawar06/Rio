using DAL.Context;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services
{
    public class SalesReturnService
    {
        #region Fields

        private readonly RioContext _context;

        #endregion

        #region Ctor
        public SalesReturnService(RioContext context)
        {
            _context = context;
        }

        #endregion

        #region Methods

        public async Task CreateSalesReturnAsync(SalesReturn salesReturn)
        {
            _context.SalesReturns.Add(salesReturn);
            await _context.SaveChangesAsync();
        }

        public async Task<SalesReturn> GetSalesReturnByApplicationSrNoAsync(int applicationSrNo)
        {
            var salesreturnSrNo = await _context.SalesReturns.Where(x => x.ApplicationSrNo == applicationSrNo).AsNoTracking().FirstOrDefaultAsync();

            return salesreturnSrNo;
        }

        public async Task UpdateSalesReturnAsync(SalesReturn salesReturn)
        {
            _context.SalesReturns.Update(salesReturn);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSalesReturnAsync(SalesReturn salesReturn)
        {
            _context.SalesReturns.Remove(salesReturn);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
