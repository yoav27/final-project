using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace final_project.Pages
{
    // מחלקת המודל עבור עמוד איפוס הסיסמה
    public class Forget_PasswordModel : PageModel
    {
        // שדה אימייל לקבלת הכתובת של המשתמש
        [BindProperty]
        public string Email { get; set; }
        // שדה הסיסמה החדשה - מחובר לשדה ב-HTML
        [BindProperty]
        public string NewPassword { get; set; }

        // שדה אישור הסיסמה החדשה - מחובר לשדה ב-HTML
        [BindProperty]
        public string ConfirmPassword { get; set; }

        // משתנה להחזרת הודעת שגיאה למשתמש במידה והאיפוס נכשל
        public string ErrorMessage { get; set; }

        // פונקציה הפועלת בעת טעינת העמוד (בקשת GET)
        public void OnGet()
        {
        }

        // פונקציה הפועלת בלחיצה על כפתור השליחה (בקשת POST)
        public IActionResult OnPost()
        {
            // בדיקה האם כל השדות הנדרשים מולאו כראוי
            if (!ModelState.IsValid)
            {
                return Page(); // החזרת העמוד עם השגיאות הקיימות
            }

            // בדיקת התאמה בין הסיסמה החדשה לאישור הסיסמה
            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match."; // הגדרת הודעת שגיאה
                return Page(); // טעינת העמוד מחדש להצגת ההודעה
            }

            // לכאן תתווסף בעתיד הלוגיקה של עדכון הסיסמה במסד הנתונים (DB)

            // הפניית המשתמש חזרה לעמוד ההתחברות לאחר הצלחה
            return RedirectToPage("/Index");
        }
    }
}
