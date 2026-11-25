// ==========================================
// PARAMETERS
// ==========================================

param location string = resourceGroup().location

// Generate unique names for globally unique resources
var suffix = uniqueString(resourceGroup().id)

var stagingAppName = 'studentmanagement-staging-${suffix}'
var productionAppName = 'studentmanagement-production-${suffix}'

var storageAccountName = 'studentmgmt${suffix}'
var keyVaultName = 'sm-kv-${suffix}'

// ==========================================
// 1. APP SERVICE PLAN - WINDOWS F1
// ==========================================

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'studentmanagement-f1-plan'
  location: location
  kind: 'windows'
  sku: {
    name: 'F1'
    tier: 'Free'
    size: 'F1'
    capacity: 1
  }
  properties: {
    reserved: false
  }
}

// ==========================================
// 2. STAGING APP SERVICE
// ==========================================

resource stagingApp 'Microsoft.Web/sites@2023-12-01' = {
  name: stagingAppName
  location: location
  kind: 'app'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true

    siteConfig: {
      netFrameworkVersion: 'v10.0'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      metadata: [
        {
          name: 'CURRENT_STACK'
          value: 'dotnet'
        }
      ]
    }
  }
}

// ==========================================
// 4. STORAGE ACCOUNT
// ==========================================

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'

  sku: {
    name: 'Standard_LRS'
  }

  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
  }
}

// ==========================================
// 5. BLOB SERVICE
// ==========================================

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

// ==========================================
// 6. BLOB CONTAINER
// ==========================================

resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'student-files'

  properties: {
    publicAccess: 'None'
  }
}

// ==========================================
// 7. KEY VAULT
// ==========================================

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location

  properties: {
    tenantId: subscription().tenantId

    sku: {
      family: 'A'
      name: 'standard'
    }

    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    enablePurgeProtection: false

    publicNetworkAccess: 'Enabled'
  }
}

// ==========================================
// 8. OUTPUTS
// ==========================================

output stagingAppName string = stagingApp.name

output productionAppName string = productionApp.name

output stagingUrl string = 'https://${stagingApp.properties.defaultHostName}'


output storageAccountName string = storageAccount.name

output blobContainerName string = blobContainer.name

output keyVaultName string = keyVault.name

output stagingPrincipalId string = stagingApp.identity.principalId

