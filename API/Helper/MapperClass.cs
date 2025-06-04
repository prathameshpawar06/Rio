using AutoMapper;
using BAL.Response;
using BAL.ViewModels;
using DAL.Models;

namespace API.Helper
{
    public class MapperClass : Profile
    {
        public MapperClass()
        {
            CreateMap<BranchMasterResponseModel, BranchMaster>().ReverseMap();
            CreateMap<Salesman, SalesmanModel>().ReverseMap();
            CreateMap<SalesmanResponseModel, Salesman>().ReverseMap();
            CreateMap<Category, CategoryModel>().ReverseMap();
            CreateMap<CategoryResponseModel, Category>().ReverseMap();
            CreateMap<CustomerResponseModel, Customer>().ReverseMap();
            CreateMap<Department, DepartmentModel>().ReverseMap();
            CreateMap<DepartmentResponseModel, Department>().ReverseMap();
            CreateMap<SalesInvoiceResponseModel, SalesInvoice>().ReverseMap();
            CreateMap<SalesReturnResponseModel, SalesReturn>().ReverseMap();
            CreateMap<Order, OrderViewModel>().ReverseMap();
            CreateMap<OrderItem, OrderItemsViewModel>().ReverseMap();
            CreateMap<OrderResponseModel, Order>().ReverseMap();
            CreateMap<ItemResponseModel, OrderItem>().ReverseMap();//not neded //remove this

            //For Mapping Ignore Fields

            //CreateMap<Item, ItemResponseModel>()
            //.ForMember(x => x.Image, x => x.MapFrom<ItemImageResolver>());

            CreateMap<CompanyMasterResponseModel, CompanyMaster>();
            CreateMap<CompanyMaster, CompanyMasterResponseModel>()
              .ForMember(x => x.Logo, x => x.MapFrom<LogoResolver>());

            CreateMap<ItemResponseModel, Item>();
            CreateMap<Item, ItemResponseModel>()
                .ForMember(x => x.Image, x => x.MapFrom<ItemImageResolver>())
                .ForMember(x => x.TotalPrice, x => x.MapFrom(x =>
                                                                      x.IncludingVat
                                                                      ? decimal.Parse(x.SalesPrice) * x.PercentageVat / 100 + decimal.Parse(x.SalesPrice)
                                                                      : decimal.Parse(x.SalesPrice)
                                                                  ));

            CreateMap<CompanyMaster, CompanyMasterModel>().ReverseMap();
            CreateMap<CompanyMasterModel, CompanyMaster>()
               .ForMember(x => x.COCODE, option => option.Ignore())
               .ForMember(x => x.SerialNO, option => option.Ignore());

            CreateMap<BranchMasterModel, BranchMaster>()
               .ForMember(x => x.BranchCode, option => option.Ignore())
               .ForMember(x => x.CRNO, option => option.Ignore())
               .ForMember(x => x.VATNO, option => option.Ignore())
               .ForMember(x => x.COCODE, option => option.Ignore())
               .ForMember(x => x.SerialNO, option => option.Ignore());

            CreateMap<CustomerModel, Customer>()
                .ForMember(x => x.COCODE, option => option.Ignore())
                .ForMember(x => x.SerialNO, option => option.Ignore())
                .ForMember(x => x.VATNO, option => option.Ignore())
                .ForMember(x => x.CRNO, option => option.Ignore());

            CreateMap<ItemModel, Item>()
                .ForMember(x => x.ItemCode, option => option.Ignore())
                .ForMember(x => x.COCODE, option => option.Ignore())
                .ForMember(x => x.SerialNO, option => option.Ignore());

            CreateMap<SalesReturnModel, SalesReturn>()
                .ForMember(x => x.ItemCode, option => option.Ignore())
                .ForMember(x => x.COCODE, option => option.Ignore())
                .ForMember(x => x.SerialNO, option => option.Ignore());

            CreateMap<SalesInvoiceModel, SalesInvoice>()
                .ForMember(x => x.ItemCode, option => option.Ignore())
                .ForMember(x => x.COCODE, option => option.Ignore())
                .ForMember(x => x.SerialNO, option => option.Ignore());


            // for Order Response
            CreateMap<Customer, CustomerOrderResponseModel>();
           
        }
    }


    public class LogoResolver : IValueResolver<CompanyMaster, CompanyMasterResponseModel, string>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogoResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Resolve(CompanyMaster source, CompanyMasterResponseModel destination, string destMember, ResolutionContext context)
        {
            // Access HttpContext using the injected IHttpContextAccessor
            HttpContext httpContext = _httpContextAccessor.HttpContext;

            // Your logic to construct the complete URL for the logo
            // Example: Assuming logoPath is a relative path
            string logoPath = source.Logo;
            string domainUrl = $"{httpContext?.Request.Scheme}://{httpContext?.Request.Host}";
            string completeUrl = $"{domainUrl}/{logoPath}";

            return completeUrl;
        }
    }

    public class ItemImageResolver : IValueResolver<Item, ItemResponseModel, string>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ItemImageResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Resolve(Item source, ItemResponseModel destination, string destMember, ResolutionContext context)
        {
            // Access HttpContext using the injected IHttpContextAccessor
            HttpContext httpContext = _httpContextAccessor.HttpContext;

            // Your logic to construct the complete URL for the item image
            // Example: Assuming imagePath is a relative path
            string imagePath = source.Image;
            string domainUrl = $"{httpContext?.Request.Scheme}://{httpContext?.Request.Host}";
            string completeUrl = $"{domainUrl}/{imagePath}";

            return completeUrl;
        }
    }
    
}
