namespace Beseler.Shared.Extensions;
public static class TaskExt
{
    public static async Task WhenAll(params Task[] tasks)
    {
        var taskOfTasks = Task.WhenAll(tasks);
        try
        {
            await taskOfTasks;
        }
        catch (Exception)
        {
            if (taskOfTasks.Exception is not null)
            {
                throw taskOfTasks.Exception;
            }

            throw;
        }
    }
}
