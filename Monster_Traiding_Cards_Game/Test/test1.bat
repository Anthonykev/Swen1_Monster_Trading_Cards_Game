# create user
curl -i -X POST http://localhost:12000/users --header "Content-Type: application/json" -d "{\"username\":\"batman\", \"password\":\"batcave\", \"name\":\"Bruce Wayne\", \"email\":\"office@batman.org\"}"
curl -i -X POST http://localhost:12000/users --header "Content-Type: application/json" -d "{\"username\":\"oswin\", \"password\":\"souffle\", \"name\":\"Clara Oswald\"}"

# shouldn't work
curl -i -X POST http://localhost:12000/users --header "Content-Type: application/json" -d "{\"username\":\"oswin\", \"password\":\"none\", \"name\":\"Oswin Oswald\"}"

# logon
curl -i -X POST http://localhost:12000/sessions --header "Content-Type: application/json" -d "{\"username\":\"oswin\", \"password\":\"souffle\"}"



curl -i -X GET http://localhost:12000/users/oswin --header "Content-Type: application/json" --header "Authorization: Bearer oswin-debug"