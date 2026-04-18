using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Shared.CommonResponses
{
    public class Error
    {
        public string Code { get; set; }

        public string Description { get; set; }

        public ErrorType ErrorType { get; set; }

        private Error(string code, string description, ErrorType errorType)
        {
            Code = code;
            Description = description;
            ErrorType = errorType;
        }

        public static Error Faliure(
            string code = "Genaral.Faliure",
            string description = "A general Faliure Has Occured"
        )
        {
            return new Error(code, description, ErrorType.Faliure);
        }

        public static Error Validation(
            string code = "Genaral.Validation",
            string description = "Validation Error Has Occured"
        )
        {
            return new Error(code, description, ErrorType.Validation);
        }

        public static Error NotFound(
            string code = "Genaral.NotFound",
            string description = "The Requested Resource was not found"
        )
        {
            return new Error(code, description, ErrorType.NotFound);
        }

        public static Error UnAuthorized(
            string code = "Genaral.UnAuthorized",
            string description = "You are Not Authorized to perform this action"
        )
        {
            return new Error(code, description, ErrorType.UnAuthorized);
        }

        public static Error Forbidden(
            string code = "Genaral.Forbidden",
            string description = "You don't have the access to this resource,Access denied"
        )
        {
            return new Error(code, description, ErrorType.Forbidden);
        }

        public static Error InvalidCredintals(
            string code = "Genaral.InvalidCredintals",
            string description = "Your Credintals is not valid to reach this resource"
        )
        {
            return new Error(code, description, ErrorType.InvalidCredintals);
        }
    }
}
