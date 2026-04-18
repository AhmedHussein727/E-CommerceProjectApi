using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Services.Exceptions
{
    public abstract class NotFoundException(string message) : Exception(message) { }

    public sealed class ProductNotFoundException(int id)
        : NotFoundException($"Product with Id:{id} is Not Found") { }

    public sealed class BasketNotFoundException(string id)
        : NotFoundException($"Basket with Id : {id} is not found") { }
}
