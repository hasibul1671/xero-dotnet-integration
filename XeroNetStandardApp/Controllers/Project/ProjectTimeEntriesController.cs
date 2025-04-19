using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Project;
using Task = Xero.NetStandard.OAuth2.Model.Project.Task;

namespace XeroNetStandardApp.Controllers.Project
{
    public class ProjectTimeEntriesController : ApiAccessorController<ProjectApi>
    {
        public ProjectTimeEntriesController(IOptions<XeroConfiguration> xeroConfig) : base(xeroConfig) { }

        #region GET Endpoints
        public async Task<IActionResult> GetTimeEntries(Guid projectId)
        {
            var selectedProject = await Api.GetProjectAsync(XeroToken.AccessToken, TenantId, projectId);
            var timeEntries = await Api.GetTimeEntriesAsync(XeroToken.AccessToken, TenantId, projectId);

            var timeEntryWrappers = new List<TimeEntryWrapper>();
            foreach (var timeEntry in timeEntries.Items)
            {
                var task = await Api.GetTaskAsync(XeroToken.AccessToken, TenantId, projectId, (Guid)timeEntry.TaskId);
                var users = await Api.GetProjectUsersAsync(XeroToken.AccessToken, TenantId);
                var userName = "";
                foreach (var user in users.Items)
                {
                    if (user.UserId == timeEntry.UserId)
                    {
                        userName = user.Name;
                        break;
                    }
                }

                timeEntryWrappers.Add(
                    new TimeEntryWrapper
                    {
                        Data = timeEntry,
                        UserName = userName,
                        TaskName = task.Name
                    }
                );
            }

            ViewBag.jsonResponse = timeEntries.ToJson();
            return View((selectedProject, timeEntryWrappers));
        }
        [HttpGet]
        public async Task<IActionResult> UpdateTimeEntry(Guid projectId, Guid timeEntryId)
        {
            var selectedTimeEntry = await Api.GetTimeEntryAsync(XeroToken.AccessToken, TenantId, projectId, timeEntryId);
            var users = await Api.GetProjectUsersAsync(XeroToken.AccessToken, TenantId);
            var tasks = await Api.GetTasksAsync(XeroToken.AccessToken, TenantId, projectId);

            return View("UpdateTimeEntry", model: new UpdateTimeEntryModel { Users = users.Items, Tasks = tasks.Items, TimeEntry = selectedTimeEntry });
        }
        [HttpGet]
        public async Task<IActionResult> DeleteTimeEntry(Guid projectId, Guid timeEntryId)
        {
            await Api.DeleteTimeEntryAsync(XeroToken.AccessToken, TenantId, projectId, timeEntryId);
            await System.Threading.Tasks.Task.Delay(300);
            return RedirectToAction("GetTimeEntries", new { projectId });
        }
        [HttpGet]
        public async Task<IActionResult> CreateTimeEntry(Guid projectId)
        {

            var tasks = await Api.GetTasksAsync(XeroToken.AccessToken, TenantId, projectId);
            var users = await Api.GetProjectUsersAsync(XeroToken.AccessToken, TenantId);
            return View("CreateTimeEntry", new CreateTimeEntryModel { ProjectId = projectId, Tasks = tasks.Items, Users = users.Items });
        }

        #endregion

        #region POST Endpoints
        [HttpPost]
        public async Task<IActionResult> UpdateTimeEntry(Guid projectId, Guid timeEntryId, Guid userId, Guid taskId, int duration)
        {

            var updatedTimeEntry = new TimeEntryCreateOrUpdate
            {
                UserId = userId,
                Duration = duration,
                TaskId = taskId
            };
            await System.Threading.Tasks.Task.Delay(300);

            await Api.UpdateTimeEntryAsync(XeroToken.AccessToken, TenantId, projectId, timeEntryId, updatedTimeEntry);

            return RedirectToAction("GetTimeEntries", new { projectId });
        }
        [HttpPost]
        public async Task<IActionResult> CreateTimeEntry(Guid projectId, Guid userId, Guid taskId, int duration)
        {
            var newTimeEntry = new TimeEntryCreateOrUpdate
            {
                UserId = userId,
                Duration = duration,
                TaskId = taskId
            };

            await Api.CreateTimeEntryAsync(XeroToken.AccessToken, TenantId, projectId, newTimeEntry);
            await System.Threading.Tasks.Task.Delay(300);
            return RedirectToAction("GetTimeEntries", new { projectId });
        }

        #endregion

    }

    #region Models
    public class CreateTimeEntryModel
    {
        public List<Task> Tasks { get; set; }
        public List<ProjectUser> Users { get; set; }
        public Guid ProjectId { get; set; }
    }
    public class UpdateTimeEntryModel
    {
        public List<ProjectUser> Users { get; set; }
        public List<Task> Tasks { get; set; }
        public TimeEntry TimeEntry { get; set; }
    }
    public class TimeEntryWrapper
    {
        public TimeEntry Data { get; set; }
        public string TaskName { get; set; }
        public string UserName { get; set; }
    }

    #endregion
}
