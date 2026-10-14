// ייבוא מחלקות הבסיס של ASP.NET Core Razor Pages
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

// ייבוא הספרייה לחיבור ומעבר נתונים מול בסיס הנתונים PostgreSQL
using Npgsql;

// ייבוא ספריות מובנות ב-NET. עבור הצפנה והמרת מחרוזות
using System.Security.Cryptography;
using System.Text;

namespace final_project.Pages
{
    public class RegisterModel : PageModel
    {
        // משתנה פרטי לשמירת תצורת המערכת (לקריאת קובץ appsettings.json)
        private readonly IConfiguration _configuration;

        // הזרקת התלות (Dependency Injection) של IConfiguration דרך ה-Constructor
        public RegisterModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // שדות הטופס - Attribute ה-BindProperty מאפשר לקבל את הקלטים מה-HTML באופן אוטומטי
        [BindProperty]
        public string FullName { get; set; } = string.Empty;

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        // משתנה להצגת הודעות שגיאה למשתמש במידת הצורך
        public string ErrorMessage { get; set; } = string.Empty;

        // מתודה הפועלת בעת טעינת הדף (בקשת GET)
        public void OnGet()
        {
        }

        // מתודה הפועלת בעת שליחת הטופס (בקשת POST)
        public IActionResult OnPost()
        {
            // בדיקה האם הנתונים שהוזנו בטופס תואמים את חוקי ה-Validation
            if (!ModelState.IsValid)
            {
                return Page(); // במידה ולא, נשארים באותו הדף
            }

            try
            {
                // 1. שליפת מחרוזת החיבור (Connection String) מתוך קובץ appsettings.json
                string connectionString = _configuration.GetConnectionString("DefaultConnection")!;

                // 2. הצפנת הסיסמה שהקליד המשתמש באמצעות פונקציית SHA-256
                string hashedPassword = HashPassword(Password);

                // 3. יצירת חיבור לשרת PostgreSQL בעזרת NpgsqlConnection
                // שימוש ב-using מבטיח לסגור את החיבור ולשחרר משאבים בסיום הבלוק
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    // פתיחת החיבור אקטיבית מול מסד הנתונים
                    conn.Open();

                    // שאילתת SQL מאובטחת המשתמשת בפרמטרים למניעת SQL Injection
                    string sql = "INSERT INTO users (full_name, username, email, password_hash) VALUES (@fullName, @username, @email, @password)";

                    // יצירת אובייקט הפקודה להרצה על ה-Connection
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        // השמת הערכים הפרמטריים מתוך השדות בטופס
                        cmd.Parameters.AddWithValue("@fullName", FullName);
                        cmd.Parameters.AddWithValue("@username", Username);
                        cmd.Parameters.AddWithValue("@email", Email);
                        cmd.Parameters.AddWithValue("@password", hashedPassword); // שמירת הסיסמה המוצפנת בלבד

                        // הרצת הפקודה מול מסד הנתונים (אינה מחזירה שורות)
                        cmd.ExecuteNonQuery();
                    }
                }

                // לאחר רישום מוצלח, מעבירים את המשתמש לדף ההתחברות (Index)
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                // תפיסת שגיאות בזמן ריצה (כגון אימייל כפול או בעיית תקשורת מול השרת)
                ErrorMessage = "אירעה שגיאה בעת ההרשמה: " + ex.Message;
                return Page(); // החזרת הדף עם הודעת השגיאה
            }
        }

        /// <summary>
        /// פונקציית עזר פרטית להצפנת מחרוזת באמצעות אלגוריתם SHA-256.
        /// מבוססת על ספריות מובנות בלבד ללא צורך בהורדת חבילות נוספות.
        /// </summary>
        private static string HashPassword(string rawData)
        {
            // יצירת מופע של מנגנון ה-SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // המרת מחרוזת הטקסט המקורית ערך בייטים והרצת מנגנון ה-Hash
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // בניית המחרוזת ההקסדצימלית המוצפנת
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    // "x2" ממיר כל בייט למחרוזת בבסיס 16 באורך 2 תווים
                    builder.Append(bytes[i].ToString("x2"));
                }

                // החזרת המחרוזת המוצפנת הסופית (באורך 64 תווים)
                return builder.ToString();
            }
        }
    }
}
