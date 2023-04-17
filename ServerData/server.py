from flask import Flask, request, jsonify, make_response, send_file, render_template, abort
import json
import os
import uuid

app = Flask(__name__)

def get_user_state():
    user_state_str = request.headers.get('X-User-State', None)

    if user_state_str is not None:
        user_state = json.loads(user_state_str)
    else:
        user_state = {
            'session': str(uuid.uuid4()),
            'day': True,
            'count': 0,
        }
    
    return user_state

def set_user_state(response, user_state):
    response.headers['X-User-State'] = json.dumps(user_state)

@app.route('/home')
def api_sample():
    user_state = get_user_state()
    mode = request.args.get('mode', '')
    if mode != '':
        user_state['day'] = (mode != 'night')
    data = [
        {
            "id": "Globe",
            "overwrite": "true",
            "model": "Sphere",
            "material": "Unlit",
            "texture": "globe_texture.jpg" if user_state['day'] else "globe_night.jpg",
            "location": "0, 0, 0",
            "direction": "0, 0, 0",
            "scale": "1, 1, 1",
            "actions": [
                { "action": "open", "param": "view_sample" }
            ]
        },
    ]
    response = make_response(jsonify(data))
    response.headers['Content-Type'] = 'application/json'
    set_user_state(response, user_state)
    return response

@app.route('/view_sample')
def view_sample():
    user_state = get_user_state()

    count = user_state['count']

    user_state['count'] = count + 1

    html = render_template('view_sample.tmpl.html',
        session=user_state['session'],
        count=count,
        next_mode='night' if user_state['day'] else 'day',
        state=json.dumps(user_state))
    response = make_response(html)
    response.headers['Content-Type'] = 'text/html; charset=UTF-8'
    set_user_state(response, user_state)
    return response

content_types = {
    '.json': 'application/json',
    '.png': 'image/png',
    '.jpg': 'image/jpeg',
    '.html': 'text/html'
}

@app.route('/<path:filename>')
def get_file(filename):
    file_ext = os.path.splitext(filename)[1].lower()

    if file_ext not in content_types:
        abort(403)

    cwd = os.getcwd()
    file_path = os.path.join(cwd, filename)

    # prevent directory traversal attack
    if not os.path.abspath(file_path).startswith(cwd):
        abort(403)

    if not os.path.isfile(file_path):
        abort(404)

    response = send_file(file_path)
    response.headers['Content-Type'] = content_types[file_ext]
    return response

if __name__ == '__main__':
    app.run(port=8080)