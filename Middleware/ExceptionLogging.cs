using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Penguin.Cms.Errors;
using Penguin.Cms.Errors.Extensions;
using Penguin.Persistence.Abstractions.Interfaces;
using Penguin.Security.Abstractions.Interfaces;
using Penguin.Web.Abstractions.Interfaces;
using System;

using System.Threading.Tasks;

namespace Penguin.Cms.Web.Errors.Middleware
{
    //https://exceptionnotfound.net/using-middleware-to-log-requests-and-responses-in-asp-net-core/
    public class ExceptionLogging : IPenguinMiddleware
    {
        private readonly RequestDelegate _next;

        //TODO: Learn what this is
        public ExceptionLogging(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            try
            {
                await this._next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                IRepository<AuditableError> errorRepository = context.RequestServices.GetService<IRepository<AuditableError>>();

                _ = errorRepository.TryAdd(ex, false, context.Request.Path.Value, context.RequestServices.GetService<IUserSession>()?.LoggedInUser?.Guid);

                throw;
            }
        }
    }
}