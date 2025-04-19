using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using System.Collections.Generic;
using System;
using Xero.NetStandard.OAuth2.Model.Project;
using Task = System.Threading.Tasks.Task;

namespace XeroNetStandardApp.Controllers
{
    public class ProjectInfoController : ApiAccessorController<ProjectApi>
    {
        public ProjectInfoController(IOptions<XeroConfiguration> xeroConfig) : base(xeroConfig){}

        #region GET Endpoints
        public async Task<IActionResult> GetProjects()
        {
            var response = await Api.GetProjectsAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response.Items);
        }
        [HttpGet]
        public async Task<IActionResult> CreateProject()
        {
            var accountingApi = new AccountingApi();
            var contacts = await accountingApi.GetContactsAsync(XeroToken.AccessToken, TenantId);

            var contactInfo = new List<(string, string)>();
            contacts._Contacts.ForEach(contact => contactInfo.Add(
                (
                    contact.Name,
                    contact.ContactID.ToString()
                )
            ));

            return View(contactInfo);
        }
        [HttpGet]
        public async Task<IActionResult> UpdateProject(Guid projectId)
        {
            var selectedProject = await Api.GetProjectAsync(XeroToken.AccessToken, TenantId, projectId);
            return View(selectedProject);
        }

        #endregion


        [HttpPost]
        public async Task<IActionResult> CreateProject(string contactId, string name, string estimateAmount)
        {
            var newProject = new ProjectCreateOrUpdate
            {
                Name = name,
                EstimateAmount = decimal.Parse(estimateAmount),
                ContactId = Guid.Parse(contactId),
                DeadlineUtc = DateTime.Today.AddDays(10)
            };

            await Api.CreateProjectAsync(XeroToken.AccessToken, TenantId, newProject);
  
            await Task.Delay(300);

            return RedirectToAction("GetProjects");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProject(string projectId, string name, string estimateAmount)
        {
            var updatedProject = new ProjectCreateOrUpdate
            {
                Name = name,
                EstimateAmount = decimal.Parse(estimateAmount)
            };

            await Api.UpdateProjectAsync(XeroToken.AccessToken, TenantId, Guid.Parse(projectId), updatedProject);

           
            await Task.Delay(300);

            return RedirectToAction("GetProjects");
        }
    }
}