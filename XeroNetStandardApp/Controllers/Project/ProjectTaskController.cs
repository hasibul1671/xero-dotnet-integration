using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Project;
using Task = System.Threading.Tasks.Task;

namespace XeroNetStandardApp.Controllers.Project
{
    public class ProjectTaskController : ApiAccessorController<ProjectApi>
    {
        public ProjectTaskController(IOptions<XeroConfiguration> xeroConfig) : base(xeroConfig) { }

        public async Task<IActionResult> GetTasks(Guid projectId)
        {
            var selectedProject = await Api.GetProjectAsync(XeroToken.AccessToken, TenantId, projectId);
            var tasks = await Api.GetTasksAsync(XeroToken.AccessToken, TenantId, projectId);

            ViewBag.jsonResponse = tasks.ToJson();

            return View((selectedProject.Name, tasks)); 
        }
        [HttpGet]
        public async Task<IActionResult> DeleteTask(Guid projectId, Guid taskId)
        {
            await Api.DeleteTaskAsync(XeroToken.AccessToken, TenantId, projectId, taskId);
            await Task.Delay(300);
            return RedirectToAction("GetTasks", new { projectId });
        }

        [HttpGet]
        public IActionResult CreateTask(Guid projectId, CurrencyCode currencyCode)
        {
            return View("CreateTask",new CreateTaskGetModel { ProjectId = projectId, CurrencyCode = currencyCode });
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(Guid projectId, CurrencyCode currencyCode, string taskName, string taskRate, string estimateMinute, string taskChargeType)
        {
            var newTask = new TaskCreateOrUpdate
            {
                Name = taskName,
                Rate = new Amount{Currency = currencyCode, Value = decimal.Parse(taskRate)},
                EstimateMinutes = int.Parse(estimateMinute),
                ChargeType = (ChargeType)Enum.Parse(typeof(ChargeType), taskChargeType)
            };

            await Api.CreateTaskAsync(XeroToken.AccessToken, TenantId, projectId, newTask);

            return RedirectToAction("GetTasks", new { projectId });
        }
        [HttpGet]
        public async Task<IActionResult> UpdateTask(Guid projectId, Guid taskId)
        {
            var selectedTask = await Api.GetTaskAsync(XeroToken.AccessToken, TenantId, projectId, taskId);

            return View("UpdateTask", selectedTask);
        }
        public async Task<IActionResult> UpdateTask(Guid projectId, Guid taskId, string taskName, string taskRate, string estimateMinute, string taskChargeType)
        {
            var associatedProject = await Api.GetProjectAsync(XeroToken.AccessToken, TenantId, projectId);

            var updatedTask = new TaskCreateOrUpdate
            {
                Name = taskName,
                Rate = new Amount { Currency = associatedProject.Estimate.Currency, Value = decimal.Parse(taskRate) },
                EstimateMinutes = int.Parse(estimateMinute),
                ChargeType = (ChargeType)Enum.Parse(typeof(ChargeType), taskChargeType)
            };

            await Api.UpdateTaskAsync(XeroToken.AccessToken, TenantId, projectId, taskId, updatedTask);

            return RedirectToAction("GetTasks", new { projectId });
        }

    }
    public class CreateTaskGetModel
    {
        public Guid ProjectId { get; set; }
        public CurrencyCode CurrencyCode { get; set; }
    }
}
