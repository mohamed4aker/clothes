namespace MAS.Application.Common;

/// <summary>
/// توليد أرقام المستندات (فواتير، مرتجعات، أوامر إنتاج... إلخ)
/// بالشكل: PREFIX-yyyyMM-0001
///
/// الكود القديم كان بيستخدم int.Parse على الجزء الرقمي على طول،
/// وده بيرمي Exception ويوقّع الصفحة لو الرقم الأخير مكانش رقم صافي
/// (مثلاً رقم اتكتب بالإيد، أو مستند اتعمله تعديل). هنا بنستخدم TryParse
/// وبنرجع لأول رقم بدل ما نكسر العملية.
/// </summary>
public static class DocumentNumber
{
    /// <summary>
    /// يرجّع الرقم التالي بناءً على آخر رقم متسجّل.
    /// </summary>
    /// <param name="prefix">البادئة كاملة، مثال: "INV-202608-"</param>
    /// <param name="lastNumber">آخر رقم متسجّل (ممكن يكون null)</param>
    /// <param name="digits">عدد خانات الجزء الرقمي (3 أو 5 حسب المستند)</param>
    public static string Next(string prefix, string? lastNumber, int digits)
    {
        var first = 1.ToString(new string('0', digits));

        if (string.IsNullOrEmpty(lastNumber) || !lastNumber.StartsWith(prefix))
            return prefix + first;

        var suffix = lastNumber.Substring(prefix.Length);
        if (!int.TryParse(suffix, out var parsed))
            return prefix + first;

        return prefix + (parsed + 1).ToString(new string('0', digits));
    }
}
