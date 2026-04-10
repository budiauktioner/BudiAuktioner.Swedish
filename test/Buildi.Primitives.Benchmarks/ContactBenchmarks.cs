using BenchmarkDotNet.Attributes;
using Buildi.Primitives.Contact;
using Buildi.Primitives.Web;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class ContactBenchmarks
{
    private const string ValidEmail = "user@gmail.com";
    private const string InvalidEmail = "not-an-email";

    private const string ValidPhoneSwedish = "070-174 06 33";
    private const string ValidPhoneInternational = "+44 7911 123456";
    private const string InvalidPhone = "abc";

    private const string ValidCountryCode = "SE";
    private const string ValidCountryName = "Sverige";
    private const string InvalidCountry = "XX";

    private const string ValidZipCodeSwedish = "114 36";
    private const string InvalidZipCode = "0";

    private const string ValidCity = "Stockholm";
    private const string InvalidCity = "";

    private const string ValidStreet = "Storgatan 1";
    private const string InvalidStreet = "";

    [Benchmark] public bool Email_TryParse_Valid() => EmailAddress.TryParse(ValidEmail, out _);
    [Benchmark] public bool Email_TryParse_Invalid() => EmailAddress.TryParse(InvalidEmail, out _);

    [Benchmark] public bool Phone_TryParse_Swedish() => PhoneNumber.TryParse(ValidPhoneSwedish, out _);
    [Benchmark] public bool Phone_TryParse_International() => PhoneNumber.TryParse(ValidPhoneInternational, out _);
    [Benchmark] public bool Phone_TryParse_Invalid() => PhoneNumber.TryParse(InvalidPhone, out _);

    [Benchmark] public bool Country_TryParse_Code() => Country.TryParse(ValidCountryCode, out _);
    [Benchmark] public bool Country_TryParse_Name() => Country.TryParse(ValidCountryName, out _);
    [Benchmark] public bool Country_TryParse_Invalid() => Country.TryParse(InvalidCountry, out _);

    [Benchmark] public bool ZipCode_TryParse_Valid() => AddressZipCode.TryParse(ValidZipCodeSwedish, out _);
    [Benchmark] public bool ZipCode_TryParse_Invalid() => AddressZipCode.TryParse(InvalidZipCode, out _);

    [Benchmark] public bool City_TryParse_Valid() => AddressCity.TryParse(ValidCity, out _);
    [Benchmark] public bool City_TryParse_Invalid() => AddressCity.TryParse(InvalidCity, out _);

    [Benchmark] public bool Street_TryParse_Valid() => AddressStreet.TryParse(ValidStreet, out _);
    [Benchmark] public bool Street_TryParse_Invalid() => AddressStreet.TryParse(InvalidStreet, out _);

    [Benchmark] public bool Address_TryParse_Valid() => Address.TryParse("Storgatan 1, 114 36 Stockholm, Sverige", out _);
    [Benchmark] public bool Address_TryParse_Invalid() => Address.TryParse("", out _);
}
