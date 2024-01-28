import json
import requests
import os

def download_image(url, filename):
    response = requests.get(url)
    with open(filename, 'wb') as f:
        f.write(response.content)

def process_exercises(json_file):
    with open(json_file, 'r') as f:
        exercises = json.load(f)

    for exercise in exercises:
        url = exercise['gifUrl']
        filename = url.split('/')[-1]
        download_image(url, filename)
        exercise['gifUrl'] = filename

    with open(json_file, 'w') as f:
        json.dump(exercises, f)

# Call the function with your JSON file
process_exercises('exercises.json')
