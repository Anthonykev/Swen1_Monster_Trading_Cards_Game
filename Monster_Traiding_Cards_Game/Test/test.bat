
curl -X POST http://127.0.0.1:12000/create-tables
curl -X POST http://127.0.0.1:12000/add-default-cards
curl -X POST http://127.0.0.1:12000/seed-users
curl -X POST http://127.0.0.1:12000/login -H "Content-Type: application/json" -d "{\"username\":\"admin\",\"password\":\"password123\"}"
curl -X POST http://127.0.0.1:12000/login -H "Content-Type: application/json" -d "{\"username\":\"admin2\",\"password\":\"password123\"}"
curl -X POST http://127.0.0.1:12000/set-motto -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-1\", \"username\":\"admin\", \"motto\":\"I am the King!!!\"}"
curl -X POST http://127.0.0.1:12000/set-motto -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-2\", \"username\":\"admin2\", \"motto\":\"I am just better than you!!!\"}"
curl -X POST http://127.0.0.1:12000/buy-package -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-1\", \"username\":\"admin\"}"
curl -X POST http://127.0.0.1:12000/buy-package -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-2\", \"username\":\"admin2\"}"
curl -X POST http://127.0.0.1:12000/get-user-cards -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-1\", \"username\":\"admin\"}"
curl -X POST http://127.0.0.1:12000/get-user-cards -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-2\", \"username\":\"admin2\"}"
curl -X POST http://127.0.0.1:12000/choose-deck -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-1\", \"username\":\"admin\", \"cardIds\":[17,6,1,15]}"
curl -X POST http://127.0.0.1:12000/choose-deck -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-2\", \"username\":\"admin2\", \"cardIds\":[12,6,14,7]}"
curl -X POST http://127.0.0.1:12000/battle-request -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-1\", \"username\":\"admin\"}"
curl -X POST http://127.0.0.1:12000/battle-request -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-2\", \"username\":\"admin2\"}"
curl -X POST http://127.0.0.1:12000/register -H "Content-Type: application/json" -d "{\"username\":\"kevin\",\"password\":\"password123\",\"fullname\":\"Test User\",\"email\":\"kodzo@example.com\"}" 
curl -X POST http://127.0.0.1:12000/register -H "Content-Type: application/json" -d "{\"username\":\"mike\",\"password\":\"password123\",\"fullname\":\"Test User\",\"email\":\"kodzo2@example.com\"}" 
curl -X POST http://127.0.0.1:12000/login -H "Content-Type: application/json" -d "{\"username\":\"kevin\",\"password\":\"password123\"}"
curl -X POST http://127.0.0.1:12000/login -H "Content-Type: application/json" -d "{\"username\":\"mike\",\"password\":\"password123\"}"
curl -X GET http://127.0.0.1:12000/get-elo-ranking
curl -X GET http://127.0.0.1:12000/get-all-users-mottos
curl -X POST http://127.0.0.1:12000/sessions/logout -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-1\"}"
curl -X POST http://127.0.0.1:12000/sessions/logout -H "Content-Type: application/json" -d "{\"token\":\"fixed-token-2\"}"







