namespace MovieDeck.Web.Controllers
{
using System.Data;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using MovieDeck.Common;
    using MovieDeck.Services.Data;
    using MovieDeck.Web.ViewModels.Companies;

    public class CompaniesController : BaseController
    {
        private readonly ICompaniesService companiesService;

        public CompaniesController(
            ICompaniesService companiesService)
{
            this.companiesService = companiesService;
        }

        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        public IActionResult Create()
        {
            return this.View();
        }

        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        [HttpPost]
        public async Task<IActionResult> Create(CreateCompanyInputModel input)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(input);
            }

            await this.companiesService.CreateAsync(input);

            return this.Redirect("/");
        }
    }
}
