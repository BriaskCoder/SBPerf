@description('Location for all resources.')
param location string = resourceGroup().location
param subscriptionCountLow int = 1 // Number of subscriptions to create
param subscriptionCountMedium int = 5 // Number of subscriptions to create
param subscriptionCountHigh int = 50 // Number of subscriptions to create
var priorityStrings = [ 'High', 'Normal', 'Low']

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2023-01-01-preview' = {
  name: 'brwsStandardSB'
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {}
}

resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2023-01-01-preview' = {
  parent: serviceBusNamespace
  name: 'q-default'
  properties: {
    lockDuration: 'PT5M'
    maxSizeInMegabytes: 5120
    requiresDuplicateDetection: false
    requiresSession: false
    defaultMessageTimeToLive: 'PT10M'
    deadLetteringOnMessageExpiration: false
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    maxDeliveryCount: 10
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    enablePartitioning: false
    enableExpress: false
  }
}

resource serviceBusQueue1 'Microsoft.ServiceBus/namespaces/queues@2023-01-01-preview' = {
  parent: serviceBusNamespace
  name: 'q-duplicatedetection-on'
  properties: {
    lockDuration: 'PT5M'
    maxSizeInMegabytes: 5120
    requiresDuplicateDetection: true
    requiresSession: false
    defaultMessageTimeToLive: 'PT10M'
    deadLetteringOnMessageExpiration: false
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    maxDeliveryCount: 10
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    enablePartitioning: false
    enableExpress: false
  }
}

resource serviceBusQueue2 'Microsoft.ServiceBus/namespaces/queues@2023-01-01-preview' = {
  parent: serviceBusNamespace
  name: 'q-partitioning-on'
  properties: {
    lockDuration: 'PT5M'
    maxSizeInMegabytes: 5120
    requiresDuplicateDetection: false
    requiresSession: false
    defaultMessageTimeToLive: 'PT10M'
    deadLetteringOnMessageExpiration: false
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    maxDeliveryCount: 10
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    enablePartitioning: false
    enableExpress: false
  }
}

resource serviceBusQueue3 'Microsoft.ServiceBus/namespaces/queues@2023-01-01-preview' = {
  parent: serviceBusNamespace
  name: 'q-sessions-on'
  properties: {
    lockDuration: 'PT5M'
    maxSizeInMegabytes: 5120
    requiresDuplicateDetection: false
    requiresSession: true
    defaultMessageTimeToLive: 'PT10M'
    deadLetteringOnMessageExpiration: false
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    maxDeliveryCount: 10
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    enablePartitioning: false
    enableExpress: false
  }
}

resource serviceBusTopicDefault 'Microsoft.ServiceBus/namespaces/topics@2023-01-01-preview' = {
  parent: serviceBusNamespace
  name: 't-default'
  properties: {
    defaultMessageTimeToLive: 'PT10M'
    maxSizeInMegabytes: 5120
  }
}

resource subscriptionsDefault 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2023-01-01-preview' = {
  parent: serviceBusTopicDefault
  name: 'subscriptionDefault'
  properties: {
    maxDeliveryCount: 10
    lockDuration: 'PT1M'
  }
}

resource serviceBusTopicSubs1 'Microsoft.ServiceBus/namespaces/topics@2023-01-01-preview' = {
  parent: serviceBusNamespace
  name: 't-subs1'
  properties: {
    defaultMessageTimeToLive: 'PT10M'
    maxSizeInMegabytes: 5120
  }
}

resource subscriptionsLow 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2023-01-01-preview' = [for i in range(0, subscriptionCountLow): {
  parent: serviceBusTopicSubs1
  name: 'subscription${i+1}'
  properties: {
    maxDeliveryCount: 10
    lockDuration: 'PT1M'
  }
}]

resource serviceBusTopicSubs5 'Microsoft.ServiceBus/namespaces/topics@2023-01-01-preview' = {
  parent: serviceBusNamespace
  name: 't-subs5'
  properties: {
    defaultMessageTimeToLive: 'PT10M'
    maxSizeInMegabytes: 5120
  }
}

resource subscriptionsMedium 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2023-01-01-preview' = [for i in range(0, subscriptionCountMedium): {
  parent: serviceBusTopicSubs5
  name: 'subscription${i+1}'
  properties: {
    maxDeliveryCount: 10
    lockDuration: 'PT1M'
  }
}]

resource serviceBusTopicSubs50 'Microsoft.ServiceBus/namespaces/topics@2023-01-01-preview' = {
  parent: serviceBusNamespace
  name: 't-subs50'
  properties: {
    defaultMessageTimeToLive: 'PT10M'
    maxSizeInMegabytes: 5120
  }
}

resource subscriptionsHigh 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2023-01-01-preview' = [for i in range(0, subscriptionCountHigh): {
  parent: serviceBusTopicSubs50
  name: 'subscription${i+1}'
  properties: {
    maxDeliveryCount: 10
    lockDuration: 'PT1M'
  }
}]

resource serviceBusTopicFilterCorrelation 'Microsoft.ServiceBus/namespaces/topics@2023-01-01-preview' = {
  parent: serviceBusNamespace
  name: 't-filter-correlation'
  properties: {
    defaultMessageTimeToLive: 'PT10M'
    maxSizeInMegabytes: 5120
  }
}

resource subscriptionFilterCorrelation 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2023-01-01-preview' = {
  parent: serviceBusTopicFilterCorrelation
  name: 'subscriptionFilterCorrelation'
  properties: {
    maxDeliveryCount: 10
    lockDuration: 'PT1M'
  }
}

resource correlationFilterRules 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2023-01-01-preview' = [for code in priorityStrings: {
  name: 'CorrelationFilter-${code}'
  parent: subscriptionFilterCorrelation
  properties: {
    filterType: 'CorrelationFilter'
    correlationFilter: {
      properties: {
        Priority: code
      }
    }
  }
}]

resource subscriptionFilterCorrelationHigh 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  parent: serviceBusTopicFilterCorrelation
  name: 'subscriptionFilterCorrelationHigh'
  properties: {
    maxDeliveryCount: 10
    lockDuration: 'PT1M'
  }
}

resource correlationFilterRulesHigh 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2021-06-01-preview' = {
  name: 'CorrelationFilter-High'
  parent: subscriptionFilterCorrelationHigh
  properties: {
    filterType: 'CorrelationFilter'
    correlationFilter: {
      properties: {
        Priority: 'High'
      }
    }
  }
}


var serviceBusEndpoint = '${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey'
var serviceBusConnectionString = listKeys(serviceBusEndpoint, serviceBusNamespace.apiVersion).primaryConnectionString
output serviceBusConnectionString string = serviceBusConnectionString
