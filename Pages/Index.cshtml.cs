using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace final_project.Pages
{
    public class IndexModel : PageModel
    {
        // מקשר את שדה ה-Email מה-HTML ל-C#
        [BindProperty]
        public string Email { get; set; }

        // מקשר את שדה ה-Password מה-HTML ל-C#
        [BindProperty]
        public string Password { get; set; }

        // משתנה להצגת הודעת שגיאה במידה וההתחברות נכשלה
        public string ErrorMessage { get; set; }

            public void OnGet()
            {
            }

        public IActionResult OnPost()
        {
            // בדיקה של האימייל והסיסמה
            if (Email == "admin@gmail.com" && Password == "123456")
            {
                // התחברות בהצלחה - מעביר לעמוד Privacy
                return RedirectToPage("/Privacy");
            }

                // במידה והפרטים שגויים
            ErrorMessage = "אימייל או סיסמה לא נכונים";
            return Page();
        }
    }
    
}

