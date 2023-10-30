using DapperHelper.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DapperApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DapperController : ControllerBase
    {
        private readonly IDapper _dapper;
        public DapperController(IDapper dapper)
        {
            _dapper = dapper;
        }

        [HttpGet]
        public async Task<IEnumerable<DevicesViewModel>> GetAll()
        {
            return await _dapper.GetAll<DevicesViewModel>("SELECT * FROM Devices");
        }

        [HttpGet("WithParam")]
        public async Task<IEnumerable<DevicesViewModel>> GetAllWithParam([FromQuery] DevicesDto devicesDto)
        {
            return await _dapper.GetAllWithParams<DevicesViewModel, DevicesDto>("SELECT * FROM Devices Where DeviceId = @DeviceId", devicesDto);
        }


    }

    public class DevicesDto
    {
        public string DeviceId { get; set; }        
    }

    public class DevicesViewModel
    {
        public string DeviceId { get; set; }
        public string Code { get; set; }

        public string Name { get; set; }
    }
}
