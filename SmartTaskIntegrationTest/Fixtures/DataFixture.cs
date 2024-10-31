using SmartTaskManagementAPI.TaskCategory.model;
using SmartTaskManagementAPI.TaskManagement.model;

namespace SmartTaskIntegrationTest.Fixtures;

public static class DataFixture
{
    public static List<TaskManagement> GetTaskManagement()
    {
        return
        [
            new TaskManagement()
            {
                TaskId = new Guid("212bd5f9-d108-4dc4-9ef4-03fbe237ad74"),
                Title = "Sample Task1", 
                Status = "Open",
                Description = "description1",
                DueDate = DateTime.Now,
                Priority = "High",
                TaskCategory = new TaskCategory()
                {
                    Name = ""
                },
                TaskCategoryId = 100L

            },

            new TaskManagement()
            {
                TaskId = new Guid("3fc52bc0-70d2-4301-89c1-f6cbd271cae8"),
                Title = "Sample Task2", 
                Status = "Close",
                Description = "description1 and description1",
                DueDate = DateTime.Now,
                Priority = "High",
                TaskCategory = new TaskCategory()
                {
                    Name = ""
                },
                TaskCategoryId = 200L
            }
        ];
    }

    public static List<TaskCategory> GetTaskCategories()
    {
        return
        [
            new TaskCategory()
            {
                //TaskCategoryId = 400L,
                Name = "Work",
                Description = "",

            },
            new TaskCategory()
            {
                //TaskCategoryId = 500L,
                Name = "Personal",
                Description = "",

            }

        ];
    }
}