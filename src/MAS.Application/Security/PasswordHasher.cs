namespace MAS.Application.Security;

/// <summary>
/// تشفير كلمات المرور والتحقق منها باستخدام BCrypt.
///
/// النظام كان بيخزّن الباسورد نص صريح (Plain Text) في عمود PasswordHash،
/// وده خطر حقيقي لو الموقع اترفع على النت. الكلاس ده بيشفّر الباسوردات الجديدة،
/// وفي نفس الوقت بيفضل يقبل الباسوردات القديمة المخزّنة نص صريح عشان
/// المستخدمين اللي موجودين في الداتابيز دلوقتي ميتقفلش عليهم الدخول.
/// أول ما المستخدم يدخل بباسورد قديم، بيتحوّل تلقائياً لـ hash مشفّر.
/// </summary>
public static class PasswordHasher
{
    /// <summary>يرجّع hash مشفّر للباسورد المدخل.</summary>
    public static string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password);

    /// <summary>
    /// هل القيمة المخزّنة دي hash بتاع BCrypt أصلاً؟
    /// (الـ hash بيبدأ بـ ‎$2a$‎ / ‎$2b$‎ / ‎$2y$‎ وطوله 60 حرف)
    /// </summary>
    public static bool IsHashed(string? stored) =>
        !string.IsNullOrEmpty(stored)
        && stored.Length == 60
        && stored.StartsWith("$2")
        && stored[3] == '$';

    /// <summary>
    /// يتحقق من الباسورد. بيشتغل مع الـ hash المشفّر ومع الباسوردات القديمة النص الصريح.
    /// </summary>
    public static bool Verify(string password, string? stored)
    {
        if (string.IsNullOrEmpty(stored)) return false;

        if (IsHashed(stored))
        {
            try { return BCrypt.Net.BCrypt.Verify(password, stored); }
            catch { return false; }
        }

        // باسورد قديم متخزّن نص صريح
        return password == stored;
    }

    /// <summary>
    /// لو القيمة المخزّنة لسه نص صريح، بيرجّع hash جديد عشان نحدّثه في الداتابيز.
    /// لو هي متشفّرة أصلاً بيرجّع null (يعني مفيش داعي لتحديث).
    /// </summary>
    public static string? UpgradeIfNeeded(string password, string? stored) =>
        IsHashed(stored) ? null : Hash(password);
}
