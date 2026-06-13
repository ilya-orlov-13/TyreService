using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace TyreServiceApp.Areas.Owner.Controllers
{
    /// <summary>
    /// Перенаправляет запросы Owner-зоны на основной контроллер автомобилей (/Cars).
    /// </summary>
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class CarsController : Controller
    {
        public IActionResult Index() => RedirectToRoot("Index");

        public IActionResult Details(int? id) => RedirectToRoot("Details", new { id });

        public IActionResult Create() => RedirectToRoot("Create");

        public IActionResult Edit(int? id) => RedirectToRoot("Edit", new { id });

        public IActionResult Delete(int? id) => RedirectToRoot("Delete", new { id });

        private IActionResult RedirectToRoot(string action, object? routeValues = null)
        {
            var values = new RouteValueDictionary(routeValues) { ["area"] = "" };
            return RedirectToAction(action, "Cars", values);
        }
    }
}
