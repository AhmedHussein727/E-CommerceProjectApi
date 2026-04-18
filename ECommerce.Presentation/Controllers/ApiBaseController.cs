using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Shared.CommonResponses;
using ECommerce.Shared.DTOs.ProductDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ECommerce.Presentation.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class ApiBaseController : ControllerBase
    {
        //Handle Result  without Value
        //If Result is Success=> with No Content 204
        //If Result is  Faliure=>return problem Details along with desc,Status code

        protected IActionResult HandleResult(Result result)
        {
            if (result.IsSuccess)
                return NoContent();
            else
                return HandleProblem(result.Errors);
        }

        //Handle Result with Value
        //If Result is Success=> With Value OK 200
        // If Result is Faliure =>return problem Details along with desc,Status code


        protected string GetEmailFromToken() => User.FindFirstValue(ClaimTypes.Email)!;

        protected ActionResult<TValue> HandleResult<TValue>(Result<TValue> result)
        {
            if (result.IsSuccess)
                return Ok(result.Value);
            else
                return HandleProblem(result.Errors);
        }

        private ActionResult HandleProblem(IReadOnlyList<Error> errors)
        {
            // If NO Errors are provided , return 500 Error

            if (errors.Count == 0)
            {
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "An Error Occurred"
                );
            }

            //If All Errors are validation Errors handle them as a validation Problem
            if (errors.All(E => E.ErrorType == ErrorType.Validation))
                return HandleValidationErrors(errors);

            //If there is only one error ,Handle as single error problem

            return HandleSingleError(errors[0]);
        }

        private ActionResult HandleSingleError(Error error)
        {
            return Problem(
                title: error.Code,
                detail: error.Description,
                type: error.ErrorType.ToString(),
                statusCode: MapErrorTypeIntoStatusCode(error.ErrorType)
            );
        }

        private ActionResult HandleValidationErrors(IReadOnlyList<Error> errors)
        {
            var modelState = new ModelStateDictionary();

            foreach (var error in errors)
            {
                modelState.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem(modelState);
        }

        private static int MapErrorTypeIntoStatusCode(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.UnAuthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.InvalidCredintals => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError,
            };
    }
}
