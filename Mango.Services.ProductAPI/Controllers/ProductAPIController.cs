﻿using AutoMapper;
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
		public ResponseDto Post([FromBody] ProductDto dtoModel)
		{
			try
			{
                 var obj= _mapper.Map<Product>(dtoModel);
                _db.Products.Add(obj);
                _db.SaveChanges();
				_response.Result = _mapper.Map<ProductDto>(obj);			              
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
		public ResponseDto Put([FromBody] ProductDto dtoModel)
		{
			try
			{
				var obj = _mapper.Map<Product>(dtoModel);
				_db.Products.Update(obj);
				_db.SaveChanges();
				_response.Result = _mapper.Map<ProductDto>(obj);
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
