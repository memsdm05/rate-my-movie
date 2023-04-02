from flask import Flask, request, abort, make_response
import openai
import json
import random

with open("config.json") as f:
    config = json.load(f)

with open("dan.txt") as f:
    dan = f.read().strip()

prompt = (
    "Rate the following movie overview: \"{}\". "
    "Pretend you have enough information to do this. "
    "Make sure your answer is a number between 1 and 10. "
    "Make sure to answer only with the rating. "
)


app = Flask(__name__)

openai.organization = config["org"]
openai.api_key = config["token"]
# print(openai.Model.list())

def ask_gpt(*args, **kwargs):
    kwargs["model"] = "gpt-3.5-turbo"
    resp = openai.ChatCompletion.create(*args, **kwargs)
    return resp

def make_cors_response(*args, **kwargs):
    resp = make_response(*args, **kwargs)
    resp.headers['Access-Control-Allow-Origin'] = '*'
    return resp

@app.get("/rate")
def rate():
    overview = request.args.get("overview")
    if not overview:
        abort(400, "you forgot the overview idiot")
    
    resp = ask_gpt(
        messages=[
            {"role": "user", "content": dan + "\n\n" + prompt.format(overview)}
        ]
    )

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
    print("giving random response")
    return make_cors_response(str(random.randint(2, 8)))

if __name__ == "__main__":
    app.run(debug=True)