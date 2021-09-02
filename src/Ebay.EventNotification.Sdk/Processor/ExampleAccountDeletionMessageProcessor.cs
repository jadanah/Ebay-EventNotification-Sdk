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

using System.Threading.Tasks;
using Ebay.EventNotification.Sdk.Models;
using Microsoft.Extensions.Logging;

namespace Ebay.EventNotification.Sdk.Processor
{
    public class ExampleAccountDeletionMessageProcessor : BaseMessageProcessor, IMessageProcessor<AccountDeletionData>
    {
        private readonly ILogger<ExampleAccountDeletionMessageProcessor> _logger;

        public ExampleAccountDeletionMessageProcessor(ILogger<ExampleAccountDeletionMessageProcessor> logger)
        {
            _logger = logger;
        }

        public override Task ProcessAsync(Message message)
        {
            var accountDeletionData = Deserialize<AccountDeletionData>(message.Notification.Data);

            _logger.LogInformation("AccountDeletionRequested: {@AccountDeletionData}", accountDeletionData);

            return Task.CompletedTask;
        }
    }
}