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
using Ebay.EventNotification.Sdk.Utils;

namespace Ebay.EventNotification.Sdk.Processor
{
    public abstract class BaseMessageProcessor
    {
        private readonly IMessageSerializer _serializer;
        protected BaseMessageProcessor() => _serializer = new MessageSerializer();
        public abstract Task ProcessAsync(Message message);
        private string GetJsonString(object obj) => _serializer.Serialize(obj);
        protected T Deserialize<T>(object obj) => _serializer.Deserialize<T>(obj);
    }
}