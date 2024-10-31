namespace SmartTaskManagementIntegrationTests.TaskManagement.Helper;

public class HttpHelper
{
    internal static class Urls
    {
        static Guid id = Guid.NewGuid();
        public static string GetAllTaskManagement = "api/v1/taskmanagement";
        public static string GetTaskManagementById = "api/v1/taskmanagement/{id}";
        public static string AddTaskManagement = "api/v1/taskmanagement";
        public static string UpdateTaskManagement = "api/v1/taskmanagement/{id}";
        public static string DeleteTaskManagement = "api/v1/taskmanagement/{id}";
    }
}