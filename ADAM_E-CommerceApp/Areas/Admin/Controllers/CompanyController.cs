using ADAM.DataAccess.Repository.IRepository;
using ADAM.Models;
using ADAM.Models.ViewModels;
using ADAM.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ADAM_E_CommerceApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;        

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;          
        }

        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitOfWork.companyRepository.GetAll().ToList();
            return View(objCompanyList);
        }
        public IActionResult Upsert(int? id)       
        { 
            if (id != null || id != 0) //Update case
            {
                Company companyObj = _unitOfWork.companyRepository.GetDetails(u => u.CompanyId == id);
                return View(companyObj);
            }
            else
            {
                return View(new Company());
            }            
        }

        [HttpPost]
        public IActionResult Upsert(Company companyObj)
        {
            if (ModelState.IsValid)
            {                
                if (companyObj.CompanyId == 0)
                {
                    _unitOfWork.companyRepository.Add(companyObj);
                }
                else
                {
                    _unitOfWork.companyRepository.Update(companyObj);
                }
                _unitOfWork.Save();
                TempData["successMsg"] = "Company created successfully!";
                return RedirectToAction("Index", "Company");
            }            
            return View(companyObj);
        }        

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objCompanyList = _unitOfWork.companyRepository.GetAll().ToList();
            return Json(new { data = objCompanyList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            Company? companyToBeDeleted = _unitOfWork.companyRepository.GetDetails(u => u.CompanyId == id);
            if (companyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }            
            _unitOfWork.companyRepository.Remove(companyToBeDeleted);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Company deleted successfully" });
        }
        #endregion
    }
}
