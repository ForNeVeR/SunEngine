using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SunEngine.Commons.Cache.Services;
using SunEngine.Commons.Configuration.Options;
using SunEngine.Commons.Managers;
using SunEngine.Commons.Models;

namespace SunEngine.Commons.Security
{
    /// <summary>
    /// Jwt validation handler with system of 3 tokens
    /// 1 - ShortToken (Access token), stored in client JS or localStorage Short token life approximate 5 minutes to 2 days
    /// 2 - LongToken1 (Refresh token), stored in client JS or localStorage. Long token life ~ 3 month.
    /// 3 - LongToken2 (Access + Refresh token, 2 in 1), stored in cookie Long token life ~ 3 month.
    /// LongToken2 needed to verify ShortToken and LongToken1 to protect against XSS attacks.
    /// </summary>
    public class SunJwtHandler : AuthenticationHandler<SunJwtOptions>
    {
        private readonly IRolesCache rolesCache;
        private readonly JwtOptions jwtOptions;
        private readonly JwtService jwtService;
        private readonly SunUserManager userManager;
        private readonly JwtBlackListService jwtBlackListService;

        public SunJwtHandler(
            IOptionsMonitor<SunJwtOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IRolesCache rolesCache,
            IOptions<JwtOptions> jwtOptions,
            JwtService jwtService,
            JwtBlackListService jwtBlackListService,
            SunUserManager userManager) : base(options, logger, encoder, clock)
        {
            this.rolesCache = rolesCache;
            this.jwtOptions = jwtOptions.Value;
            this.jwtService = jwtService;
            this.userManager = userManager;
            this.jwtBlackListService = jwtBlackListService;
        }


        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            AuthenticateResult ErrorAuthorization()
            {
                jwtService.MakeLogoutCookiesAndHeaders(Response);

                return AuthenticateResult.NoResult();
            }

            try
            {
                var cookie = Request.Cookies[TokenClaimNames.LongToken2CoockiName];

                if (cookie == null)
                    return AuthenticateResult.NoResult();


                JwtSecurityToken jwtLongToken2 = jwtService.ReadLongToken2(cookie);
                if (jwtLongToken2 == null)
                    return ErrorAuthorization();

                var longToken2db = jwtLongToken2.Claims.First(x => x.Type == TokenClaimNames.LongToken2Db).Value;

                SunClaimsPrincipal sunClaimsPrincipal;

                if (Request.Headers.ContainsKey(Headers.LongToken1HeaderName))
                {
                    string longToken1db = Request.Headers[Headers.LongToken1HeaderName];
                    int userId = int.Parse(jwtLongToken2.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);

                    var longSessionToFind = new LongSession
                    {
                        UserId = userId,
                        LongToken1 = longToken1db,
                        LongToken2 = longToken2db
                    };

                    var longSession = userManager.FindLongSession(longSessionToFind);

                    if (longSession == null)
                        return ErrorAuthorization();

                    sunClaimsPrincipal = await jwtService.RenewSecurityTokensAsync(Response, userId, longSession);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nToken renews\n");
                    Console.ResetColor();
                }
                else
                {
                    string authorization = Request.Headers["Authorization"];

                    if (string.IsNullOrEmpty(authorization))
                        return AuthenticateResult.NoResult();

                    string jwtShortToken = null;
                    if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        jwtShortToken = authorization.Substring("Bearer ".Length).Trim();

                    if (string.IsNullOrEmpty(jwtShortToken))
                        return AuthenticateResult.NoResult();


                    var claimsPrincipal =
                        jwtService.ReadShortToken(jwtShortToken, out SecurityToken shortToken);

                    string lat2ran_1 = jwtLongToken2.Claims.FirstOrDefault(x => x.Type == TokenClaimNames.LongToken2Ran).Value;
                    string lat2ran_2 = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == TokenClaimNames.LongToken2Ran).Value;

                    if (!string.Equals(lat2ran_1, lat2ran_2))
                        return ErrorAuthorization();

                    long sessionId = long.Parse(jwtLongToken2.Claims.FirstOrDefault(x => x.Type == TokenClaimNames.SessionId).Value);

                    string lat2db = jwtLongToken2.Claims.FirstOrDefault(x => x.Type == TokenClaimNames.LongToken2Db).Value;

                    sunClaimsPrincipal = new SunClaimsPrincipal(claimsPrincipal, rolesCache, sessionId, lat2db);
                }

                if (jwtBlackListService.IsTokenNotInBlackList(sunClaimsPrincipal.LongToken2Db))
                    return ErrorAuthorization();

                if (sunClaimsPrincipal.Roles.ContainsKey(RoleNames.Banned))
                    return ErrorAuthorization();

                var authenticationTicket = new AuthenticationTicket(sunClaimsPrincipal, SunJwt.Scheme);
                return AuthenticateResult.Success(authenticationTicket);
            }
            catch (Exception e)
            {
                return ErrorAuthorization();
            }
        }
    }

    public class SunJwtOptions : AuthenticationSchemeOptions
    {
    }

    public static class SunJwt
    {
        public const string Scheme = "MyScheme";
    }
}