using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using SunEngine.Controllers;
using SunEngine.Services;
using SunEngine.Stores;

namespace SunEngine.Infrastructure
{
    public class CaptchaValidationFilter : ActionFilterAttribute
    {        
        
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            CaptchaService captchaService =
                context.HttpContext.RequestServices.GetRequiredService<CaptchaService>();
            
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (descriptor != null)
            {
                var parameters = descriptor.MethodInfo.GetParameters();

                var modelParameter = parameters.FirstOrDefault(x=>x.ParameterType.IsSubclassOf(typeof(CaptchaViewModel)));
                
                var model = (CaptchaViewModel)context.ActionArguments[modelParameter.Name];
                
                if (!captchaService.VerifyToken(model.CaptchaToken, model.CaptchaText))
                {
                    context.Result = ((Controller)context.Controller).BadRequest(new ErrorViewModel
                    {
                        ErrorName = "CaptchaValidationError",
                        ErrorText = "Ошибка проверки капчи."
                    });
                }
                
                base.OnActionExecuting(context);
            }
        }

        
    }
}