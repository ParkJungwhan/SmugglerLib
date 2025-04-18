using System.Globalization;

namespace SmugCommon.Utils
{
    public class fGlobalization
    {
        /// <summary>
        ///  ISO 639-3값으로 CultureInfo를 가져옴.
        /// </summary>
        /// <param name="name">ISO 639-3</param>
        /// <returns></returns>
        public static CultureInfo FromISO639Code(string isoCountryName)
        {
            return CultureInfo
                .GetCultures(CultureTypes.NeutralCultures)
                .FirstOrDefault(c => c.ThreeLetterISOLanguageName == isoCountryName.Trim().ToLower());
        }

        /// <summary>
        /// ISO 3166에 정의되어 있는 세 문자로 된 코드로 가져옴.
        /// </summary>
        /// <param name="isoCountryCode"></param>
        /// <returns></returns>
        public static RegionInfo FromISO3166Code(string isoCountryCode)
        {
            var cultureList = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            var regionList = cultureList.Select(x => new RegionInfo(x.LCID)).ToList();
            var regionInfo = regionList.FirstOrDefault(x => x.ThreeLetterISORegionName.Equals(isoCountryCode.Trim().ToUpper()));

            return regionInfo;
        }
    }
}