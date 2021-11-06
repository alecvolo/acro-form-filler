using System.Text.Json;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PdfFiller.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}")]
    public class FormFillerController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FormFillerController> _logger;

        public FormFillerController(IMediator mediator, ILogger<FormFillerController> logger)
        {
            _logger = logger;
            _mediator = mediator;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Returns filled pdf</response>
        [ApiVersion("1.0")]
        [HttpPost("produce-pdf/{formId}")]
        [Produces("application/pdf", "application/json")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProducePdf([FromRoute] string formId, [FromBody] JsonElement jsonData, [FromQuery] string resultFileName)
        {
            if (jsonData.ValueKind != JsonValueKind.Object)
            {
                ModelState.AddModelError("", "request body should be a JSON object");
               return ValidationProblem();
            }

            try
            {
                var content = await _mediator.Send(new Commands.FillPdf.Command { FormId = formId, JsonData = jsonData }).ConfigureAwait(false);
                return new FileContentResult(content, "application/pdf")
                {
                    FileDownloadName = string.IsNullOrWhiteSpace(resultFileName)
                        ? "result.pdf"
                        : resultFileName
                };
            }
            catch (Commands.FillPdf.NotFoundException)
            {
                return NotFound();
            }
        }
    }
}
