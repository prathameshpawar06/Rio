using AutoMapper;
using BAL.Response;
using BAL.Services;
using BAL.ViewModels;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Authorize]
    public class DepartmentsController : Controller
    {
        #region Fields

        private readonly IMapper _mapper;
        private readonly DepartmentService _departmentService;
        private readonly CompanyMastersService _companyMastersService;
        private readonly LoggerService _loggerService;

        #endregion

        #region Ctor

        public DepartmentsController(IMapper mapper,
            DepartmentService departmentService,
            CompanyMastersService companyMastersService,
            LoggerService loggerService)
        {
            _mapper = mapper;
            _departmentService = departmentService;
            _companyMastersService = companyMastersService;
            _loggerService = loggerService;
        }

        #endregion

        #region Method

        [HttpGet]
        [Route("department/searchdepartment/")]
        public async Task<DepartmentResponse> SearchDepartmentAsync(int departmentId = 0, int companyId = 0, string departmentName = null, int pageNo = 1, int pageSize = 10)
        {
            try
            {
                DepartmentResponse resp = new();
                var departmentList = await _departmentService.GetAllDepartmentsAsync(departmentId, companyId, departmentName, pageIndex: pageNo - 1, pageSize: pageSize);
                if (departmentList.Count == 0)
                {
                    resp.Message = "Record not found.";
                    resp.Error = 1;
                    return resp;
                }
                foreach (var item in departmentList)
                {
                    var departments = _mapper.Map<DepartmentResponseModel>(item);
                    resp.Data.DepartmentModels.Add(departments);
                }

                resp.Data.PageIndex = pageNo;
                resp.Data.TotalCount = departmentList.TotalCount;
                resp.Data.HasNextPage = departmentList.HasNextPage;
                resp.Message = "All records of departments retrieved successfully by CompanyId/DepartmentId.";
                resp.Error = 0;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new DepartmentResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }

        }

        [HttpPost]
        [Route("department/create/{companyId}")]
        public async Task<DepartmentResponse> CreateAsync([FromBody] DepartmentModel departmentModel, int companyId)
        {
            try
            {
                DepartmentResponse resp = new();
                var company = await _companyMastersService.GetCompanyMasterBySrNoAsync(companyId);
                if (company == null)
                {
                    resp.Error = 1;
                    resp.Message = "Company not found.";
                    return resp;
                }

                var department = _mapper.Map<Department>(departmentModel);
                department.COCODE = Guid.NewGuid().ToString();
                department.SerialNO = Guid.NewGuid().ToString();
                await _departmentService.CreateDepartmentAsync(department);
                await _companyMastersService.CreateComapnyDepartmentMapping(department, companyId);

                resp.Error = 0;
                resp.Data.DepartmentModels.Add(_mapper.Map<DepartmentResponseModel>(department));
                resp.Message = "Department record created successfully.";
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new DepartmentResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [HttpPost]
        [Route("department/edit/{departmentId}")]
        public async Task<DepartmentResponse> EditAsync([FromBody] DepartmentModel departmentModel, int departmentId)
        {
            try
            {
                DepartmentResponse resp = new();
                var departmentBySrNo = await _departmentService.GetDepartmentByApplicationSrNoAsync(departmentId);
                if (departmentBySrNo != null)
                {
                    departmentBySrNo.DepartmentName = departmentModel.DepartmentName;
                    departmentBySrNo.DepartmentNameAR = departmentModel.DepartmentNameAR;
                    await _departmentService.UpdateDepartmentAsync(departmentBySrNo);

                    resp.Error = 0;
                    var department = _mapper.Map<DepartmentResponseModel>(departmentBySrNo);
                    resp.Data.DepartmentModels.Add(department);
                    resp.Message = "Department record updated successfully.";
                    return resp;
                }

                resp.Message = "Department not found";
                resp.Error = 1;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new DepartmentResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [HttpPost]
        [Route("department/delete/{departmentId}")]
        public async Task<DepartmentResponse> DeleteAsync(int departmentId)
        {
            try
            {
                DepartmentResponse resp = new();
                var departmentBySrno = await _departmentService.GetDepartmentByApplicationSrNoAsync(departmentId);
                if (departmentBySrno != null)
                {
                    await _departmentService.DeleteDepartmentAsync(departmentBySrno);
                    resp.Error = 0;
                    resp.Message = "Department record has been deleted successfully.";
                    return resp;
                }

                resp.Message = "Department not found";
                resp.Error = 1;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new DepartmentResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        #endregion
    }
}
