using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Shared.DTOs.IdentityDTOs
{
    public record UserDTO(string Email, string DisplayName, string Token);
}
