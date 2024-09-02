# Dynamsoft SDK Web Service
The sample creates web APIs for Dynamsoft SDKs including Dynamsoft Barcode Reader, Dynamsoft Document Normalizer and Dynamsoft Label Recognizer. 

https://github.com/yushulx/dynamsoft-sdk-web-service/assets/2202306/01f40f3a-87cd-4f0e-b100-65b363b2832e

## Installation
```bash
pip install -r requirements.txt
```

## Setup
1. Apply for a [trial license](https://www.dynamsoft.com/customer/license/trialLicense) for Dynamsoft SDKs, and then set the license keys in `app.py`:
    
    ```python
    license_key = "DLS2eyJoYW5kc2hha2VDb2RlIjoiMjAwMDAxLTE2NDk4Mjk3OTI2MzUiLCJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSIsInNlc3Npb25QYXNzd29yZCI6IndTcGR6Vm05WDJrcEQ5YUoifQ=="
    ```
2. Run the Flask server:
    
    ```bash
    python app.py
    ```
## Usage
- cURL commands:

    ```bash
    # barcode
    curl -X POST -F 'file=@./barcode.jpg' 127.0.0.1:5000/api/dbr/decode

    # mrz
    curl -X POST -F 'file=@./mrz.png' http://127.0.0.1:5000/api/mrz/scan

    # document
    curl -X POST -F 'file=@./document.png' http://127.0.0.1:5000/api/document/rectify
    ```
- Power Automate
  
  ![Power Automate with Dynamsoft SDK service](https://www.dynamsoft.com/codepool/img/2023/05/power-automate-dynamsoft-service.png)
