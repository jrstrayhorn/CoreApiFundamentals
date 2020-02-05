using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public CampsController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this._repository = repository;
            this._mapper = mapper;
            this._linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsAsync(includeTalks);

                return _mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
            
        }

        [HttpGet("{moniker}")]
        //[HttpGet("{moniker:int}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        //public async Task<ActionResult<CampModel>> Get(int moniker)
        {
            try
            {
                var result = await _repository.GetCampAsync(moniker);

                if (result == null) return NotFound();

                return _mapper.Map<CampModel>(result);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        // using the query string to give flexibility in creating multiple search
        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try 
	        {	        
		        var results = await _repository.GetAllCampsByEventDate(theDate, includeTalks);

                 if (!results.Any()) return NotFound();

                 return _mapper.Map<CampModel[]>(results);
	        }
	        catch (Exception)
	        {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
	        }
        }

        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try 
	        {
                var existing = await _repository.GetCampAsync(model.Moniker);
                if (existing != null)
                {
                    return BadRequest("Moniker in Use");
                }
                // if no APIController attribute you can use, but don't need to, will auto generate error messages and 400 Bad request
                //if (ModelState.IsValid)
                var location = _linkGenerator.GetPathByAction("Get", "Camps", new { moniker = model.Moniker });

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker");
                }

                // create a new Camp
                var camp = _mapper.Map<Camp>(model);
                _repository.Add(camp);
                if (await _repository.SaveChangesAsync())
                {
                    return Created(location, _mapper.Map<CampModel>(camp));
                }
	        }
	        catch (Exception ex)
	        {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure - " + ex.Message);
	        }

            return BadRequest();
        }
    }
}
