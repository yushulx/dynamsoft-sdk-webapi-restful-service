from flask import Flask, request, jsonify
import base64
import urllib.parse

from dynamsoft_capture_vision_bundle import (
    LicenseManager, CaptureVisionRouter, EnumPresetTemplate, EnumErrorCode,
    EnumImagePixelFormat
)
import cv2
import numpy as np
import time
import os

# Get a license key from https://www.dynamsoft.com/customer/license/trialLicense
license_key = "DLS2eyJoYW5kc2hha2VDb2RlIjoiMjAwMDAxLTE2NDk4Mjk3OTI2MzUiLCJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSIsInNlc3Npb25QYXNzd29yZCI6IndTcGR6Vm05WDJrcEQ5YUoifQ=="

# Initialize license
LicenseManager.init_license(license_key)

# Create CaptureVisionRouter instance for barcode detection
cvr_barcode = CaptureVisionRouter()

# Create CaptureVisionRouter instance for MRZ detection
cvr_mrz = CaptureVisionRouter()

# Create CaptureVisionRouter instance for document detection
cvr_document = CaptureVisionRouter()

app = Flask(__name__)

def decode_file_stream(file_content):
    output = []
    try:
        results = cvr_barcode.capture_multi_pages(file_content, EnumPresetTemplate.PT_READ_BARCODES)
        result_list = results.get_results()
        for result in result_list:
            if result.get_error_code() == EnumErrorCode.EC_OK:
                items = result.get_items()
                for item in items:
                    text = item.get_text()
                    format_str = item.get_format_string()
                    output.append({'format': format_str, 'text': text})
    except Exception as error:
        output = str(error)
        
    return output


def mrz_decode_file_stream(file_content):
    output = []
    try:
        results = cvr_mrz.capture_multi_pages(file_content, "ReadPassportAndId")
        result_list = results.get_results()
        for result in result_list:
            if result.get_error_code() == EnumErrorCode.EC_OK:
                # Get recognized text lines (raw MRZ text)
                line_result = result.get_recognized_text_lines_result()
                if line_result:
                    items = line_result.get_items()
                    for item in items:
                        text = item.get_text()
                        output.append(text)
                
                # Get parsed results (structured MRZ data)
                parsed_result = result.get_parsed_result()
                if parsed_result:
                    parsed_items = parsed_result.get_items()
                    for item in parsed_items:
                        code_type = item.get_code_type()
                        # Extract key fields from parsed MRZ
                        doc_id = item.get_field_value("passportNumber") or item.get_field_value("documentNumber")
                        nationality = item.get_field_value("nationality")
                        issuer = item.get_field_value("issuingState")
                        dob = item.get_field_value("dateOfBirth")
                        expiry = item.get_field_value("dateOfExpiry")
                        gender = item.get_field_value("sex")
                        surname = item.get_field_value("primaryIdentifier")
                        given_name = item.get_field_value("secondaryIdentifier")
                        
                        parsed_info = f"Type: {code_type}"
                        if doc_id:
                            parsed_info += f", ID: {doc_id}"
                        if surname:
                            parsed_info += f", Name: {surname}"
                        if given_name:
                            parsed_info += f" {given_name}"
                        if nationality:
                            parsed_info += f", Nationality: {nationality}"
                        if dob:
                            parsed_info += f", DOB: {dob}"
                        if expiry:
                            parsed_info += f", Expiry: {expiry}"
                        if gender:
                            parsed_info += f", Gender: {gender}"
                        output.append(parsed_info)
    except Exception as error:
        output = str(error)
        
    return output


def convertImageData2Mat(normalized_image):
    """Convert ImageData to OpenCV Mat"""
    ba = bytearray(normalized_image.get_bytes())
    width = normalized_image.get_width()
    height = normalized_image.get_height()

    channels = 3
    if normalized_image.get_image_pixel_format() == EnumImagePixelFormat.IPF_BINARY:
        channels = 1
        all_pixels = []
        skip = normalized_image.stride * 8 - width

        index = 0
        n = 1
        for byte in ba:
            byteCount = 7
            while byteCount >= 0:
                b = (byte & (1 << byteCount)) >> byteCount

                if index < normalized_image.stride * 8 * n - skip:
                    if b == 1:
                        all_pixels.append(255)
                    else:
                        all_pixels.append(0)

                byteCount -= 1
                index += 1

            if index == normalized_image.stride * 8 * n:
                n += 1

        mat = np.array(all_pixels, dtype=np.uint8).reshape(height, width, channels)
        return mat

    elif normalized_image.get_image_pixel_format() == EnumImagePixelFormat.IPF_GRAYSCALED:
        channels = 1

    mat = np.array(ba, dtype=np.uint8).reshape(height, width, channels)
    return mat


