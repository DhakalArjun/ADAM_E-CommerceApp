using ADAM.DataAccess.Repository.IRepository;
using ADAM.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ADAM_E_CommerceApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.productRepository.GetAll().ToList();
            return View(productList);
        }

        public IActionResult Details(int? productId)
        {
            Product? productDetails = _unitOfWork.productRepository.GetDetails(u => u.ProductId == productId, includeProperties: "Category");
            return View(productDetails);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
