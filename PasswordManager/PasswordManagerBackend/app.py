from flask import Flask, request, jsonify
from flask_sqlalchemy import SQLAlchemy
from flask_cors import CORS

# Inicjalizacja aplikacji Flask
app = Flask(__name__)
CORS(app)

# Konfiguracja bazy danych
app.config['SQLALCHEMY_DATABASE_URI'] = 'sqlite:///passwords.db'
app.config['SQLALCHEMY_TRACK_MODIFICATIONS'] = False
db = SQLAlchemy(app)

# Model bazy danych
class Password(db.Model):
    id = db.Column(db.Integer, primary_key=True)
    site = db.Column(db.String(50), nullable=False)
    username = db.Column(db.String(50), nullable=False)
    password = db.Column(db.Text, nullable=False)
    key_id = db.Column(db.String(50), nullable=False)  # Identyfikator klucza AES


# Endpoint: Dodawanie hasła
@app.route('/password', methods=['POST'])
def add_password():
    data = request.json
    new_password = Password(
        site=data['site'],
        username=data['username'],
        password=data['password'],  # Zaszyfrowane hasło
        key_id=data['key_id']  # Identyfikator klucza
    )
    db.session.add(new_password)
    db.session.commit()
    return jsonify({'message': 'Password added!'})


# Endpoint: Pobieranie wszystkich haseł
@app.route('/passwords', methods=['GET'])
def get_all_passwords():
    passwords = Password.query.all()
    result = [
    {
        'id': p.id,
        'site': p.site,
        'username': p.username,
        'password': p.password,
        'key_id': p.key_id
    } for p in passwords]
    return jsonify(result)

# Tworzenie tabel w bazie danych
if __name__ == '__main__':
    with app.app_context():  # Ustawienie kontekstu aplikacji
        db.create_all()  # Tworzenie tabel w bazie danych
    app.run(debug=True)
