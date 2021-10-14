# Event Notification SDK - Unofficial

**Forked from https://github.com/eBay/eBay-Notification-SDK-Dot-Net-Core. I've made many changes from the original code for improved DI integration, async support, removing unrequired dependencies, .net naming conventions/code styles etc.**
 
============================================================================

With notifications, business moments are communicated to all interested listeners a.k.a. subscribers of those event streams. eBay's most recent notification payloads are also secured using ECC signature headers.  

Event Notification DotNet SDK  is designed to simplify processing eBay notifications. The application receives subscribed messages, validates the integrity of the message using the X-EBAY-SIGNATURE header and delegates to a custom configurable MessageProcessor for plugging in usecase specific processing logic.


Table of contents
==========
* [What Notifications are covered?](#notifications)
* [Features](#features)
* [Usage](#usage)
* [Logging](#logging)
* [License](#license)

# Notifications

This SDK is intended for the latest eBay notifications that use ECC signatures and JSON payloads. 
While this SDK is generic for any topics, it currently includes the schema definition for `MARKETPLACE_ACCOUNT_DELETION` notifications. 

This SDK now also incorporates support for endpoint validation.

# Features

This SDK is intended to bootstrap subscriptions to eBay Notifications and provides a ready DotNet example.

This SDK also incorporates support for endpoint validation.

This SDK incorporates

- A deployable example Dot Net Core application that is generic across topics and can process incoming https notifications
- Allows registration of custom Message Processors.
- Verify the integrity of the incoming messages
  - Use key id from the decoded signature header to fetch public key required by the verification algorithm. A cache is used to prevent re-fetches for same 'key'.
  - On verification success, delegate processing to the registered custom message processor and respond with a 204 (No Content) HTTP status code.
  - On verification failure, respond back with a 412 (Precondition Failed) HTTP status code
For more details on endpoint validation please refer to the [documentation](https://developer.ebay.com/marketplace-account-deletion).
# Usage

**Prerequisites**
```
.NET Core 3.1 or .NET 5.0
```
**Configure**

Update [appsettings.json](appsettings.json)  with: 
* Path to client credentials (required to fetch Public Key from /commerce/notification/v1/public_key/{public_key_id}).  
 Sample Client Credentials Configuration: [ebay-config.yaml](Samples/ebay-config.yaml).

* Environment (PRODUCTION or SANDBOX). Default: PRODUCTION

* Verification token associated with your endpoint. A random sample is included for your endpoint.
  It is recommended that this verification token be stored in a secure location and this needs to be the same as that provided to eBay. 

* Endpoint specific to this deployment. A random url is included as an example. 

```json
{
  "EbayEventNotification": {
    "ClientCredentialsFile": "RELATIVE_PATH_TO_EBAY_CONFIG_YAML",
    "Environment": "PRODUCTION",
    "Endpoint": "https://PUBLIC_URL_OF_SERVICE/event-notification/webhook",
    "VerificationToken": "YOUR_VERIFICATION_TOKEN"
  }
}
```

**Install and Run**
```
dotnet build
dotnet run
```

**Onboard any new topic in 3 simple steps! :**

* Create new topic model in [Models](Models/) 
* Add a custom MessageProcessor for new topic that implements `IMessageProcessor<TMessage>` see `ExampleAccountDeletionMessageProcessor.ProcessAsync()` for an example.
* Register the new MessageProcessor in `MessageProcessorFactory.GetProcessor()`
* Register processor service for new message type
```cs
 public void ConfigureServices(IServiceCollection services)
 { 
    services.AddEbayNotificationSdkServices(configuration);
    services.RegisterEbayNotificationProcessor<AccountDeletionData, ExampleAccountDeletionMessageProcessor>(); 
 }
```

Note on Production deploys
```
For production, please host with HTTPS enabled.
```

## Logging

Uses standard logging. 

## License

Copyright 2021 eBay Inc.  
Developer: Bhuvana Chandra Meka / Joe Adanah

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    https://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
