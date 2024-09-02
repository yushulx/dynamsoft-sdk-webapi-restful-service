# Dynamsoft Vision SDK RESTful Service

This project is designed to create a RESTful service for Dynamsoft products, encompassing Dynamic Web TWAIN, Dynamsoft Barcode Reader, Dynamsoft Label Recognizer, and Dynamsoft Document Normalizer. The service is developed with .NET WebAPI and can be tested using the included web client project.



https://github.com/yushulx/dynamsoft-sdk-dotnet-webapi-restful-service/assets/2202306/c097b67f-6e5c-47f0-8ecc-2ce93be7bceb



## Supported Deployment Platforms
- Windows
- Linux

## Getting Started

To get started with the project, follow these steps:

1. Clone the repository to your local machine.
2. Open the project in Visual Studio Code.
3. Request a [trial license](https://www.dynamsoft.com/customer/license/trialLicense) for each Dynamsoft product you wish to use. Replace the license keys in the `LicenseManager.cs` file for each product.
4. Start the RESTful service by running the following command in the terminal:

    ```bash
    dotnet run
    ```
5. Open the web client project for testing the RESTful service.

    ```bash
    cd client
    npm install
    node app.js
    ```

**Dynamic Web TWAWIN**

<img src="https://github.com/yushulx/dynamsoft-sdk-dotnet-webapi-restful-service/assets/2202306/b41bc94f-4f91-40c3-8aa2-37513bb7417b" width="400">


**Dynamsoft Barcode Reader**

<img src="https://github.com/yushulx/dynamsoft-sdk-dotnet-webapi-restful-service/assets/2202306/26dc6be3-202b-443f-ac30-0d081d6d9516" width="400">


**Dynamsoft Label Recognizer**

<img src="https://github.com/yushulx/dynamsoft-sdk-dotnet-webapi-restful-service/assets/2202306/0555ae72-cacc-423e-a68d-21d0b37bb1a6" width="400">


**Dynamsoft Document Normalizer**

<img src="https://github.com/yushulx/dynamsoft-sdk-dotnet-webapi-restful-service/assets/2202306/2e2ce815-40b1-4745-9485-b897c8009c21" width="400">


## Usage

The RESTful service provides endpoints for each of the supported Dynamsoft products, including:


| Method | Endpoint | Parameters | Response | Description |
| ------ | -------- | ---------- | ------- | ----------- |
| GET    | `/dynamsoft/product` | - | Product list | Retrieve all Dynamsoft products. |
| GET    | `/dynamsoft/dwt/GetDevices` | - | Scanner list | List available scanners. |
| POST   | `/dynamsoft/dwt/ScanDocument` | Scanner config | Job ID | Initiate a scanning job. |
| GET    | `/dynamsoft/dwt/GetImageStream/{jobId}` | jobId | Image stream | Fetch image stream for a specific job. |
| POST   | `/dynamsoft/dbr/DecodeBarcode` | Image stream | Barcode results | Decode barcodes using the Dynamsoft Barcode Reader. |
| POST   | `/dynamsoft/ddn/rectifyDocument` | Image stream | Mrz results | Detect MRZ from an image with Dynamsoft Label Recognizer. |
| POST   | `/dynamsoft/dlr/DetectMrz` | Image stream | Rectified document image | Rectify a document with Dynamsoft Document Normalizer. |

