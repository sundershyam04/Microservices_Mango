using Mango.Services.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service
{
    public class TokenProvider : ITokenProvider
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public TokenProvider(IHttpContextAccessor contextAccessor) => _contextAccessor = contextAccessor;
        public void ClearToken()
        {
            _contextAccessor.HttpContext?.Response.Cookies.Delete(SD.TokenCookie);
        }

        // Get token from cookie( to access other resources) via httpreq
        // - from the http request sent to the server from client
        //  client ----httpreq--->server : 
        public string? GetToken()
        {
           string? token = null;
           bool? hasToken = _contextAccessor.HttpContext?.Request.Cookies.TryGetValue(SD.TokenCookie, out token);
            return hasToken is true ? token : null;
        }

        // In server side code, instructions in HttpResponse given as to delete cookie/set 
        // to be sent to client(browser)
        public void SetToken(string token)
        {
            _contextAccessor.HttpContext?.Response.Cookies.Append(SD.TokenCookie, token);
        }
    }
}
