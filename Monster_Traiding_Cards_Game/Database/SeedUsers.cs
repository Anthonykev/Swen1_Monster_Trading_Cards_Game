using Monster_Trading_Cards_Game.Repositories;

namespace Monster_Trading_Cards_Game.Database
{
    public static class SeedUsers
    {
        public static void Seed(UserRepository userRepository)
        {
            // Benutzer 1
            string username1 = "admin";
            string password1 = "password123";
            string fullName1 = "Test User 1";
            string email1 = "testuser1@example.com";

            // Benutzer 2
            string username2 = "admin2";
            string password2 = "password123";
            string fullName2 = "Test User 2";
            string email2 = "testuser2@example.com";

            // Überprüfen, ob die Benutzer bereits existieren
            if (!userRepository.UserExists(username1))
            {
                userRepository.CreateUser(username1, password1, fullName1, email1);
            }

            if (!userRepository.UserExists(username2))
            {
                userRepository.CreateUser(username2, password2, fullName2, email2);
            }

            Console.WriteLine("Benutzer wurden erstellt.");
        }
    }
}
