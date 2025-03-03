using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class MedicationController : ControllerBase
{
    private readonly ILoggerWrapper<MedicationController> _logger = StaticLoggingProvider.Create<MedicationController>();
    private readonly IMedicationService _medicationService;

    [HttpPost("postMedicationData", Name = "postMedicationData")]
    public async Task<IActionResult> PostMedication([FromBody] Medication medication)
    {
        try
        {
            _logger.LogInformation("Starting Posting Process");
            //adding responseMessage here to allow service to describe the success and failure.
            HttpResponseMessage responseMessage = new HttpResponseMessage();

            responseMessage = await _medicationService.GetHeritageData(medication);

            //adding switch statement in order to run the code block until one of the condition becomes true. 
            // after one of the condition becomes true , it would execute the relevant status condition. 
            return responseMessage.StatusCode switch
            {
                HttpStatusCode.OK => await SuccessStatus(responseMessage),
                HttpStatusCode.BadRequest => await BadRequestError(responseMessage, $"unable to process post"),
                HttpStatusCode.Unauthorized => await HandleUnauthorizedError($"Unauthorized"),
                HttpStatusCode.NotFound => await HandleNotFoundError(responseMessage, $"Not Found"),
                _ => HandleServerErrorStatus(responseMessage, "Unable to process event")
            };
            //using HandleServerErrorStatus in case none of the 4 above conditions and the service than responds with 500. 

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, new EventId(999999, "Server error"), ex, "Service not responding");
            return StatusCode(500);
        }

    }



    // An example for adding roles authentication to allow only authorised user to retrieve data.
   //[Authorize(Roles = "GetData")]

    //getMedicationData is the name that will appear on swagger once the service is ran
    [HttpGet("getMedicationData", Name = "getMedicationData")]
    [Consumes("application/json")]

    //This is an example of how we can add status codes that the endpoint with respond with. 
    //[ProducesResponseType(typeof(SuccessResponse>), (int)HttpStatusCode.OK)]

    // FromHeader takes the authorization which is passed in ModHeader to execute the endpoint.
    public async Task<IActionResult> GetHeritageData([FromHeader(Name = HeaderKeys.Authorization)] string accessToken, [FromQuery] MedicationStatus medicationStatus, DateTime prescribedDate)
    {
        try
        {
            //_logger (ILoggerWrapper) - allows us to monitor the service by viewing this LogInformation logs in kibana
            //this is good because we can view how the service is responding when it is ran on various environments and 
            //view any errors that it is throwing
            _logger.LogInformation("GetMedication Process Start");
            HttpResponseMessage responseMessage = new HttpResponseMessage();

            responseMessage = await _medicationService.GetMedicationData(accessToken, searchRequest);

            return responseMessage.StatusCode switch
            {
                HttpStatusCode.OK => await SuccessStatus(responseMessage),
                HttpStatusCode.BadRequest => await BadRequestError(responseMessage, $"unable to retrieve data "),
                HttpStatusCode.Unauthorized => await HandleUnauthorizedError($"Unauthorized"),
                HttpStatusCode.NotFound => await HandleNotFoundError(responseMessage, $"Not Found"),
                _ => HandleServerErrorStatus(responseMessage, "Unable to process your request")
            };

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, new EventId(999999, "Server error"), ex, "Service with error");
            return StatusCode(500);
        }
    }

    private async Task<IActionResult> SuccessStatus(HttpResponseMessage responseMessage)
    {
        //ReadAsStringAsync - this to serialise the HTTP code as a string
        // when the status is = 200 
        //than this block os code is executed and logging information to kibana 
        // and responding with 200 status code in swagger 
        var content = await responseMessage.Content?.ReadAsStringAsync();
        _logger.LogInformation("Successfully completed", content);
        return Ok(JsonConvert.DeserializeObject<T>(content));
    }


    private async Task<IActionResult> BadRequestError(HttpResponseMessage responseMessage, string message)
    {
        string content = await responseMessage.Content.ReadAsStringAsync();
        object errorResponse = JsonConvert.DeserializeObject<T>(content);
        string errorMessage = $"Bad Request from the service {message}";
        var validationStr = JsonConvert.SerializeObject(errorResponse, Formatting.Indented);
        _logger.LogWarning(errorMessage, validationStr);
        return BadRequest(errorResponse);
    }
}
