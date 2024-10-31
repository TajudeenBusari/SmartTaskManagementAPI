using SmartTaskManagementAPI.TaskManagement.model.dto;

namespace SmartTaskIntegrationTest.Helper;

public static class HttpHelper
{
    internal static class Urls
    {
        public static string GetAllTaskManagement = "api/v1/taskmanagement";
        public static string GetTaskManagementById(Guid id) => $"api/v1/taskmanagement/{id}";
        public static string AddTaskManagement = "api/v1/taskmanagement";
        public static string UpdateTaskManagement(Guid id) => $"api/v1/taskmanagement/{id}";
        public static string DeleteTaskManagement(Guid id) => $"api/v1/taskmanagement/{id}";
    }
}