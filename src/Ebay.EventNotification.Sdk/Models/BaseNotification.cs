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
using System.Text.Json.Serialization;

namespace Ebay.EventNotification.Sdk.Models
{
    public class BaseNotification
    {
        public BaseNotification()
        {
        }

        [JsonPropertyName("notificationId")]
        public string NotificationId { get; set; }

        [JsonPropertyName("eventDate")]
        public DateTime EventDate { get; set; }

        [JsonPropertyName("publishDate")]
        public DateTime PublishDate { get; set; }

        [JsonPropertyName("publishAttemptCount")]
        public int PublishAttemptCount { get; set; }

        [JsonPropertyName("data")]
        public object Data { get; set; }
    }
}