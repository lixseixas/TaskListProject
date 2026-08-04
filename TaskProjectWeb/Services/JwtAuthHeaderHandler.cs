using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TaskProject.Services
{
    public class JwtAuthHeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtAuthHeaderHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var tokenClaim = httpContext.User.FindFirst("JWT");
                if (tokenClaim != null && !string.IsNullOrEmpty(tokenClaim.Value))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenClaim.Value);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