def document_rectify_file_stream(file_content):
    output = []
    try:
        results = cvr_document.capture_multi_pages(file_content, EnumPresetTemplate.PT_DETECT_AND_NORMALIZE_DOCUMENT)
        result_list = results.get_results()
        
        for result in result_list:
            if result.get_error_code() == EnumErrorCode.EC_OK:
                processed_doc_result = result.get_processed_document_result()
                if processed_doc_result:
                    deskewed_items = processed_doc_result.get_deskewed_image_result_items()
                    for item in deskewed_items:
                        # Get the normalized image data
                        normalized_image_data = item.get_image_data()
                        if normalized_image_data:
                            # Convert ImageData to OpenCV Mat
                            normalized_image = convertImageData2Mat(normalized_image_data)
                            
                            image_path = os.path.join(os.getcwd(), str(time.time()) + '.png')
                            cv2.imwrite(image_path, normalized_image)
                            output.append(image_path)
                            break
    except Exception as error:
        output = str(error)
        
    return output


def process_file(file_content, sdk):
    output = []
    if sdk == 'dbr':
        output = decode_file_stream(file_content)
    elif sdk == 'mrz':
        output = mrz_decode_file_stream(file_content)
    elif sdk == 'document':
        output = document_rectify_file_stream(file_content)
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
    

@app.route('/api/dbr/decode', methods=['POST'])
def dbr_decode():
    return handle_request(request, 'dbr')

@app.route('/api/mrz/scan', methods=['POST'])
def mrz_scan():
    return handle_request(request, 'mrz')

@app.route('/api/document/rectify', methods=['POST'])
def document_rectify():
    return handle_request(request, 'document')


# Test functions
def test_barcode_detection():
    """Test barcode detection with barcode.jpg"""
    print("\n=== Testing Barcode Detection ===")
    try:
        image_path = os.path.join(os.getcwd(), 'barcode.jpg')
        if os.path.exists(image_path):
            with open(image_path, 'rb') as f:
                file_content = f.read()
            results = decode_file_stream(file_content)
            print(f"Barcode detection results: {results}")
            return results
        else:
            print(f"Image not found: {image_path}")
            return []
    except Exception as e:
        print(f"Error in barcode detection: {e}")
        return []


def test_mrz_detection():
    """Test MRZ detection with mrz.png"""
    print("\n=== Testing MRZ Detection ===")
    try:
        image_path = os.path.join(os.getcwd(), 'mrz.png')
        if os.path.exists(image_path):
            with open(image_path, 'rb') as f:
                file_content = f.read()
            results = mrz_decode_file_stream(file_content)
            print(f"MRZ detection results: {results}")
            return results
        else:
            print(f"Image not found: {image_path}")
            return []
    except Exception as e:
        print(f"Error in MRZ detection: {e}")
        return []


def test_document_detection():
    """Test document detection with document.png"""
    print("\n=== Testing Document Detection ===")
    try:
        image_path = os.path.join(os.getcwd(), 'document.png')
        if os.path.exists(image_path):
            with open(image_path, 'rb') as f:
                file_content = f.read()
            results = document_rectify_file_stream(file_content)
            print(f"Document detection results: {results}")
            return results
        else:
            print(f"Image not found: {image_path}")
            return []
    except Exception as e:
        print(f"Error in document detection: {e}")
        return []


def run_all_tests():
    """Run all tests and report results"""
    print("=" * 50)
    print("Running All Tests")
    print("=" * 50)
    
    # Test barcode detection
    barcode_results = test_barcode_detection()
    barcode_success = len(barcode_results) > 0 and not isinstance(barcode_results, str)
    
    # Test MRZ detection
    mrz_results = test_mrz_detection()
    mrz_success = len(mrz_results) > 0 and not isinstance(mrz_results, str)
    
    # Test document detection
    document_results = test_document_detection()
    document_success = len(document_results) > 0 and not isinstance(document_results, str)
    
    # Summary
    print("\n" + "=" * 50)
    print("Test Summary")
    print("=" * 50)
    print(f"Barcode Detection: {'PASSED' if barcode_success else 'FAILED'}")
    print(f"MRZ Detection: {'PASSED' if mrz_success else 'FAILED'}")
    print(f"Document Detection: {'PASSED' if document_success else 'FAILED'}")
    
    return {
        'barcode': barcode_success,
        'mrz': mrz_success,
        'document': document_success
    }


if __name__ == '__main__':
    # Run tests first
    test_results = run_all_tests()
    
    # Start Flask server if all tests pass or user wants to continue
    print("\nStarting Flask server...")
    # Use 0.0.0.0 to allow external connections (requires admin on Windows)
    # Use 127.0.0.1 for localhost only (default, no admin required)
    import sys
    host = '0.0.0.0' if '--host-all' in sys.argv else '127.0.0.1'
    print(f"Server running on http://{host}:5000")
    app.run(host=host, port=5000)
