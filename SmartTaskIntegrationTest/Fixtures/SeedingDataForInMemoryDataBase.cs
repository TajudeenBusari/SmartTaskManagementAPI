using SmartTaskManagementAPI.AppUser.models;
using SmartTaskManagementAPI.AppUser.models.dto;
using SmartTaskManagementAPI.Data;
using SmartTaskManagementAPI.TaskCategory.model;
using SmartTaskManagementAPI.TaskManagement.model;

namespace SmartTaskIntegrationTest.Fixtures;

public class SeedingDataForInMemoryDataBase
{
    public static void InitializeTestDb(ApplicationDbContext dbContext)
    {
        dbContext.AddRange(GetCategory());
        dbContext.AddRange(GetTask());
        dbContext.SaveChanges();
    }

    private static List<TaskManagement> GetTask()
    {
        return new List<TaskManagement>()
        {
            new TaskManagement()
            {
                TaskId = new Guid("c938a235-8a75-40d8-861f-b8e0f5498d6c"),
                Title = "Title1",
                Description = "Description1",
                DueDate = DateTime.Now,
                Status = "Status1",
                Priority = "Priority1",
                TaskCategoryId = 400L
            },
            new TaskManagement()
            {
                TaskId = new Guid("b94160f4-1d0b-49c8-8fd5-3e344ec915a6"),
                Title = "Title2",
                Description = "Description2",
                DueDate = DateTime.Now,
                Status = "Status2",
                Priority = "Priority2",
                TaskCategoryId = 500L
            }
        };

    }

    private static List<TaskCategory> GetCategory()
    {
        return new List<TaskCategory>()
        {
            new TaskCategory()
            {
                TaskCategoryId = 400,
                Name = "Name3",
                Description = "Description1",
            },
            
            new TaskCategory()
            {
            TaskCategoryId = 500,
            Name = "Name4",
            Description = "Description2",
            } 
        };

    }

    /*
     /// <summary>
    /// since I have some users already in the ApplicationDbContext,
    /// I will not seed any user and their roles in the custom class
    /// </summary>
    /// <returns></returns>
    private static List<ApplicationUser> GetUser()
    {
        return new List<ApplicationUser>()
        {
            new ApplicationUser()
            {

            }
        };
    }*/
}