
# Data Engineering Microservices

The following microservices support various data engineering use cases, including pre-processing, data ingestion, transformation, feature engineering, and modeling:

## Services

### 1. CommonWebAPI
- **Functionality**: User Login, Authentication, Registration
- **Type**: API Service

### 2. Data Service
- **Functionality**: 
  - Manage data transformation pipeline tasks/projects
  - Construct schema modeling
  - Workflow scheduling
- **Type**: Service

### 3. FileUpload
- **Functionality**:
  - Upload data files (CSV, JSON)
  - Support third-party API-based data pulls (currently Twitter)
- **Type**: Service

### 4. DataShareService
- **Functionality**:
  - Provide API access to data
  - Enable user-accessible API for authorized users through API key
  - Retrieve data from tables
  - Similarity search
- **Type**: API Service

### 5. Gateway Service
- **Functionality**: Act as an API gateway to route all user-end API calls
- **Type**: Ocelot-based Service

## Architecture Diagram of the full system

![Microservices Architecture](https://github.com/kmranikmr/bundleservices/assets/173465556/8f899121-5586-48ca-a94a-9f9179d99de4)

## AWS deployment


![image](https://github.com/kmranikmr/bundleservices/assets/173465556/d057ef66-63b3-4a17-a3c8-d157a7188956)


