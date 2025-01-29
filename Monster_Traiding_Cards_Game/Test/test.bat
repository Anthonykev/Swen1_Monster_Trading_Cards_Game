# Tabellen erstellen
curl -X POST http://127.0.0.1:12000/create-tables

# Standardkarten zur Datenbank hinzufügen
curl -X POST http://127.0.0.1:12000/add-default-cards

# Admins mit festen Tokens erstellen
curl -X POST http://127.0.0.1:12000/seed-users


Admins einloggen
curl -X POST http://127.0.0.1:12000/login -H "Content-Type: application/json" -d "{\"username\":\"admin\",\"password\":\"password123\"}"
curl -X POST http://127.0.0.1:12000/login -H "Content-Type: application/json" -d "{\"username\":\"admin2\",\"password\":\"password123\"}"

#Admin Pakete kaufen
curl -X POST http://127.0.0.1:12000/buy-package -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-1\", \"username\":\"admin\"}"
curl -X POST http://127.0.0.1:12000/buy-package -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-2\", \"username\":\"admin2\"}"


#get all users cards 
curl -X POST http://127.0.0.1:12000/get-user-cards -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-1\", \"username\":\"admin\"}"
curl -X POST http://127.0.0.1:12000/get-user-cards -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-2\", \"username\":\"admin2\"}"


# Deck für Admins auswählen
curl -X POST http://127.0.0.1:12000/choose-deck -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-1\", \"username\":\"admin\", \"cardIds\":[17,4,9,18]}"
curl -X POST http://127.0.0.1:12000/choose-deck -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-2\", \"username\":\"admin2\", \"cardIds\":[5,6,15,8]}"



# Battle-Anfrage senden (zwei admins)
curl -X POST http://127.0.0.1:12000/battle-request -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-1\", \"username\":\"admin\"}"
curl -X POST http://127.0.0.1:12000/battle-request -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-2\", \"username\":\"admin2\"}"


# Benutzer registrieren (zwei user)
curl -X POST http://127.0.0.1:12000/register -H "Content-Type: application/json" -d "{\"username\":\"kevin\",\"password\":\"password123\",\"fullname\":\"Test User\",\"email\":\"kodzo@example.com\"}" 
curl -X POST http://127.0.0.1:12000/register -H "Content-Type: application/json" -d "{\"username\":\"mike\",\"password\":\"password123\",\"fullname\":\"Test User\",\"email\":\"kodzo2@example.com\"}" 

# Benutzer einloggen (zwei user)
curl -X POST http://127.0.0.1:12000/login -H "Content-Type: application/json" -d "{\"username\":\"kevin\",\"password\":\"password123\"}"
curl -X POST http://127.0.0.1:12000/login -H "Content-Type: application/json" -d "{\"username\":\"mike\",\"password\":\"password123\"}"

# ELO-Rangliste abrufen
curl -X GET http://127.0.0.1:12000/get-elo-ranking

# Unique Features
curl -X POST http://127.0.0.1:12000/set-motto -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-1\", \"username\":\"admin\", \"motto\":\"Coding is my passion.\"}"
curl -X POST http://127.0.0.1:12000/set-motto -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-2\", \"username\":\"admin2\", \"motto\":\"Kevin is my motivation.\"}"



# Benutzer ausloggen
curl -X POST http://127.0.0.1:12000/sessions/logout -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-1\"}"
curl -X POST http://127.0.0.1:12000/sessions/logout -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-2\"}"







