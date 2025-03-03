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

public class Controller 
{
	public Controller()
	{
        [HttpPost("getHeritageData", Name = "getHeritageData")]
        public async Task<IActionResult> PostMedication([FromBody] Medication medication )
        {
            try
            {
                _logger.LogInformation("Starting Posting Process");
                HttpResponseMessage responseMessage = new HttpResponseMessage();

                responseMessage = await _heritageService.GetHeritageData(accessToken, searchRequest);

                return responseMessage.StatusCode switch
                {
                    HttpStatusCode.OK => await HandleSingleItemOkStatus<SuccessStatus>(responseMessage),
                    HttpStatusCode.BadRequest => await HandleBadRequestError(responseMessage, $"unable to process event"),
                    HttpStatusCode.Unauthorized => await HandleUnauthorizedError($"Unauthorized"),
                    HttpStatusCode.NotFound => await HandleNotFoundError(responseMessage, $"Not Found"),
                    _ => HandleServerErrorStatus(responseMessage, "Unable to process event")
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, new EventId(999999, "Server error"), ex, "Service not responding");
                return StatusCode(500);
            }
        }
    }
}
