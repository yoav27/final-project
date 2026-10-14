using Microsoft.AspNetCore.Mvc; // יבוא תמיכה ב-IActionResult ושימוש ב-asp-page-handler
using Microsoft.AspNetCore.Mvc.RazorPages; // יבוא מחלקות בסיס לניהול Razor Pages
using Npgsql; // יבוא הספרייה הרשמית להתקשרות וניהול שאילתות מול PostgreSQL
using System; // יבוא מחלקות בסיס כמו Exception ו-Convert
using System.Collections.Generic; // יבוא תמיכה ברשימות דינמיות (List)

namespace final_project.Pages
{
    // מחלקת ה-Model המנהלת את הנתונים והלוגיקה השרתית של דף ניהול המערכת
    public class AdminModel : PageModel
    {
        // מחרוזת החיבור המדויקת והקבועה שלך ל-PostgreSQL
        private readonly string _connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=1234;";

        // משתנה לאחסון סך כל המשתמשים במערכת (מוצג בכרטיסייה הראשונה)
        public int TotalUsers { get; set; }

        // רשימה לאחסון המשתמשים שנשלפו מתוך בסיס הנתונים (מוצגים בטבלה)
        public List<UserModel> RecentUsers { get; set; } = new List<UserModel>();

        // מתודה שמורצת באופן אוטומטי בעת טעינת הדף (בקשת HTTP GET)
        public void OnGet()
        {
            LoadDataFromDatabase(); // קריאה למתודת העזר לשליפת המשתמשים והספירה מ-PostgreSQL
        }

        // מתודה שמורצת בעת לחיצה על כפתור המחיקה בטבלה (בקשת HTTP POST עם Handler בשם Delete)
        public IActionResult OnPostDelete(int id)
        {
            try
            {
                // יצירת חיבור מול PostgreSQL בתוך בלוק using לסגירה וניקוי משאבים אוטומטי
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open(); // פתיחת החיבור הפיזי מול מסד הנתונים

                    // שאילתת SQL למחיקת המשתמש הספציפי מטבלת users לפי מזהה ה-ID
                    string deleteQuery = "DELETE FROM users WHERE id = @id";
                    using (var cmd = new NpgsqlCommand(deleteQuery, conn))
                    {
                        // הוספת פרמטר בצורה מאובטחת למניעת מתקפות SQL Injection
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery(); // הרצת פקודת המחיקה בשרת
                    }
                }
            }
            catch (Exception ex)
            {
                // הדפסת הודעת שגיאה במידה והמחיקה נכשלה
                Console.WriteLine($"Error deleting user: {ex.Message}");
            }

            // רענון הדף מחדש באופן אוטומטי לאחר ביצוע המחיקה
            return RedirectToPage();
        }

        // מתודת עזר המבצעת את שליפת הנתונים מטבלת users ב-PostgreSQL
        private void LoadDataFromDatabase()
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open(); // פתיחת החיבור מול מסד הנתונים

                    // 1. שאילתת SQL לשליפת סך כל השורות בטבלת users
                    string countQuery = "SELECT COUNT(*) FROM users";
                    using (var cmdCount = new NpgsqlCommand(countQuery, conn))
                    {
                        // ExecuteScalar מחזיר ערך יחיד (מספר השורות) והמרתו ל-int
                        TotalUsers = Convert.ToInt32(cmdCount.ExecuteScalar());
                    }

                    // 2. שאילתת SQL לשליפת id, full_name, username, email מתוך טבלת users
                    string usersQuery = "SELECT id, full_name, username, email FROM users ORDER BY id DESC LIMIT 10";
                    using (var cmdUsers = new NpgsqlCommand(usersQuery, conn))
                    {
                        using (var reader = cmdUsers.ExecuteReader())
                        {
                            // לולאה שעוברת על כל שורה שהוחזרה מ-PostgreSQL
                            while (reader.Read())
                            {
                                int userId = reader.GetInt32(0); // קריאת עמודת id
                                string userFullName = reader.IsDBNull(1) ? "—" : reader.GetString(1); // קריאת full_name בדיוק כפי שמופיע במסד הנתונים
                                string userUsername = reader.IsDBNull(2) ? "—" : reader.GetString(2); // קריאת עמודת username
                                string userEmail = reader.IsDBNull(3) ? "—" : reader.GetString(3); // קריאת עמודת email

                                // הוספת המשתמש עם השם המלא המקורי מ-PostgreSQL
                                RecentUsers.Add(new UserModel
                                {
                                    Id = userId,
                                    FullName = userFullName, // מציג בדיוק את מה שנשמר ב-full_name
                                    Username = userUsername,
                                    Email = userEmail
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // הדפסת הודעת שגיאה במידה והשליפה נכשלה
                Console.WriteLine($"Error reading from PostgreSQL: {ex.Message}");
            }
        }
    }

    // מודל נתונים פשוט המייצג שורת משתמש בודדת בטבלה
    public class UserModel
    {
        public int Id { get; set; } // מזהה ייחודי
        public string FullName { get; set; } = string.Empty; // שם מלא
        public string Username { get; set; } = string.Empty; // שם משתמש
        public string Email { get; set; } = string.Empty; // כתובת אימייל
    }
}
