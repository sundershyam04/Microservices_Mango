using AutoMapper;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ProductAPI.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductAPIController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ResponseDto _response;

        public ProductAPIController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            _response = new ResponseDto();
        }
        [HttpGet]
        public ResponseDto Get()
        {
            try
            {
                IEnumerable<Product> prodList = _db.Products.ToList();
                _response.Result = _mapper.Map<IEnumerable<ProductDto>>(prodList);
                return _response;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return _response;
            }
        }
		
		//[Route("{id}")]
		[HttpGet("{id}")]
		public ResponseDto Get(int id)
		{
			try
			{
				Product product = _db.Products.First(res => res.Id == id);
				_response.Result = _mapper.Map<ProductDto>(product);
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.Message = ex.Message;				
			}
			return _response;
		}
		
        [HttpPost]
		[Authorize(Roles = "ADMIN")]
		public ResponseDto Post(ProductDto productDto)
		{
			try
			{
                 var product= _mapper.Map<Product>(productDto);
                _db.Products.Add(product);
                _db.SaveChanges();

				//if Image i.e.,FormFile type present in productDto
				if(productDto.Image !=null)
				{
					string fileName = product.Id + Path.GetExtension(productDto.Image.FileName);
					string filePath = @"wwwroot\ProductImages\"+ fileName;
					var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(),filePath);
					using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
					{
						productDto.Image.CopyTo(fileStream);
					} 
					// copy product image to rootfolder of API server
					var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
					product.ImageUrl = baseUrl + "/ProductImages/"+ fileName;
					product.ImageLocalPath = filePath;
				}
				else{
					product.ImageUrl = "https://placehold.co/600x400";
                }
				_db.Products.Update(product);
				_db.SaveChanges();
				_response.Result = _mapper.Map<ProductDto>(product);			              
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.Message = ex.Message;
				
			}
			return _response;
		}

		[HttpPut]
		[Authorize(Roles = "ADMIN")]
		public ResponseDto Put(ProductDto productDto)
		{
			try
			{
				var product = _mapper.Map<Product>(productDto);
				// if product image is present - delete existing image ffrom wwwroot of API and store new img
				// [1] Delete existing image
				if (productDto.Image != null)
				{
                    if (!string.IsNullOrEmpty(product.ImageLocalPath))
                    { // Image of the prod to be deleted - remove from wwroot
                        var pathToDelete = Path.Combine(Directory.GetCurrentDirectory(), product.ImageLocalPath);
                        var file = new FileInfo(pathToDelete);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }
                    // [2] Add new image
					string fileName = product.Id + Path.GetExtension(productDto.Image.FileName);
					string filePath = @"wwwroot\ProductImages\" + fileName;
					var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);
					using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
					{
						productDto.Image.CopyTo(fileStream);
					}
					// copy product image to rootfolder of API server
					var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
					product.ImageUrl = baseUrl + "/ProductImages/" + fileName;
					product.ImageLocalPath = filePath;
				}
                _db.Products.Update(product);
				_db.SaveChanges();
				_response.Result = _mapper.Map<ProductDto>(product);
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.Message = ex.Message;

			}
			return _response;
		}
		//[Route("{id}")]
		[HttpDelete("{id}")]
		[Authorize(Roles = "ADMIN")]
		public ResponseDto Delete(int id)
		{
			try
			{
				Product product = _db.Products.First(res => res.Id == id);
				if (!string.IsNullOrEmpty(product.ImageLocalPath))
				{ // Image of the prod to be deleted - remove from wwroot
					var pathToDelete = Path.Combine(Directory.GetCurrentDirectory(), product.ImageLocalPath);
					var file = new FileInfo(pathToDelete);
					if (file.Exists)
					{
						file.Delete();
					}
				}
				_db.Products.Remove(product);
				_db.SaveChanges();
				_response.Result = _mapper.Map<ProductDto>(product); // To mention prod which deleted
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.Message = ex.Message;
			}
			return _response;
		}


	}
}
