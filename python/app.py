from flask import Flask, request, jsonify
import dbr
import base64
version = dbr.__version__
import urllib.parse

import mrzscanner
import docscanner
import numpy as np
import cv2
import time

from dbr import *

# Get a license key from https://www.dynamsoft.com/customer/license/trialLicense
license_key = "DLS2eyJoYW5kc2hha2VDb2RlIjoiMjAwMDAxLTE2NDk4Mjk3OTI2MzUiLCJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSIsInNlc3Npb25QYXNzd29yZCI6IndTcGR6Vm05WDJrcEQ5YUoifQ=="
BarcodeReader.init_license(license_key)
reader = BarcodeReader()

mrzscanner.initLicense(license_key)
mrz_scanner = mrzscanner.createInstance()
mrz_scanner.loadModel(mrzscanner.load_settings())
    
docscanner.initLicense(license_key)
doc_scanner = docscanner.createInstance()
doc_scanner.setParameters(docscanner.Templates.color)

app = Flask(__name__)

def decode_file_stream(file_content):
    output = []
    try:
        results = reader.decode_file_stream(file_content)
        for result in results:
            output.append({'format': result.barcode_format_string, 'text': result.barcode_text})
    except BarcodeReaderError as error:
        output = error
        
    return output

def mrz_decode_file_stream(file_content):
    output = []
    results = mrz_scanner.decodeMat(file_content)
    for result in results:
        output.append(result.text)
        
    return output


def document_rectify_file_stream(file_content):
    output = []
    results = doc_scanner.detectMat(file_content)
    for result in results:
        x1 = result.x1
        y1 = result.y1
        x2 = result.x2
        y2 = result.y2
        x3 = result.x3
        y3 = result.y3
        x4 = result.x4
        y4 = result.y4
        
        normalized_image = doc_scanner.normalizeBuffer(file_content, x1, y1, x2, y2, x3, y3, x4, y4)
        image_path = os.path.join(os.getcwd(), str(time.time()) + '.png')
        normalized_image.save(image_path)
        output.append(image_path)
        break
        
    return output

def process_file(file_content, sdk):
    output = []
    if sdk == 'dbr':
        output = decode_file_stream(file_content)
    elif sdk == 'mrz':
        output = mrz_decode_file_stream(cv2.imdecode(np.frombuffer(file_content, np.uint8), cv2.IMREAD_COLOR))
    elif sdk == 'document':
        output = document_rectify_file_stream(cv2.imdecode(np.frombuffer(file_content, np.uint8), cv2.IMREAD_COLOR))
    return output
    
def handle_request(request, sdk):
    output = []
    
    request_body = request.data.decode('utf-8')
    if request_body != '':
        try:
            
            base64_content = urllib.parse.unquote(request_body)
            file_content = base64.b64decode(base64_content)
        except:
            return 'Invalid base64 string', 400
        
        output = process_file(file_content, sdk)
        
    else:
        if 'file' not in request.files:
            return 'No file uploaded', 400
        
        file = request.files['file']
        
        if file.filename == '':
            return 'Empty file', 400
        
        file_content = file.read()
        
        output = process_file(file_content, sdk)
        
    return jsonify(results=output)
    
    
@app.route('/api/dbr/version', methods=['GET'])
def dbr_version():
    result = version
    return jsonify(result=result)

@app.route('/api/dbr/decode', methods=['POST'])
def dbr_decode():
    return handle_request(request, 'dbr')

@app.route('/api/mrz/scan', methods=['POST'])
def mrz_scan():
    return handle_request(request, 'mrz')

@app.route('/api/document/rectify', methods=['POST'])
def document_rectify():
    return handle_request(request, 'document')

if __name__ == '__main__':
    app.run()