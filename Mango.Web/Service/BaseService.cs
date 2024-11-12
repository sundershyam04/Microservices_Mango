using Mango.Web.Models;
using Mango.Web.Service.IService;
using Newtonsoft.Json;
using System.Text;
using static Mango.Web.Utility.SD;
using System.Net;
using Mango.Services.Web.Service.IService;

namespace Mango.Web.Service
{
    public class BaseService : IBaseService
    {

        //To create httpclient instance
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenProvider  _tokenProvider;
        public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider)
        {
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;
        }
        public async Task<ResponseDto?> SendAsync(RequestDto requestDto, bool withBearer = true)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient("MangoAPI");
                HttpResponseMessage apiResponse = null;
                HttpRequestMessage message = new();
                //: Populating message on request
                message.Headers.Add("Accept", "application/json");
                //message.Headers.Add("Content-Type", "application/json");
                // token code
                message.RequestUri = new Uri(requestDto.Url);

                if (requestDto.Data != null)
                {
                    // Info: Provides HTTP content based on a string.
                    // Object --> HttpContent (abstract) -->ByteArrayContent --> StringContent
                    // Json --> String (SERIALIZE)
                    message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
                }

                if (withBearer)
                {
                    var token = _tokenProvider.GetToken();
                    message.Headers.Add("Authorization", $"Bearer {token}");
                }
                //set apitype in message(httpreqmsg)

                switch (requestDto.ApiType)
                {
                    case ApiType.DELETE:
                        message.Method = HttpMethod.Delete;
                        break;
                    case ApiType.POST:
                        message.Method = HttpMethod.Post;
                        break;
                    case ApiType.PUT:
                        message.Method = HttpMethod.Put;
                        break;
                    default:
                        message.Method = HttpMethod.Get;
                        break;
                }

                apiResponse = await client.SendAsync(message);


                // Align apiResponse with ResponseDto to return resp

                switch (apiResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return new() { IsSuccess = false, Message = "Not Found" };
                    case HttpStatusCode.Forbidden:          //no access rights to request resrc
                        return new() { IsSuccess = false, Message = "Access denied" };
                    case HttpStatusCode.Unauthorized:
                        return new() { IsSuccess = false, Message = "Unauthorized" };
                    case HttpStatusCode.InternalServerError:
                        return new() { IsSuccess = false, Message = "Internal Server error" };
                    case HttpStatusCode.BadRequest:
                        return new() { IsSuccess = false, Message = "Request not understood" };
                    default:
                        var apiContent = await apiResponse.Content.ReadAsStringAsync();
                        var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
                        return apiResponseDto;
                }
            }
            catch (Exception ex)
            {
                ResponseDto respDto = new() { IsSuccess = false, Message = ex.Message };
                return respDto;
            }

        }
    }
}