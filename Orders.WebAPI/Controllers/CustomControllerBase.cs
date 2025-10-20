using Microsoft.AspNetCore.Mvc;
using Orders.WebAPI.Filters.ExceptionFilters;

namespace Orders.WebAPI.Controllers
{
    [ApiController]
    [Route("api/")]
    [TypeFilter(typeof(HandleExceptionFilter))]

    public class CustomControllerBase : ControllerBase
    {

    }
}
