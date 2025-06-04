using AutoMapper;
using BAL.Response;
using BAL.ViewModels;
using DAL.Context;
using DAL.Helper;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services
{
    public class ShoppingCartService
    {
        #region Fields

        private readonly RioContext _context;
        private readonly IMapper _mapper;

        #endregion

        #region Ctor

        public ShoppingCartService(RioContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        #endregion

        #region Methods

        public async Task<Order> ConfirmOrder(OrderViewModel orderViewModel,int companyId,int brachId)
        {
            var newOrder = _mapper.Map<Order>(orderViewModel);
            newOrder.OrderDateTime = DateTime.Now;
            newOrder.PaymentStatusId = 0;
            newOrder.CompanyId = companyId;
            newOrder.BranchId = brachId;
            await _context.Orders.AddAsync(newOrder);
            await _context.SaveChangesAsync();

            decimal otderTotalWithVat = 0;
            decimal WithoutVat = 0;
            foreach (var items in orderViewModel.OrderItems)
            {
                var newItem = _mapper.Map<OrderItem>(items);
                newItem.OrderId = newOrder.Id;
                var itemById = _context.Items.FirstOrDefault(x => x.ApplicationSrNo == items.ItemId);
                if (itemById == null)
                    continue;
                newItem.Price = items.Quantity * (itemById.IncludingVat ? (decimal.Parse(itemById.SalesPrice) * itemById.PercentageVat / 100 + decimal.Parse(itemById.SalesPrice)) : decimal.Parse(itemById.SalesPrice));
                otderTotalWithVat += newItem.Price;
                WithoutVat += items.Quantity * decimal.Parse(itemById.SalesPrice);
                await _context.OrderItems.AddAsync(newItem);
            }

            newOrder.OrderTotal = otderTotalWithVat;
            newOrder.VatAmount = otderTotalWithVat - WithoutVat;
            await _context.SaveChangesAsync();
            return newOrder;
        }

        public async Task<IPagedList<OrderResponseModel>> GetOrderList(int? orderId = 0 , int? companyId = 0, int? branchId = 0, int? salesmanId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            IList<OrderResponseModel> model = new List<OrderResponseModel>();
            OrderResponseModel orderModel = new();

            var order = await _context.Orders.Where(x => (orderId == 0 || x.Id == orderId)
             && (companyId == 0 || x.CompanyId == companyId)
             && (branchId == 0 || x.BranchId == branchId)
             && (salesmanId == 0 || x.SalesManId == salesmanId)).ToListAsync();

            foreach (var item in order)
            {
                orderModel = _mapper.Map<OrderResponseModel>(item);
                if (item != null)
                {
                    var company = await _context.CompanyMasters.FirstOrDefaultAsync(x => x.ApplicationSrNo == item.CompanyId);
                    orderModel.Company = _mapper.Map<CompanyMasterResponseModel>(company);

                    var branch = await _context.BranchMasters.FirstOrDefaultAsync(x => x.ApplicationSrNo == item.BranchId);
                    orderModel.Branch = _mapper.Map<BranchMasterResponseModel>(branch);

                    var salesman = await _context.Salesmans.FirstOrDefaultAsync(x => x.EmpCode == item.SalesManId);
                    orderModel.SalesMan = _mapper.Map<SalesmanResponseModel>(salesman);

                    var customer = await _context.Customers.FirstOrDefaultAsync(x => x.ApplicationSrNo == item.CustomerId);
                    orderModel.Customer = _mapper.Map<CustomerOrderResponseModel>(customer);

                }
                var orderItems = await _context.OrderItems.Where(x => x.OrderId == item.Id).ToListAsync();
                foreach (var product in orderItems)
                {
                    var newitem = await _context.Items.FirstOrDefaultAsync(x => x.ApplicationSrNo == product.ItemId);
                    if (newitem == null)
                        continue;
                    var neworderItems = _mapper.Map<ItemResponseModel>(newitem);
                    neworderItems.Quantity = product.Quantity;
                    neworderItems.PriceTotal = product.Price;
                    orderModel.OrderItems.Add(neworderItems);
                }

                model.Add(orderModel);
            }
            return new PagedList<OrderResponseModel>(model, pageIndex, pageSize);
        }

        public async Task<IPagedList<SalesReportModel>> GetOrderDetailsAsync(DateTime? startDate = null, DateTime? endDate = null, int companyId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            List<SalesReportModel> salesModelList = new();

            var orderList = await GetAllOrderAsync(startDate, endDate, companyId);
            var salesManList = await _context.Salesmans.ToListAsync();
            var orderItemList = await _context.OrderItems.ToListAsync();

            foreach (var salesMan in salesManList)
            {
                var quentity = 0;

                var orders = orderList.Where(x => x.SalesManId == salesMan.EmpCode);
                var orderCount = 0;
                decimal madaAmount = 0;
                decimal cashAmount = 0;
                int numberOfItems = 0;
                foreach (var order in orders)
                {
                    var orderItems = orderItemList.Where(x => x.OrderId == order.Id);
                    numberOfItems = orderItems.Select(x => x.ItemId).Distinct().Count();
                    quentity += orderItems.Sum(x => x.Quantity);

                    orderCount++;

                    if (order.PaymentMethod.ToLower() == "mada")
                        madaAmount += order.OrderTotal;

                    if (order.PaymentMethod.ToLower() == "cash")
                        cashAmount += order.OrderTotal;

                }
                if (orderCount > 0)
                {
                    var SalesReportModel = new SalesReportModel
                    {
                        SalesManId = salesMan.EmpCode,
                        SalesManName = salesMan.SalesMan,
                        TotalNotrans = orderCount,
                        MadaAmount = madaAmount,
                        CashAmount = cashAmount,
                        TotalAmount = (madaAmount + cashAmount),
                        VATAmount = 0,
                        Noitems = numberOfItems,
                        ItemsQuantity = quentity,
                    };
                    salesModelList.Add(SalesReportModel);
                }
            }
            return new PagedList<SalesReportModel>(salesModelList, pageIndex, pageSize);
        }

        public async Task<IList<Order>> GetAllOrderAsync(DateTime? startDate = null, DateTime? endDate = null, int companyId = 0)
        {
            var orderList = _context.Orders.AsQueryable();

            if (startDate.HasValue)
                orderList = orderList.Where(c => startDate.Value.Date <= c.OrderDateTime.Date);
            if (endDate.HasValue)
                orderList = orderList.Where(c => endDate.Value.Date >= c.OrderDateTime.Date);
            if (companyId > 0)
                orderList = orderList.Where(c => c.CompanyId == companyId);

            return await orderList.ToListAsync();
        }

        public async Task UpdatePaymentStatus(Order order, PaymentStatus newPaymentStatus)
        {
            if (Enum.IsDefined(typeof(PaymentStatus), newPaymentStatus))
            {
                order.PaymentStatusId = (int)newPaymentStatus;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders.FindAsync(orderId);
        }

        #endregion
    }
}
