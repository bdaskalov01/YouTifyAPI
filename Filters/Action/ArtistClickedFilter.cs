using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAPIProgram.Filters.Action;

public class ArtistClickedFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var arguments = context.ActionArguments;
        foreach (var arg in arguments)
        {
            Console.WriteLine($"{arg.Key}: {arg.Value}");
        }        
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Do nothing yet
    }
}