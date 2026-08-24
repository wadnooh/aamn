namespace SudanTravelApp.API.Services;

public static class AirportCatalog
{
    private static readonly Dictionary<string, (string Code, string CityAr, string CityEn)> ByCode =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Sudan & region
            ["KRT"] = ("KRT", "الخرطوم", "Khartoum"),
            ["PZU"] = ("PZU", "بورتسودان", "Port Sudan"),
            ["UYL"] = ("UYL", "نيالا", "Nyala"),
            ["EBD"] = ("EBD", "الأبيض", "El Obeid"),
            ["ELF"] = ("ELF", "الفاشر", "El Fasher"),
            ["KSL"] = ("KSL", "كسلا", "Kassala"),
            ["DOG"] = ("DOG", "دنقلا", "Dongola"),
            ["JUB"] = ("JUB", "جوبا", "Juba"),
            ["GSU"] = ("GSU", "القضارف", "Gedaref"),
            ["WHF"] = ("WHF", "وادي حلفا", "Wadi Halfa"),
            // Middle East & North Africa
            ["CAI"] = ("CAI", "القاهرة", "Cairo"),
            ["ASW"] = ("ASW", "أسوان", "Aswan"),
            ["JED"] = ("JED", "جدة", "Jeddah"),
            ["RUH"] = ("RUH", "الرياض", "Riyadh"),
            ["MED"] = ("MED", "المدينة المنورة", "Madinah"),
            ["DXB"] = ("DXB", "دبي", "Dubai"),
            ["AUH"] = ("AUH", "أبوظبي", "Abu Dhabi"),
            ["DOH"] = ("DOH", "الدوحة", "Doha"),
            ["BAH"] = ("BAH", "المنامة", "Bahrain"),
            ["KWI"] = ("KWI", "الكويت", "Kuwait"),
            ["MCT"] = ("MCT", "مسقط", "Muscat"),
            ["AMM"] = ("AMM", "عمّان", "Amman"),
            ["BEY"] = ("BEY", "بيروت", "Beirut"),
            ["IST"] = ("IST", "إسطنبول", "Istanbul"),
            ["CMN"] = ("CMN", "الدار البيضاء", "Casablanca"),
            ["TUN"] = ("TUN", "تونس", "Tunis"),
            ["ALG"] = ("ALG", "الجزائر", "Algiers"),
            // Africa
            ["ADD"] = ("ADD", "أديس أبابا", "Addis Ababa"),
            ["NBO"] = ("NBO", "نيروبي", "Nairobi"),
            ["EBB"] = ("EBB", "عنتيبي", "Entebbe"),
            ["JNB"] = ("JNB", "جوهانسبرغ", "Johannesburg"),
            ["CPT"] = ("CPT", "كيب تاون", "Cape Town"),
            ["LOS"] = ("LOS", "لاغوس", "Lagos"),
            ["ACC"] = ("ACC", "أكرا", "Accra"),
            // Europe
            ["LHR"] = ("LHR", "لندن", "London"),
            ["LGW"] = ("LGW", "لندن جاتويك", "London Gatwick"),
            ["CDG"] = ("CDG", "باريس", "Paris"),
            ["FRA"] = ("FRA", "فرانكفورت", "Frankfurt"),
            ["AMS"] = ("AMS", "أمستردام", "Amsterdam"),
            ["MAD"] = ("MAD", "مدريد", "Madrid"),
            ["FCO"] = ("FCO", "روما", "Rome"),
            ["MXP"] = ("MXP", "ميلانو", "Milan"),
            ["MUC"] = ("MUC", "ميونخ", "Munich"),
            ["ZRH"] = ("ZRH", "زيورخ", "Zurich"),
            ["VIE"] = ("VIE", "فيينا", "Vienna"),
            // Americas
            ["JFK"] = ("JFK", "نيويورك", "New York"),
            ["EWR"] = ("EWR", "نيوارك", "Newark"),
            ["IAD"] = ("IAD", "واشنطن", "Washington"),
            ["ORD"] = ("ORD", "شيكاغو", "Chicago"),
            ["LAX"] = ("LAX", "لوس أنجلوس", "Los Angeles"),
            ["YYZ"] = ("YYZ", "تورونتو", "Toronto"),
            ["GRU"] = ("GRU", "ساو باولو", "Sao Paulo"),
            // Asia-Pacific
            ["BOM"] = ("BOM", "مومباي", "Mumbai"),
            ["DEL"] = ("DEL", "دلهي", "Delhi"),
            ["BKK"] = ("BKK", "بانكوك", "Bangkok"),
            ["KUL"] = ("KUL", "كوالالمبور", "Kuala Lumpur"),
            ["SIN"] = ("SIN", "سنغافورة", "Singapore"),
            ["HKG"] = ("HKG", "هونغ كونغ", "Hong Kong"),
            ["NRT"] = ("NRT", "طوكيو", "Tokyo"),
            ["ICN"] = ("ICN", "سيول", "Seoul"),
            ["SYD"] = ("SYD", "سيدني", "Sydney"),
            ["MEL"] = ("MEL", "ملبورن", "Melbourne")
        };

    private static readonly Dictionary<string, string> Aliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["الخرطوم"] = "KRT", ["خرطوم"] = "KRT", ["khartoum"] = "KRT",
            ["بورتسودان"] = "PZU", ["port sudan"] = "PZU", ["portsudan"] = "PZU",
            ["نيالا"] = "UYL", ["nyala"] = "UYL",
            ["الأبيض"] = "EBD", ["الابيض"] = "EBD", ["el obeid"] = "EBD",
            ["الفاشر"] = "ELF", ["el fasher"] = "ELF",
            ["كسلا"] = "KSL", ["kassala"] = "KSL",
            ["دنقلا"] = "DOG", ["dongola"] = "DOG",
            ["جوبا"] = "JUB", ["juba"] = "JUB",
            ["القضارف"] = "GSU", ["gedaref"] = "GSU",
            ["وادي حلفا"] = "WHF", ["wadi halfa"] = "WHF",
            ["القاهرة"] = "CAI", ["cairo"] = "CAI",
            ["أسوان"] = "ASW", ["اسوان"] = "ASW", ["aswan"] = "ASW",
            ["جدة"] = "JED", ["جده"] = "JED", ["jeddah"] = "JED",
            ["الرياض"] = "RUH", ["riyadh"] = "RUH",
            ["المدينة"] = "MED", ["madinah"] = "MED", ["medina"] = "MED",
            ["دبي"] = "DXB", ["dubai"] = "DXB",
            ["أبوظبي"] = "AUH", ["ابوظبي"] = "AUH", ["abu dhabi"] = "AUH",
            ["الدوحة"] = "DOH", ["doha"] = "DOH",
            ["الكويت"] = "KWI", ["kuwait"] = "KWI",
            ["البحرين"] = "BAH", ["bahrain"] = "BAH", ["المنامة"] = "BAH",
            ["مسقط"] = "MCT", ["muscat"] = "MCT",
            ["عمّان"] = "AMM", ["عمان"] = "AMM", ["amman"] = "AMM",
            ["بيروت"] = "BEY", ["beirut"] = "BEY",
            ["إسطنبول"] = "IST", ["اسطنبول"] = "IST", ["istanbul"] = "IST",
            ["أديس أبابا"] = "ADD", ["اديس ابابا"] = "ADD", ["addis"] = "ADD", ["addis ababa"] = "ADD",
            ["نيروبي"] = "NBO", ["nairobi"] = "NBO",
            ["عنتيبي"] = "EBB", ["entebbe"] = "EBB",
            ["جوهانسبرغ"] = "JNB", ["johannesburg"] = "JNB",
            ["كيب تاون"] = "CPT", ["cape town"] = "CPT",
            ["لاغوس"] = "LOS", ["lagos"] = "LOS",
            ["أكرا"] = "ACC", ["accra"] = "ACC",
            ["الدار البيضاء"] = "CMN", ["casablanca"] = "CMN",
            ["تونس"] = "TUN", ["tunis"] = "TUN",
            ["الجزائر"] = "ALG", ["algiers"] = "ALG",
            ["لندن"] = "LHR", ["london"] = "LHR",
            ["باريس"] = "CDG", ["paris"] = "CDG",
            ["فرانكفورت"] = "FRA", ["frankfurt"] = "FRA",
            ["أمستردام"] = "AMS", ["amsterdam"] = "AMS",
            ["مدريد"] = "MAD", ["madrid"] = "MAD",
            ["روما"] = "FCO", ["rome"] = "FCO",
            ["ميلانو"] = "MXP", ["milan"] = "MXP",
            ["ميونخ"] = "MUC", ["munich"] = "MUC",
            ["زيورخ"] = "ZRH", ["zurich"] = "ZRH",
            ["فيينا"] = "VIE", ["vienna"] = "VIE",
            ["نيويورك"] = "JFK", ["new york"] = "JFK", ["nyc"] = "JFK",
            ["واشنطن"] = "IAD", ["washington"] = "IAD",
            ["شيكاغو"] = "ORD", ["chicago"] = "ORD",
            ["لوس أنجلوس"] = "LAX", ["los angeles"] = "LAX",
            ["تورونتو"] = "YYZ", ["toronto"] = "YYZ",
            ["مومباي"] = "BOM", ["mumbai"] = "BOM",
            ["دلهي"] = "DEL", ["delhi"] = "DEL",
            ["بانكوك"] = "BKK", ["bangkok"] = "BKK",
            ["كوالالمبور"] = "KUL", ["kuala lumpur"] = "KUL",
            ["سنغافورة"] = "SIN", ["singapore"] = "SIN",
            ["هونغ كونغ"] = "HKG", ["hong kong"] = "HKG",
            ["طوكيو"] = "NRT", ["tokyo"] = "NRT",
            ["سيول"] = "ICN", ["seoul"] = "ICN",
            ["سيدني"] = "SYD", ["sydney"] = "SYD",
            ["ملبورن"] = "MEL", ["melbourne"] = "MEL"
        };

    public static bool TryResolve(string? input, out string code, out string cityAr)
    {
        code = string.Empty;
        cityAr = string.Empty;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var key = input.Trim();
        if (ByCode.TryGetValue(key, out var byCode))
        {
            code = byCode.Code;
            cityAr = byCode.CityAr;
            return true;
        }

        if (Aliases.TryGetValue(key, out var aliasCode) && ByCode.TryGetValue(aliasCode, out var mapped))
        {
            code = mapped.Code;
            cityAr = mapped.CityAr;
            return true;
        }

        foreach (var alias in Aliases)
        {
            if (key.Contains(alias.Key, StringComparison.OrdinalIgnoreCase) ||
                alias.Key.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                if (ByCode.TryGetValue(alias.Value, out var soft))
                {
                    code = soft.Code;
                    cityAr = soft.CityAr;
                    return true;
                }
            }
        }

        return false;
    }

    public static string CityName(string codeOrCity, bool english = false)
    {
        if (!TryResolve(codeOrCity, out var code, out var cityAr))
            return codeOrCity;
        if (!english) return cityAr;
        return ByCode.TryGetValue(code, out var row) ? row.CityEn : cityAr;
    }

    public static string CodeOrEmpty(string? input)
    {
        return TryResolve(input, out var code, out _) ? code : string.Empty;
    }

    public static IReadOnlyList<(string Code, string CityAr, string CityEn)> GlobalHubs() =>
    [
        ("DXB", "دبي", "Dubai"),
        ("CAI", "القاهرة", "Cairo"),
        ("JED", "جدة", "Jeddah"),
        ("IST", "إسطنبول", "Istanbul"),
        ("DOH", "الدوحة", "Doha"),
        ("LHR", "لندن", "London"),
        ("CDG", "باريس", "Paris"),
        ("FRA", "فرانكفورت", "Frankfurt"),
        ("JFK", "نيويورك", "New York"),
        ("ADD", "أديس أبابا", "Addis Ababa"),
        ("NBO", "نيروبي", "Nairobi"),
        ("JNB", "جوهانسبرغ", "Johannesburg")
    ];
}
