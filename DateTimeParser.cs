namespace JwtDecoder;

public static class DateTimeParser
{
    /// <summary>
    /// Пытается разобрать строку, содержащую временную метку JWT (число секунд с 1 января 1970 UTC),
    /// и вернуть соответствующий DateTime (UTC).
    /// </summary>
    /// <param name="token">Строка с числовым значением временной метки</param>
    /// <param name="dateTime">Результирующий DateTime (UTC), если парсинг успешен, иначе null</param>
    /// <returns>true, если парсинг прошёл успешно, иначе false</returns>
    public static bool TryParse(string token, out DateTime? dateTime)
    {
        dateTime = null;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        // Удаляем лишние пробелы и невидимые символы
        token = token.Trim();

        // Пытаемся разобрать строку как целое число (количество секунд)
        if (long.TryParse(token, out long seconds))
        {
            try
            {
                // Преобразуем секунды в DateTime (UTC)
                dateTime = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }
}