using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SunEngine.Commons.Cache.CacheModels;
using SunEngine.Commons.Cache.Services;
using SunEngine.Commons.Managers;
using SunEngine.Commons.Misc;
using SunEngine.Commons.Models;
using SunEngine.Commons.Security;
using SunEngine.Commons.Utils;

namespace SunEngine.Commons.Controllers
{
    /// <summary>
    /// Base class for all controllers on the site
    /// </summary>
    public abstract class BaseController : Controller
    {
        protected readonly SunUserManager userManager;
        protected readonly IRolesCache rolesCache;
        protected readonly IContentCache contentCache;
        protected readonly CacheKeyGenerator keyGenerator;

        protected BaseController(IServiceProvider serviceProvider)
        {
            contentCache = serviceProvider.GetRequiredService<IContentCache>();
            rolesCache = serviceProvider.GetRequiredService<IRolesCache>();
            userManager = serviceProvider.GetRequiredService<SunUserManager>();
            keyGenerator = serviceProvider.GetRequiredService<CacheKeyGenerator>();
        }
        
        

        protected string ControllerName
        {
            get => ControllerContext.ActionDescriptor.ControllerName;
        }

        protected string ActionName
        {
            get => ControllerContext.ActionDescriptor.ActionName;
        }

        private SunClaimsPrincipal _user;

        public new SunClaimsPrincipal User
        {
            get
            {
                if (_user == null)
                {
                    SunClaimsPrincipal sunClaimsPrincipal = base.User as SunClaimsPrincipal;
                    _user = sunClaimsPrincipal ?? new SunClaimsPrincipal(base.User, rolesCache);
                }

                return _user;
            }
        }

        public new UnauthorizedObjectResult Unauthorized()
        {
            return base.Unauthorized(ErrorView.Unauthorized());
        }

        public new BadRequestObjectResult BadRequest()
        {
            return base.BadRequest(ErrorView.BadRequest());
        }
        
        public Task<User> GetUserAsync()
        {
            return userManager.FindByIdAsync(User.UserId.ToString());
        }

        public IActionResult JsonString(string json)
        {
            return Content(json, "application/json", Encoding.UTF8);
        }

        public async Task<IActionResult> CacheContentAsync<T>(CategoryCached category, IEnumerable<int> categoryIds,
            Func<Task<T>> dataLoader, int page = 0)
        {
            var key = keyGenerator.ContentGenerateKey(ControllerName, ActionName, page, categoryIds);
            return await CacheContentAsync(category, key, dataLoader);
        }

        public async Task<IActionResult> CacheContentAsync<T>(CategoryCached category, int categoryId,
            Func<Task<T>> dataLoader, int page = 0)
        {
            var key = keyGenerator.ContentGenerateKey(ControllerName, ActionName, page, categoryId);
            return await CacheContentAsync(category, key, dataLoader);
        }

        protected async Task<IActionResult> CacheContentAsync<T>(CategoryCached category, string key,
            Func<Task<T>> dataLoader)
        {
            string json;
            /*if (category != null
                && category.IsCacheContent
                && !string.IsNullOrEmpty(json = contentCache.GetContent(key)))
            {
                return JsonString(json);
            }*/

            var content = await dataLoader();
            json = WebJson.Serialize(content);
            //contentCache.CacheContent(key, json);
            return JsonString(json);
        }

        protected override void Dispose(bool disposing)
        {
            userManager.Dispose();
            base.Dispose(disposing);
        }
    }

    
}