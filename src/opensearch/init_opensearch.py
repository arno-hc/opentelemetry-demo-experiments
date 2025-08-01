# Initialize OpenSearch
#
# Needs too be run once after setting up demo system
#
# Documentation: https://docs.opensearch.org/latest/clients/python-low-level/
# pip install opensearch-py  

import requests
from requests.auth import HTTPBasicAuth

## Create index patterns (see https://forum.opensearch.org/t/create-index-pattern-via-python3-api/16455
# and https://stackoverflow.com/questions/71816340/how-to-create-an-index-pattern-in-opensearch-using-api)
# for otel-logs
url = "http://localhost:5601/api/saved_objects/index-pattern/otel-logs"
headers = {
    "osd-xsrf": "true",
    "Content-Type": "application/json"
}
data = {
    "attributes": {
        "title": "otel-logs-*",
        "timeFieldName": "@timestamp"
    }
}

response = requests.post(url, headers=headers, json=data)

if response.status_code == 200 or response.status_code == 201:
    print("Index pattern created successfully.")
else:
    print(f"Failed to create index pattern. Status code: {response.status_code}, Response: {response.text}")

# for ss4o spans
url = "http://localhost:5601/api/saved_objects/index-pattern/ss4o_traces"
headers = {
    "osd-xsrf": "true",
    "Content-Type": "application/json"
}
data = {
    "attributes": {
        "title": "ss4o_traces-*",
        "timeFieldName": "startTime"
    }
}

response = requests.post(url, headers=headers, json=data)

if response.status_code == 200 or response.status_code == 201:
    print("Index pattern created successfully.")
else:
    print(f"Failed to create index pattern. Status code: {response.status_code}, Response: {response.text}")