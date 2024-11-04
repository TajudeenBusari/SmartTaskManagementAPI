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
        
        public static string GetAllCategories = "api/v1/taskcategory";
        public static string GetCategoryById(long id) => $"api/v1/taskcategory/{id}";
        public static string UpdateCategoryById(long id) => $"api/v1/taskcategory/{id}";
        
        public static string AddCategory = "api/v1/taskcategory";
        public static string AssignTaskToCategory(long categoryId, Guid taskId) => $"api/v1/taskcategory/{categoryId}/tasks/{taskId}";
        public static string DeleteCategory(long id) => $"api/v1/taskcategory/{id}";
    }
}