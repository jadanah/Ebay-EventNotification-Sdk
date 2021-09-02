/*
 * Copyright (c) 2021 eBay Inc.
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *
 */

using System;
using System.IO;
using System.Threading.Tasks;
using Ebay.EventNotification.Sdk.Constants;
using Ebay.EventNotification.Sdk.Models;
using Ebay.EventNotification.Sdk.Processor;
using Ebay.EventNotification.Sdk.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ebay.EventNotification.Sdk.AspNetCore.Controllers
{
    [ApiController]
    [Route("ebay-event-notification")]
    [Produces("application/json")]
    public class EventNotificationController : ControllerBase
    {
        private readonly ILogger<EventNotificationController> _logger;

        private readonly ISignatureValidator _signatureValidator;

        private readonly IEndPointValidator _endpointValidator;
        private readonly IMessageProcessorFactory _messageProcessorFactory;

        public EventNotificationController(ILogger<EventNotificationController> logger, ISignatureValidator validator, IEndPointValidator endPointValidator,
            IMessageProcessorFactory messageProcessorFactory)
        {
            _logger = logger;
            _signatureValidator = validator;
            _endpointValidator = endPointValidator;
            _messageProcessorFactory = messageProcessorFactory;
        }

        [HttpPost]
        [Route("webhook")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        // [ReadableBodyStream]
        public async Task<IActionResult> Process([FromBody] Message message, [FromHeader(Name = "X-EBAY-SIGNATURE")] string signatureHeader)
        {
            try
            {
                // Add ReadableBodyStream attribute to use
                // var body = await GetRequestBodyAsync();

                if (await _signatureValidator.ValidateAsync(message, signatureHeader))
                {
                    await ProcessAsync(message);
                    return NoContent();
                }
                else
                {
                    return StatusCode(StatusCodes.Status412PreconditionFailed);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Signature validation processing failure:" + e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [Route("webhook")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public ActionResult<ChallengeResponse> Validate([FromQuery(Name = "challenge_code")] string challengeCode)
        {
            try
            {
                return _endpointValidator.GenerateChallengeResponse(challengeCode);
            }
            catch (Exception ex)
            {
                _logger.LogError("Endpoint validation failure:" + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        
        private async Task ProcessAsync(Message message) => await _messageProcessorFactory.GetProcessor(Enum.Parse<TopicEnum>(message.Metadata.Topic)).ProcessAsync(message);
        
        private async Task<string> GetRequestBodyAsync()
        {
            // if you're late and body has already been read, you may need this next line
            // if "Note" is true and Body was read using StreamReader too, then it may be necessary to set "leaveOpen: true" for that stream.
            HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);

            string body = null;
            using var stream = new StreamReader(HttpContext.Request.Body);
            body = await stream.ReadToEndAsync();

            return body;
        }
    }
}