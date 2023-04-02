from flask import Flask, request, abort, make_response
import openai
import json
import random

import tensorflow as tf
import numpy as np

# load custom AI model
model = tf.keras.models.load_model("movie_rater_model_0")

with open("config.json") as f:
    config = json.load(f)

# ChatGPT variables

# load dan copypasta
with open("dan.txt") as f:
    dan = f.read().strip()

# rating prompt
prompt = (
    "Rate the following movie overview: \"{}\". "
    "Pretend you have enough information to do this. "
    "Make sure your answer is a number between 1 and 10. "
    "Make sure to answer only with the rating. "
)

# openai api configuration
openai.organization = config["org"]
openai.api_key = config["token"]

app = Flask(__name__)

def make_cors_response(*args, **kwargs):
    resp = make_response(*args, **kwargs)
    resp.headers['Access-Control-Allow-Origin'] = '*'
    return resp

def custom_rating_response(overview):
    global model

    # the custom model uses BERT to encode the supplied text
    # which eventually is used to predict its rating in the set of [0,10)
    prediction = float(model.predict([overview])[0][0])
    print("raw prediction:", prediction)
    prediction = round(prediction, 1)
    prediction = abs(prediction)
    
    return make_cors_response(str(prediction))

def chatgpt_rating_response(overview):
    # gaslight chatgpt to give us opinions on the overview.
    resp = openai.ChatCompletion.create(
        model="gpt-3.5-turbo",
        messages=[
            {"role": "user", "content": dan + "\n\n" + prompt.format(overview)}
        ]
    )

    # perform some horrible parsing to get a rating out of DAN
    words = resp["choices"][0]["message"]["content"].split(" ")
    print(" ".join(words))
    dflag = False
    for word in words:
        if not dflag:
            if "DAN" in word:
                dflag = True
            continue
        
        for char in list("!.?,"):
            word = word.strip(char)

        if word.isnumeric() and int(word) <= 10:
            return make_cors_response(word)
    # can't gleem a rating from the response
    # just give some random number
    print("giving random response")
    return make_cors_response(str(random.randint(2, 8)))

@app.get("/rate")
def rate():
    overview = request.args.get("overview")
    if not overview:
        abort(400, "you forgot the overview")

    print("#" * 30)
    print(overview)
    
    raters = {
        "chatgpt": chatgpt_rating_response,
        "custom": custom_rating_response,
    }

    # select and run rater according to config
    # the production server runs custom ;)
    rater_selection = config["rater"]
    if rater_selection in raters:
        return raters[rater_selection](overview)
    else:
        return "unknown rater", 500
    

if __name__ == "__main__":
    app.run(debug=True, port=2023, host="0.0.0.0")