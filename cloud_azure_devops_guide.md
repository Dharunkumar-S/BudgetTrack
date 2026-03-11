# ☁️ Cloud & Azure DevOps Basics — Complete Reference Guide

---

## Table of Contents

1. [Introduction to the Cloud](#1-introduction-to-the-cloud)
2. [Introduction to Azure](#2-introduction-to-azure)
3. [Azure Basic Concepts](#3-azure-basic-concepts)
4. [Azure Compute](#4-azure-compute)
5. [Azure Networking](#5-azure-networking)
6. [Data in Azure](#6-data-in-azure)
7. [Messaging in Azure](#7-messaging-in-azure)
8. [Azure Active Directory](#8-azure-active-directory)
9. [Introduction to Azure DevOps](#9-introduction-to-azure-devops)
10. [Azure Repos](#10-azure-repos)
11. [Azure Boards](#11-azure-boards)
12. [Azure Pipelines Basics](#12-azure-pipelines-basics)
13. [Advanced Azure Pipelines](#13-advanced-azure-pipelines)
14. [Azure Release Pipelines](#14-azure-release-pipelines)
15. [Azure Test Plans](#15-azure-test-plans)

---

## 1. Introduction to the Cloud

### 1.1 What is the Cloud?

The **cloud** is the delivery of computing services — servers, storage, databases, networking, software, and analytics — over the internet ("the cloud") on a **pay-as-you-go** basis. Instead of owning physical hardware, you rent resources from a cloud provider.

> 💡 Think of it like electricity: you don't build a power plant to get power — you plug into the grid and pay for what you use.

---

### 1.2 Characteristics of the Cloud (NIST Model)

| Characteristic | Description |
|---|---|
| **On-demand self-service** | Provision resources instantly without human interaction |
| **Broad network access** | Accessible over the internet from any device |
| **Resource pooling** | Resources shared across multiple customers (multi-tenant) |
| **Rapid elasticity** | Scale up or down automatically based on demand |
| **Measured service** | Pay only for what you consume (metered billing) |

---

### 1.3 CapEx vs OpEx

| | CapEx (Capital Expenditure) | OpEx (Operational Expenditure) |
|---|---|---|
| **Definition** | Upfront investment in physical infrastructure | Ongoing pay-as-you-go spending |
| **Example** | Buy your own servers and data center | Rent Azure VMs monthly |
| **Cash Flow** | Large upfront cost | Predictable monthly expense |
| **Flexibility** | Low — hard to scale quickly | High — scale instantly |
| **Cloud Model** | Traditional on-premises | ✅ Cloud computing |

> 💡 Cloud shifts IT spending from **CapEx → OpEx**, freeing capital for other investments.

---

### 1.4 Cloud Service Models

#### IaaS — Infrastructure as a Service
You manage: OS, runtime, apps, data.
Provider manages: physical servers, storage, networking.

```
Examples: Azure Virtual Machines, AWS EC2, Google Compute Engine
Use when: You need full OS control, custom environments
```

#### PaaS — Platform as a Service
You manage: applications and data only.
Provider manages: OS, runtime, middleware, infrastructure.

```
Examples: Azure App Service, Azure SQL Database, Google App Engine
Use when: You want to focus on code, not infrastructure
```

#### SaaS — Software as a Service
You manage: nothing (just use the software).
Provider manages: everything.

```
Examples: Microsoft 365, Salesforce, Gmail
Use when: You need ready-to-use applications
```

**Responsibility Model:**
```
         You Manage        Provider Manages
IaaS:  [App][Data][OS]   [VM][Storage][Network][Hardware]
PaaS:  [App][Data]       [OS][VM][Storage][Network][Hardware]
SaaS:  [Settings]        [App][Data][OS][VM][Storage][Hardware]
```

---

### 1.5 Types of Clouds

| Type | Description | Example |
|---|---|---|
| **Public Cloud** | Shared infrastructure over the internet | Azure, AWS, GCP |
| **Private Cloud** | Dedicated infrastructure for one organization | On-premises Azure Stack |
| **Hybrid Cloud** | Mix of public + private clouds | Azure + on-premises connected via VPN |
| **Multi-Cloud** | Using multiple public cloud providers | Azure + AWS together |

---

### 1.6 Main Cloud Providers

| Provider | Platform | Market Position |
|---|---|---|
| **Microsoft Azure** | Azure | #2 — enterprise-dominant |
| **Amazon Web Services** | AWS | #1 — largest market share |
| **Google Cloud** | GCP | #3 — strong in data/ML |
| **IBM Cloud** | IBM Cloud | Niche, enterprise focus |
| **Oracle Cloud** | OCI | Strong in database workloads |

---

## 2. Introduction to Azure

### 2.1 Introduction

**Microsoft Azure** is Microsoft's cloud computing platform offering 200+ services across compute, storage, networking, AI, DevOps, and more. It is the second-largest cloud provider globally, with particularly strong enterprise adoption.

---

### 2.2 Regions and Zones

**Region** — A geographic area containing one or more data centers (e.g., East US, West Europe, Southeast Asia). Azure has **60+ regions** worldwide.

**Availability Zone** — Physically separate data centers within a region, each with independent power, cooling, and networking. Used for high availability.

```
Region: East US
├── Availability Zone 1  (Data Center A)
├── Availability Zone 2  (Data Center B)
└── Availability Zone 3  (Data Center C)
```

**Region Pairs** — Each Azure region is paired with another region for disaster recovery (e.g., East US ↔ West US).

---

### 2.3 Azure Services (Key Categories)

| Category | Services |
|---|---|
| **Compute** | VMs, App Service, AKS, Azure Functions |
| **Storage** | Blob, Queue, Table, File |
| **Databases** | Azure SQL, Cosmos DB, MySQL, PostgreSQL |
| **Networking** | VNet, Load Balancer, Application Gateway, DNS |
| **Identity** | Azure AD, MFA, RBAC |
| **DevOps** | Azure DevOps, GitHub Actions |
| **AI/ML** | Azure AI, Cognitive Services, ML Studio |
| **Messaging** | Service Bus, Event Grid, Event Hubs |

---

### 2.4 Creating an Account

1. Go to [portal.azure.com](https://portal.azure.com)
2. Click **Start free** — get $200 credit for 30 days
3. Provide Microsoft account + credit card (for verification)
4. Access the Azure Portal immediately after signup

---

### 2.5 The Azure Portal

The Azure Portal is the **web-based UI** for managing all Azure resources at `portal.azure.com`.

Key areas:
- **Dashboard** — customizable overview of your resources
- **All Resources** — list of everything you've created
- **Resource Groups** — logical containers for resources
- **Marketplace** — browse and deploy pre-built services
- **Cloud Shell** — browser-based CLI (bash or PowerShell)

---

### 2.6 Account and Subscription

```
Azure Account (Microsoft Identity)
└── Tenant (Azure AD Directory)
    ├── Subscription A  (Production)
    │   ├── Resource Group 1
    │   └── Resource Group 2
    └── Subscription B  (Development)
        └── Resource Group 3
```

- **Account** — your Microsoft identity
- **Tenant** — your Azure AD organization directory
- **Subscription** — billing unit; resources and costs are tracked per subscription
- **Resource Group** — logical container for related resources

---

### 2.7 Creating, Finding, and Removing Resources

```bash
# Azure CLI — Create a resource group
az group create --name MyResourceGroup --location eastus

# Create a resource (e.g., storage account)
az storage account create \
  --name mystorageacct \
  --resource-group MyResourceGroup \
  --location eastus \
  --sku Standard_LRS

# Find/list resources
az resource list --resource-group MyResourceGroup

# Remove a resource
az resource delete \
  --resource-group MyResourceGroup \
  --name mystorageacct \
  --resource-type "Microsoft.Storage/storageAccounts"

# Remove entire resource group (deletes ALL resources inside)
az group delete --name MyResourceGroup --yes
```

---

### 2.8 Azure CLI & PowerShell

**Azure CLI (cross-platform):**
```bash
# Install
# macOS: brew install azure-cli
# Windows: winget install Microsoft.AzureCLI

az login                              # Authenticate
az account list                       # List subscriptions
az account set --subscription "MyID"  # Switch subscription
az vm list --output table             # List VMs
```

**Azure PowerShell:**
```powershell
# Install
Install-Module -Name Az -AllowClobber

Connect-AzAccount                     # Authenticate
Get-AzSubscription                    # List subscriptions
Set-AzContext -SubscriptionId "MyID"  # Switch subscription
Get-AzVM                              # List VMs
```

---

## 3. Azure Basic Concepts

### 3.1 Regions

Choose a region based on:
- **Proximity** — closer to users = lower latency
- **Compliance** — data residency requirements (GDPR etc.)
- **Availability** — not all services are in all regions
- **Cost** — pricing varies by region

---

### 3.2 Resource Groups

A **Resource Group** is a logical container that holds related Azure resources for an application or project.

```bash
# Create resource group
az group create --name "MyApp-RG" --location "westeurope"

# List all resource groups
az group list --output table

# Move resource to another group
az resource move --destination-group "NewRG" --ids <resource-id>

# Delete resource group (all resources inside deleted too)
az group delete --name "MyApp-RG"
```

**Best Practices:**
- Group resources by **application lifecycle** (dev/test/prod)
- Use consistent **naming conventions**: `{app}-{env}-{region}-rg`
- Tag resource groups for cost tracking: `Environment=Production`

---

### 3.3 Storage Accounts

Azure Storage Accounts provide the foundation for Blob, Queue, Table, and File storage.

```bash
# Create storage account
az storage account create \
  --name "myapp$RANDOM" \
  --resource-group "MyApp-RG" \
  --location "eastus" \
  --sku Standard_LRS \        # Locally redundant storage
  --kind StorageV2
```

**Redundancy Options:**

| SKU | Description | Copies |
|---|---|---|
| `LRS` | Locally Redundant Storage | 3 copies in same datacenter |
| `ZRS` | Zone Redundant Storage | 3 copies across zones |
| `GRS` | Geo-Redundant Storage | 6 copies across 2 regions |
| `GZRS` | Geo-Zone Redundant Storage | 6 copies across zones + regions |

---

### 3.4 SLA (Service Level Agreement)

An **SLA** is Microsoft's commitment to uptime and connectivity for Azure services.

| SLA | Max Downtime/Month | Max Downtime/Year |
|---|---|---|
| 99% | 7.2 hours | 3.65 days |
| 99.9% | 43.2 minutes | 8.76 hours |
| 99.95% | 21.6 minutes | 4.38 hours |
| 99.99% | 4.32 minutes | 52.56 minutes |

**Composite SLA** — when your app uses multiple services, multiply their SLAs:
```
App Service (99.95%) × Azure SQL (99.99%) = 99.94% composite SLA
```

---

### 3.5 Cost Management

```bash
# View cost analysis (Azure CLI)
az consumption usage list --top 10

# Set a budget alert
az consumption budget create \
  --budget-name "MonthlyBudget" \
  --amount 100 \
  --time-grain Monthly \
  --category Cost
```

**Cost-saving strategies:**
- Use **Reserved Instances** (1 or 3 year) — up to 72% savings vs pay-as-you-go
- **Auto-shutdown** dev/test VMs at night
- Use **Azure Cost Management + Billing** dashboard to identify waste
- Use **Spot VMs** for interruptible workloads (up to 90% cheaper)

---

## 4. Azure Compute

### 4.1 Virtual Machines

Azure VMs are **IaaS** — you control the OS and everything above it.

```bash
# Create a Linux VM
az vm create \
  --resource-group "MyRG" \
  --name "MyVM" \
  --image Ubuntu2204 \
  --size Standard_B1s \
  --admin-username azureuser \
  --generate-ssh-keys

# Connect via SSH
ssh azureuser@<public-ip>

# Start / Stop / Restart
az vm start   --resource-group MyRG --name MyVM
az vm stop    --resource-group MyRG --name MyVM
az vm restart --resource-group MyRG --name MyVM
```

---

### 4.2 ARM Templates

**Azure Resource Manager (ARM) Templates** are JSON files that define Azure infrastructure as code (IaC).

```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "vmName": { "type": "string", "defaultValue": "MyVM" }
  },
  "resources": [
    {
      "type": "Microsoft.Compute/virtualMachines",
      "apiVersion": "2022-03-01",
      "name": "[parameters('vmName')]",
      "location": "[resourceGroup().location]",
      "properties": {
        "hardwareProfile": { "vmSize": "Standard_B1s" }
      }
    }
  ]
}
```

```bash
# Deploy an ARM template
az deployment group create \
  --resource-group MyRG \
  --template-file azuredeploy.json \
  --parameters vmName=MyNewVM
```

---

### 4.3 Virtual Machine Scale Sets

**VMSS** automatically increases or decreases the number of identical VM instances based on demand or a schedule.

```bash
# Create a scale set
az vmss create \
  --resource-group MyRG \
  --name MyScaleSet \
  --image Ubuntu2204 \
  --instance-count 2 \
  --vm-sku Standard_B1s \
  --upgrade-policy-mode automatic

# Configure autoscale
az monitor autoscale create \
  --resource-group MyRG \
  --resource MyScaleSet \
  --resource-type Microsoft.Compute/virtualMachineScaleSets \
  --name autoscale-rule \
  --min-count 2 --max-count 10 --count 2
```

---

### 4.4 App Services

**Azure App Service** is a **PaaS** offering to host web apps, REST APIs, and mobile backends without managing VMs.

```bash
# Create App Service Plan
az appservice plan create \
  --name MyPlan \
  --resource-group MyRG \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --resource-group MyRG \
  --plan MyPlan \
  --name my-unique-app-name \
  --runtime "DOTNET|8.0"

# Deploy from local folder
az webapp deploy \
  --resource-group MyRG \
  --name my-unique-app-name \
  --src-path ./publish.zip
```

**App Service Tiers:**

| Tier | Use Case | Features |
|---|---|---|
| **Free (F1)** | Dev/test | 60 min/day CPU, no custom domain |
| **Basic (B1-B3)** | Low traffic | Custom domain, manual scale |
| **Standard (S1-S3)** | Production | Auto-scale, deployment slots (5) |
| **Premium (P1-P3)** | High performance | More scale, VNET integration |
| **Isolated (I1-I3)** | Enterprise | Dedicated environment (ASE) |

---

### 4.5 Deployment Slots

Deployment slots let you run **multiple versions** of your app (e.g., staging and production) and swap them with zero downtime.

```bash
# Create a staging slot
az webapp deployment slot create \
  --name my-app \
  --resource-group MyRG \
  --slot staging

# Deploy to staging
az webapp deploy --name my-app --slot staging --src-path app.zip

# Swap staging → production
az webapp deployment slot swap \
  --name my-app \
  --resource-group MyRG \
  --slot staging \
  --target-slot production
```

---

### 4.6 Azure Kubernetes Service (AKS)

**AKS** is a managed Kubernetes service — Azure manages the control plane, you manage the worker nodes.

```bash
# Create AKS cluster
az aks create \
  --resource-group MyRG \
  --name MyAKSCluster \
  --node-count 2 \
  --node-vm-size Standard_B2s \
  --generate-ssh-keys

# Get credentials (configure kubectl)
az aks get-credentials --resource-group MyRG --name MyAKSCluster

# Deploy an app
kubectl apply -f deployment.yaml

# Scale deployment
kubectl scale deployment my-app --replicas=5
```

---

### 4.7 Azure Functions

**Azure Functions** is a **serverless** compute service — run code in response to events without managing servers. You pay only when your code runs.

```bash
# Create Function App
az functionapp create \
  --resource-group MyRG \
  --consumption-plan-location eastus \
  --runtime dotnet \
  --functions-version 4 \
  --name my-function-app \
  --storage-account mystorageacct
```

```csharp
// HTTP-triggered Azure Function
public static class HttpTriggerFunction
{
    [FunctionName("HelloWorld")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("Function triggered");
        return new OkObjectResult("Hello from Azure Functions!");
    }
}
```

```bash
# Run function locally
func start
```

---

### 4.8 How to Choose a Compute Type?

```
Do you need full OS control?
├── YES → Virtual Machine (IaaS)
│
└── NO → Is it event-driven / short-lived?
         ├── YES → Azure Functions (Serverless)
         │
         └── NO → Is it containerized?
                  ├── YES → AKS (Kubernetes) or Container Apps
                  │
                  └── NO → App Service (PaaS Web App)
```

---

## 5. Azure Networking

### 5.1 Virtual Networks (VNets)

A **VNet** is an isolated private network in Azure. Resources inside a VNet can communicate with each other securely.

```bash
# Create a VNet
az network vnet create \
  --resource-group MyRG \
  --name MyVNet \
  --address-prefix 10.0.0.0/16 \
  --location eastus
```

---

### 5.2 Subnets

Subnets divide a VNet into smaller network segments. Each subnet has its own IP range and can have different security rules.

```bash
# Create subnets
az network vnet subnet create \
  --resource-group MyRG \
  --vnet-name MyVNet \
  --name WebSubnet \
  --address-prefixes 10.0.1.0/24

az network vnet subnet create \
  --resource-group MyRG \
  --vnet-name MyVNet \
  --name DBSubnet \
  --address-prefixes 10.0.2.0/24
```

```
VNet: 10.0.0.0/16
├── WebSubnet:  10.0.1.0/24  (Web tier)
├── AppSubnet:  10.0.2.0/24  (Application tier)
└── DBSubnet:   10.0.3.0/24  (Database tier — most restricted)
```

---

### 5.3 Secure VM Access

```bash
# Network Security Group (NSG) — firewall rules for a subnet or NIC
az network nsg create --resource-group MyRG --name MyNSG

# Allow SSH only from specific IP
az network nsg rule create \
  --resource-group MyRG \
  --nsg-name MyNSG \
  --name AllowSSH \
  --priority 100 \
  --source-address-prefixes "203.0.113.0/24" \
  --destination-port-ranges 22 \
  --protocol Tcp \
  --access Allow

# Azure Bastion — secure browser-based SSH/RDP (no public IP on VM needed)
az network bastion create \
  --resource-group MyRG \
  --name MyBastion \
  --vnet-name MyVNet \
  --public-ip-address BastionPublicIP
```

---

### 5.4 Service Endpoint vs Private Link

| Feature | Service Endpoint | Private Link |
|---|---|---|
| **Traffic path** | Microsoft backbone (still public IP) | Private IP in your VNet |
| **Exposure** | Service accessible via internet | Service completely private |
| **Cost** | Free | Has a per-hour cost |
| **Use case** | Basic VNet-to-service security | Enterprise / compliance requirements |

---

### 5.5 Load Balancer vs Application Gateway

| | Azure Load Balancer | Application Gateway |
|---|---|---|
| **Layer** | Layer 4 (TCP/UDP) | Layer 7 (HTTP/HTTPS) |
| **Routing** | IP/port based | URL path, hostname based |
| **SSL Termination** | ❌ | ✅ |
| **WAF** | ❌ | ✅ Web Application Firewall |
| **Use case** | Non-HTTP workloads, VM load balancing | Web apps, API routing |

```bash
# Create Application Gateway
az network application-gateway create \
  --resource-group MyRG \
  --name MyAppGateway \
  --location eastus \
  --sku Standard_v2 \
  --capacity 2 \
  --vnet-name MyVNet \
  --subnet AppGWSubnet \
  --public-ip-address AppGWPublicIP
```

---

## 6. Data in Azure

### 6.1 Azure SQL

Fully managed **SQL Server** in the cloud — no OS or SQL Server patching required.

```bash
# Create SQL Server
az sql server create \
  --name my-sql-server \
  --resource-group MyRG \
  --location eastus \
  --admin-user sqladmin \
  --admin-password "P@ssw0rd123!"

# Create database
az sql db create \
  --resource-group MyRG \
  --server my-sql-server \
  --name MyDatabase \
  --service-objective S1

# Allow your IP
az sql server firewall-rule create \
  --resource-group MyRG \
  --server my-sql-server \
  --name AllowMyIP \
  --start-ip-address 203.0.113.0 \
  --end-ip-address 203.0.113.0
```

---

### 6.2 Cosmos DB

**Azure Cosmos DB** is a globally distributed, multi-model **NoSQL** database with single-digit millisecond latency.

```bash
# Create Cosmos DB account
az cosmosdb create \
  --resource-group MyRG \
  --name my-cosmos-account \
  --kind GlobalDocumentDB \
  --locations regionName=eastus failoverPriority=0

# Create database and container
az cosmosdb sql database create \
  --resource-group MyRG \
  --account-name my-cosmos-account \
  --name MyDatabase

az cosmosdb sql container create \
  --resource-group MyRG \
  --account-name my-cosmos-account \
  --database-name MyDatabase \
  --name Products \
  --partition-key-path "/category"
```

---

### 6.3 SQL vs NoSQL

| | SQL (Azure SQL) | NoSQL (Cosmos DB) |
|---|---|---|
| **Schema** | Fixed schema | Flexible / schemaless |
| **Scaling** | Vertical (scale up) | Horizontal (scale out) |
| **Consistency** | ACID transactions | Tunable consistency levels |
| **Query** | SQL | SQL API, Mongo, Gremlin, Table |
| **Best for** | Relational data, transactions | Large-scale, global, flexible data |
| **Latency** | ms range | Single-digit ms guaranteed |

---

### 6.4 Azure Storage Services

| Service | Use Case | Example |
|---|---|---|
| **Blob Storage** | Unstructured data (files, images, videos) | Profile pictures, backups |
| **Table Storage** | Key-value NoSQL store | Lightweight structured data |
| **Queue Storage** | Message queuing | Decouple app components |
| **File Storage** | SMB file shares | Shared drives for VMs |

```bash
# Upload a file to Blob Storage
az storage blob upload \
  --account-name mystorageacct \
  --container-name mycontainer \
  --name myfile.txt \
  --file ./myfile.txt

# Generate SAS token for temporary access
az storage blob generate-sas \
  --account-name mystorageacct \
  --container-name mycontainer \
  --name myfile.txt \
  --permissions r \
  --expiry 2025-01-01
```

---

### 6.5 CDN and Automation

**Azure CDN** — caches content at edge locations globally, reducing latency for static assets.

```bash
# Create CDN profile and endpoint
az cdn profile create --resource-group MyRG --name MyCDN --sku Standard_Microsoft
az cdn endpoint create \
  --resource-group MyRG \
  --profile-name MyCDN \
  --name my-cdn-endpoint \
  --origin mystorageacct.blob.core.windows.net
```

---

## 7. Messaging in Azure

### 7.1 Storage Queue

Simple, durable **message queuing** built into Azure Storage. Best for decoupling components.

```bash
# Create a queue
az storage queue create --name myqueue --account-name mystorageacct

# Send a message
az storage message put \
  --queue-name myqueue \
  --content "Hello from queue" \
  --account-name mystorageacct

# Read and process messages (C#)
```
```csharp
var client = new QueueClient(connectionString, "myqueue");
var messages = await client.ReceiveMessagesAsync(maxMessages: 10);
foreach (var msg in messages.Value)
{
    Console.WriteLine(msg.MessageText);
    await client.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);
}
```

---

### 7.2 Event Grid

**Azure Event Grid** is a fully managed **event routing** service. It reacts to events (e.g., blob uploaded, VM stopped) and routes them to subscribers.

```
Event Source (Blob Storage, VM, Custom)
        ↓
    Event Grid Topic
        ↓
Event Subscription (Filter)
        ↓
    Handler (Function, Logic App, Webhook)
```

```bash
# Create a custom Event Grid topic
az eventgrid topic create \
  --resource-group MyRG \
  --name my-topic \
  --location eastus

# Subscribe a webhook endpoint
az eventgrid event-subscription create \
  --name my-subscription \
  --source-resource-id <topic-id> \
  --endpoint https://my-app.com/webhook
```

---

### 7.3 Service Bus

**Azure Service Bus** is an enterprise message broker for **reliable, ordered, and transactional** messaging between services.

```
Producer → [Service Bus Queue/Topic] → Consumer
```

- **Queue** — point-to-point (one producer, one consumer)
- **Topic/Subscription** — pub/sub (one producer, many consumers)

```csharp
// Send a message
await using var client = new ServiceBusClient(connectionString);
var sender = client.CreateSender("my-queue");
await sender.SendMessageAsync(new ServiceBusMessage("Order placed!"));

// Receive a message
var receiver = client.CreateReceiver("my-queue");
var message = await receiver.ReceiveMessageAsync();
Console.WriteLine(message.Body.ToString());
await receiver.CompleteMessageAsync(message);
```

---

### 7.4 Event Hubs

**Azure Event Hubs** is a big data **streaming platform** capable of ingesting millions of events per second. Best for telemetry, logs, and IoT data.

| Feature | Service Bus | Event Hubs |
|---|---|---|
| **Pattern** | Message broker | Event streaming |
| **Retention** | Until processed | Time-based (1–90 days) |
| **Throughput** | Thousands/sec | Millions/sec |
| **Use case** | Order processing, workflows | IoT telemetry, log ingestion |
| **Consumers** | Each message consumed once | Multiple consumer groups |

```csharp
// Produce events
var producer = new EventHubProducerClient(connectionString, "my-hub");
var eventBatch = await producer.CreateBatchAsync();
eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes("sensor-reading-1")));
await producer.SendAsync(eventBatch);
```

---

## 8. Azure Active Directory

### 8.1 Introduction & Tenants

**Azure Active Directory (Azure AD)** — now called **Microsoft Entra ID** — is Microsoft's cloud-based identity and access management service.

- **Tenant** — a dedicated, trusted instance of Azure AD for an organization. Created when you sign up for Azure.
- Each organization has exactly one tenant (identified by a unique `tenant-id` GUID and domain like `mycompany.onmicrosoft.com`)

---

### 8.2 Users and Groups

```bash
# Create a user
az ad user create \
  --display-name "Jane Doe" \
  --user-principal-name jane@mycompany.onmicrosoft.com \
  --password "TempP@ss123!" \
  --force-change-password-next-sign-in true

# Create a group
az ad group create \
  --display-name "Developers" \
  --mail-nickname "developers"

# Add user to group
az ad group member add \
  --group "Developers" \
  --member-id <user-object-id>
```

---

### 8.3 MFA and Security Defaults

**MFA (Multi-Factor Authentication)** requires a second verification step in addition to password:
- Authenticator app (recommended)
- SMS code
- Phone call

**Security Defaults** — a baseline set of security policies Microsoft enables for free tenants:
- Requires MFA for all admins
- Blocks legacy authentication protocols
- Requires MFA when risk is detected

---

### 8.4 RBAC (Role-Based Access Control)

RBAC controls **who can do what** on which Azure resources.

```
Principal (Who)  →  Role (What)  →  Scope (Where)
User/Group/SP    →  Reader        →  Subscription/RG/Resource
```

**Built-in Azure Roles:**

| Role | Permissions |
|---|---|
| **Owner** | Full access + assign roles |
| **Contributor** | Create/manage resources, no role assignment |
| **Reader** | View-only access |
| **User Access Administrator** | Manage user access only |

```bash
# Assign a role
az role assignment create \
  --assignee jane@mycompany.onmicrosoft.com \
  --role "Contributor" \
  --scope /subscriptions/<sub-id>/resourceGroups/MyRG

# List role assignments
az role assignment list --resource-group MyRG --output table
```

---

### 8.5 OAuth & JWT

**OAuth 2.0** — Authorization framework used by Azure AD for granting apps limited access to user resources.

**JWT (JSON Web Token)** — The token format used to carry identity and authorization claims.

```
User → Login → Azure AD → Issues JWT Access Token
App  → Calls API with: Authorization: Bearer <JWT>
API  → Validates JWT signature + claims → Allows/Denies
```

**JWT Structure:**
```
Header.Payload.Signature

Payload example:
{
  "sub": "user-object-id",
  "name": "Jane Doe",
  "roles": ["Admin"],
  "exp": 1700000000
}
```

---

## 9. Introduction to Azure DevOps

### 9.1 Overview

**Azure DevOps** is a suite of developer tools for planning, building, testing, and deploying software. It supports any language, platform, and cloud.

---

### 9.2 Key Components

| Service | Purpose |
|---|---|
| **Azure Boards** | Agile project management (work items, backlogs, sprints) |
| **Azure Repos** | Git repositories for source control |
| **Azure Pipelines** | CI/CD — build and deploy automation |
| **Azure Test Plans** | Manual and automated testing management |
| **Azure Artifacts** | Package management (NuGet, npm, Maven, PyPI) |

---

### 9.3 Creating an Account and Organization

1. Go to [dev.azure.com](https://dev.azure.com)
2. Sign in with a Microsoft account
3. Create an **Organization** (e.g., `mycompany`)
4. Create a **Project** inside the organization
5. Choose visibility: **Private** or **Public**

```
dev.azure.com/mycompany/         ← Organization
└── MyWebApp/                    ← Project
    ├── Boards
    ├── Repos
    ├── Pipelines
    ├── Test Plans
    └── Artifacts
```

---

## 10. Azure Repos

### 10.1 Git Basics

```bash
git init                          # Initialize local repo
git clone <url>                   # Clone remote repo
git add .                         # Stage all changes
git commit -m "My message"        # Commit staged changes
git push origin main              # Push to remote
git pull origin main              # Pull latest changes
git status                        # Check working tree status
git log --oneline                 # View commit history
```

---

### 10.2 Creating and Managing Repositories

```bash
# Clone your Azure Repo
git clone https://mycompany@dev.azure.com/mycompany/MyProject/_git/MyRepo

# Set remote for existing project
git remote add origin https://dev.azure.com/mycompany/MyProject/_git/MyRepo
git push -u origin main
```

---

### 10.3 Branching and Merging Strategies

```bash
# Create and switch to a feature branch
git checkout -b feature/user-login

# After development, merge back
git checkout main
git merge feature/user-login

# Delete merged branch
git branch -d feature/user-login

# Rebase instead of merge (cleaner history)
git checkout feature/user-login
git rebase main
```

---

### 10.4 Git Workflows

**GitFlow (structured releases):**
```
main          ← Production-ready code only
develop       ← Integration branch
feature/*     ← New features branched from develop
release/*     ← Pre-release stabilization
hotfix/*      ← Critical production fixes
```

**Trunk-Based Development (modern CI/CD):**
```
main          ← Single trunk, always deployable
feature/*     ← Short-lived feature branches (days, not weeks)
              ← Merge via PR with CI checks
```

---

### 10.5 Pull Requests

A **Pull Request (PR)** is a request to merge code from one branch to another, with code review and CI checks.

**PR best practices:**
- Keep PRs small and focused (< 400 lines)
- Link to a work item (`#123`)
- Require at least 1 reviewer approval
- Set up **branch policies**: require CI to pass before merge
- Use **squash merge** for cleaner history

---

## 11. Azure Boards

### 11.1 Work Item Types

| Work Item | Description | Used In |
|---|---|---|
| **Epic** | Large initiative spanning multiple sprints | Both |
| **Feature** | Functionality delivered within a few sprints | Both |
| **User Story** | End-user functionality ("As a user, I want...") | Scrum/Agile |
| **Task** | Smallest unit of work (hours) | Both |
| **Bug** | Defect to be fixed | Both |
| **Test Case** | Test scenario to verify behavior | Both |

---

### 11.2 Agile and Scrum

**Scrum Ceremonies:**
```
Sprint Planning  → What will we build this sprint?
Daily Standup   → What did I do? What will I do? Any blockers?
Sprint Review   → Demo completed work to stakeholders
Retrospective   → What went well? What to improve?
```

**Sprint** = fixed time-box (usually 1–4 weeks) to complete a set of work items.

---

### 11.3 Kanban Boards and Sprints

**Kanban Board** — visualizes work flow across columns:
```
To Do → Active → In Review → Testing → Done
```

**Sprint Board** — shows only work items for the current sprint, in a task board view.

**Configuring in Azure Boards:**
- Drag work items between columns to update state
- Set **WIP limits** per column (Kanban)
- Use **swimlanes** to separate priority (e.g., "Expedite" vs "Normal")

---

### 11.4 Backlogs and Planning

- **Product Backlog** — all work items prioritized by business value
- **Sprint Backlog** — items committed for the current sprint
- **Velocity** — average story points completed per sprint (used for forecasting)

```
Backlog Refinement:
1. Review and estimate items (story points)
2. Break Epics → Features → User Stories → Tasks
3. Prioritize by business value
4. Move top items into next sprint during Sprint Planning
```

---

## 12. Azure Pipelines Basics

### 12.1 Introduction to CI/CD

**CI (Continuous Integration)** — automatically build and test code on every commit.

**CD (Continuous Deployment/Delivery)** — automatically deploy code to environments after CI passes.

```
Developer pushes code
        ↓
CI Pipeline: Build → Unit Tests → Code Analysis
        ↓
CD Pipeline: Deploy to Dev → Test → Staging → Production
```

---

### 12.2 Building Blocks of a Pipeline

```yaml
# azure-pipelines.yml

trigger:           # When to run
  - main

pool:              # Where to run (agent)
  vmImage: 'ubuntu-latest'

variables:         # Reusable values
  buildConfig: 'Release'

stages:            # Major phases (Build, Test, Deploy)
  - stage: Build
    jobs:          # Units of work within a stage
      - job: BuildJob
        steps:     # Individual commands
          - task: DotNetCoreCLI@2
            inputs:
              command: 'build'
              arguments: '--configuration $(buildConfig)'
```

---

### 12.3 YAML vs Visual Designer

| | YAML Pipeline | Visual Designer (Classic) |
|---|---|---|
| **Format** | Code (YAML file in repo) | GUI-based |
| **Version Control** | ✅ Tracked in Git | ❌ Stored in Azure DevOps |
| **Reusability** | ✅ Templates | Limited |
| **Recommended** | ✅ Yes (modern) | Legacy |

---

### 12.4 Triggers and Variables

```yaml
# Trigger on push to main or release branches
trigger:
  branches:
    include:
      - main
      - release/*
  paths:
    exclude:
      - docs/*           # Don't run for doc-only changes

# Pull request trigger
pr:
  branches:
    include:
      - main

# Scheduled trigger
schedules:
  - cron: "0 2 * * *"   # Run at 2 AM daily
    displayName: Nightly Build
    branches:
      include: [ main ]

# Variables
variables:
  - name: environment
    value: production
  - group: MyVariableGroup  # From Azure DevOps Library (secrets)
```

---

### 12.5 Agent Pools and Jobs

**Agent** — the machine that runs your pipeline steps.

| Agent Type | Description |
|---|---|
| **Microsoft-hosted** | Azure manages the VM — clean environment each run |
| **Self-hosted** | You provide and manage the machine (more control) |

```yaml
# Microsoft-hosted agent
pool:
  vmImage: 'ubuntu-latest'    # or 'windows-latest', 'macos-latest'

# Self-hosted agent pool
pool:
  name: 'MyPrivateAgents'
  demands:
    - docker

# Parallel jobs
jobs:
  - job: Linux
    pool: { vmImage: 'ubuntu-latest' }
    steps: [...]

  - job: Windows
    pool: { vmImage: 'windows-latest' }
    steps: [...]
```

---

## 13. Advanced Azure Pipelines

### 13.1 Multi-Stage Pipelines

```yaml
stages:
  - stage: Build
    displayName: 'Build & Test'
    jobs:
      - job: Build
        steps:
          - task: DotNetCoreCLI@2
            inputs: { command: 'build' }
          - task: DotNetCoreCLI@2
            inputs: { command: 'test' }
          - task: PublishBuildArtifacts@1
            inputs: { pathToPublish: '$(Build.ArtifactStagingDirectory)' }

  - stage: DeployDev
    displayName: 'Deploy to Dev'
    dependsOn: Build
    condition: succeeded()
    jobs:
      - deployment: DeployWebApp
        environment: 'Development'
        strategy:
          runOnce:
            deploy:
              steps:
                - task: AzureWebApp@1
                  inputs:
                    azureSubscription: 'MyServiceConnection'
                    appName: 'my-app-dev'

  - stage: DeployProd
    displayName: 'Deploy to Production'
    dependsOn: DeployDev
    jobs:
      - deployment: DeployProd
        environment: 'Production'   # Requires manual approval
        strategy:
          runOnce:
            deploy:
              steps:
                - task: AzureWebApp@1
                  inputs:
                    azureSubscription: 'MyServiceConnection'
                    appName: 'my-app-prod'
```

---

### 13.2 Pipeline Templates

Reuse pipeline logic across multiple pipelines:

```yaml
# templates/build-steps.yml
parameters:
  - name: buildConfig
    type: string
    default: 'Release'

steps:
  - task: DotNetCoreCLI@2
    inputs:
      command: 'restore'
  - task: DotNetCoreCLI@2
    inputs:
      command: 'build'
      arguments: '--configuration ${{ parameters.buildConfig }}'
  - task: DotNetCoreCLI@2
    inputs:
      command: 'test'
```

```yaml
# azure-pipelines.yml — use the template
stages:
  - stage: Build
    jobs:
      - job: BuildJob
        steps:
          - template: templates/build-steps.yml
            parameters:
              buildConfig: 'Release'
```

---

### 13.3 Environments and Approvals

**Environments** in Azure Pipelines represent deployment targets (Dev, Staging, Production) with:
- **Manual approval gates** — a person must approve before deploying
- **Deployment history** — see what was deployed when
- **Checks** — run tests, policies before proceeding

```yaml
# Environment with approval gate (configured in Azure DevOps UI)
- deployment: DeployProd
  environment: 'Production'    # Set approval in Environments settings
```

---

### 13.4 Deployment Strategies

```yaml
# Rolling — update instances incrementally
strategy:
  rolling:
    maxParallel: 25%    # Update 25% of instances at a time
    deploy:
      steps: [...]

# Canary — send small % of traffic to new version first
strategy:
  canary:
    increments: [10, 50]
    deploy:
      steps: [...]
    postRouteTraffic:
      steps: [...]

# Blue-Green — swap deployment slots (zero downtime)
strategy:
  runOnce:
    deploy:
      steps:
        - task: AzureAppServiceManage@0
          inputs:
            action: 'Swap Slots'
            sourceSlot: 'staging'
```

---

## 14. Azure Release Pipelines

### 14.1 Introduction to Release Pipelines

**Release Pipelines** (Classic) manage the deployment of build artifacts to environments. They complement CI pipelines by handling CD workflows visually.

```
Build Artifact
      ↓
Release Pipeline
├── Stage 1: Dev        → Auto-deploy on new artifact
├── Stage 2: QA         → Auto-deploy after Dev succeeds
└── Stage 3: Production → Manual approval gate required
```

---

### 14.2 Deployment Environments

Each **stage** in a Release Pipeline maps to an environment:
- Set **pre-deployment conditions** — manual approval, automated gates
- Set **post-deployment conditions** — verify deployment health
- Configure **environment-specific variables** per stage

---

### 14.3 Deployment Gates and Approvals

**Gates** — automated checks that must pass before progressing:
- **Azure Monitor** — check for zero alerts/exceptions
- **Work Item Query** — ensure no blocking bugs
- **REST API Check** — call a health endpoint
- **Azure Policy Compliance** — check compliance score

**Approvals** — a named person/group must click Approve in the portal before the stage runs.

---

### 14.4 Variables and Configuration

```yaml
# Release pipeline variables (set in UI or YAML)
variables:
  # Normal variable
  - name: AppName
    value: 'my-web-app'

  # Secret variable (stored encrypted, not echoed in logs)
  - name: DatabasePassword
    value: $(DB_PASSWORD)   # Linked from variable group / Key Vault

  # Stage-scoped variable (override per environment)
  - name: ConnectionString
    value: 'prod-connection-string'
    scope: Production
```

**Variable Groups** — shared variables across pipelines, with optional **Azure Key Vault** integration for secrets.

---

## 15. Azure Test Plans

### 15.1 Introduction to Testing in Azure DevOps

**Azure Test Plans** provides tools for organizing, managing, and executing tests — both manual and automated.

```
Test Plan (e.g., "Sprint 5 Testing")
├── Test Suite — Static (manual)
│   ├── Test Case: Login with valid credentials
│   ├── Test Case: Login with invalid password
│   └── Test Case: Password reset flow
└── Test Suite — Automated
    └── Linked to Azure Pipelines test results
```

---

### 15.2 Test Plans and Test Suites

| Concept | Description |
|---|---|
| **Test Plan** | Top-level container for a release or sprint's testing |
| **Static Suite** | Manually curated list of test cases |
| **Requirement Suite** | Linked to User Stories/Requirements automatically |
| **Query Suite** | Dynamic — populated by a work item query |
| **Test Case** | Individual test scenario with steps and expected results |

---

### 15.3 Manual Testing

**Creating a Test Case:**
1. Go to **Test Plans** → Select a suite
2. Click **New Test Case**
3. Add **test steps** with Action + Expected Result:
   ```
   Step 1: Navigate to login page
           Expected: Login form is displayed

   Step 2: Enter valid username and password
           Expected: Fields accept input

   Step 3: Click "Login" button
           Expected: User redirected to dashboard
   ```

**Running Manual Tests:**
- Click **Run** on a test case → opens the **Test Runner**
- Mark each step as ✅ Pass or ❌ Fail
- Capture screenshots and create bugs directly from the runner

---

### 15.4 Automated Testing Integration

Link automated tests (from Pipelines) to Test Plans:

```yaml
# azure-pipelines.yml — publish test results
steps:
  - task: DotNetCoreCLI@2
    inputs:
      command: 'test'
      arguments: '--logger trx --results-directory $(Agent.TempDirectory)'

  - task: PublishTestResults@2
    inputs:
      testResultsFormat: 'VSTest'
      testResultsFiles: '**/*.trx'
      mergeTestResults: true
      testRunTitle: 'Unit Tests - $(Build.BuildNumber)'
```

Tests run in pipelines automatically appear in **Test Plans → Runs**, linked to the build.

---

### 15.5 Test Reporting and Analysis

**Test Run Results:**
- Pass rate over time (trend charts)
- Flaky test detection
- Failure analysis with stack traces

**Key Metrics:**
| Metric | Description |
|---|---|
| **Pass Rate** | % of tests passing in a run |
| **Failed Tests** | Tests that need investigation |
| **Flaky Tests** | Tests that pass/fail inconsistently |
| **Test Coverage** | % of code exercised by tests |
| **Duration Trend** | Test suite getting slower over time? |

```bash
# Query test results via Azure DevOps REST API
GET https://dev.azure.com/{org}/{project}/_apis/test/runs?api-version=7.0
```

---

*End of Cloud & Azure DevOps Basics Complete Reference Guide*
