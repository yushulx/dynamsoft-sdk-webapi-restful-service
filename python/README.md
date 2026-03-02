# Dynamsoft SDK Web Service
The sample creates web APIs for Dynamsoft SDKs using Dynamsoft Capture Vision Bundle, which includes Dynamsoft Barcode Reader, Dynamsoft Document Normalizer, and Dynamsoft Label Recognizer. 

https://github.com/yushulx/dynamsoft-sdk-web-service/assets/2202306/01f40f3a-87cd-4f0e-b100-65b363b2832e

## Installation
```bash
pip install -r requirements.txt
```

## Setup
1. Apply for a [trial license](https://www.dynamsoft.com/customer/license/trialLicense/?product=dcv&package=cross-platform) for Dynamsoft SDKs, and then set the license keys in `app.py`:
    
    ```python
    license_key = "LICENSE-KEY"
    ```
2. Run the Flask server:
    
    ```bash
    python app.py
    ```
## Usage
1. Run the Flask server (localhost only):
    ```bash
    python app.py
    ```

    Or run with external access (requires administrator privileges on Windows):
    ```bash
    python app.py --host-all
    ```

2. Test with cURL commands (use `curl.exe` in PowerShell, or `curl` in CMD/Git Bash):

    ```bash
    # barcode
    curl.exe -X POST -F 'file=@./barcode.jpg' http://127.0.0.1:5000/api/dbr/decode

    # mrz
    curl.exe -X POST -F 'file=@./mrz.png' http://127.0.0.1:5000/api/mrz/scan

    # document
    curl.exe -X POST -F 'file=@./document.png' http://127.0.0.1:5000/api/document/rectify
    ```
- Power Automate
  
  ![Power Automate with Dynamsoft SDK service](https://www.dynamsoft.com/codepool/img/2023/05/power-automate-dynamsoft-service.png)
