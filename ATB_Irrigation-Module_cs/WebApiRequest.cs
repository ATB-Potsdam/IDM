/*!
 * \file    WebApiRequest.cs
 *
 * \brief   Static class with functions to load data from Sponge-JS webservice.
 *          Implemented for testing and validating purpose.
 *          The webservice is maintained by ATB, username and password are required
 *
 * \author  Hunstock
 * \date    19.08.2015
 */


using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

using atbApi;
using atbApi.data;

namespace local
{
    internal static class LoginCookieCache
    {
        private static CookieContainer cookieContainer = new CookieContainer();

        internal static CookieContainer getCookieContainer()
        {
            return cookieContainer;
        }
    }

    internal static class WebApiRequest
    {
        private const String SpongeJsAuthUrl = "https://agrohyd-api-dev.runlevel3.de/auth";
        private const String SpongeJsUrlDataLocation = "https://agrohyd-api-dev.runlevel3.de/ParameterSetData/getByTagTypeLocation/{0}/climate/{1}/{2}/date%3A{3}?end=date%3A{4}&step=date%3A{5}&format=csv";
        private const String SpongeJsUrlDataId = "https://agrohyd-api-dev.runlevel3.de/ParameterSetData/getByDataObjId/{0}/date%3A{1}?end=date%3A{2}&step=date%3A{3}&format=csv";
        private const String SpongeJsUrlBaseDataId = "https://agrohyd-api-dev.runlevel3.de/DataObj/getBaseData/{0}";
        private const String SpongeJsUrlAllIds = "https://agrohyd-api-dev.runlevel3.de/DataObjRaw/getIdNamesByType/climate?tag={0}&format=csv";
        private const String SpongeJsUrlAltitude = "https://agrohyd-api-dev.runlevel3.de/Tools/getAltitude/{0}/{1}";
        private static String SpongeJsUser = "irri_mod_user";
        private static String SpongeJsPass = "irri_mod_pass";
        private const string DefaultTag = "public_data";

        internal static void SetUserPass(String user, String pass)
        {
            SpongeJsUser = user;
            SpongeJsPass = pass;
        }

        internal static async Task<Stream> LoadClimateByLocationTagFromATBWebService(Location location, String tag, DateTime start, DateTime end, TimeStep step)
        {
            try
            {
                String url = String.Format(
                    SpongeJsUrlDataLocation,
                    String.IsNullOrEmpty(tag) ? DefaultTag : tag,
                    location.lon.ToString(CultureInfo.InvariantCulture),
                    location.lat.ToString(CultureInfo.InvariantCulture),
                    start.ToUniversalTime().ToString("s") + "Z",
                    end.ToUniversalTime().ToString("s") + "Z",
                    step.ToString()
                );
                return await WebApiRequest.ExecuteWebApiRequest(url);
            }
            catch
            {
                return null;
            }
        }


        internal static async Task<Stream> LoadClimateByIdFromATBWebService(String dataObjId, DateTime start, DateTime end, TimeStep step)
        {
            try
            {
                String url = String.Format(
                    SpongeJsUrlDataId,
                    dataObjId,
                    start.ToUniversalTime().ToString("s") + "Z",
                    end.ToUniversalTime().ToString("s") + "Z",
                    step.ToString()
                );
                return await WebApiRequest.ExecuteWebApiRequest(url);
            }
            catch
            {
                return null;
            }
        }

        internal static async Task<Stream> LoadClimateIdsFromATBWebService(String tag)
        {
            try
            {
                String url = String.Format(
                    SpongeJsUrlAllIds,
                    String.IsNullOrEmpty(tag) ? DefaultTag : tag
                );
                return await WebApiRequest.ExecuteWebApiRequest(url);
            }
            catch
            {
                return null;
            }
        }

        internal static async Task<Double> LoadBaseDataByIdFromATBWebService(String dataObjId)
        {
            try
            {
                String url = String.Format(
                    SpongeJsUrlBaseDataId,
                    dataObjId
                );
                Stream webResponse = await ExecuteWebApiRequest(url);
                StreamReader altReader = new StreamReader(webResponse);
                String altResult = altReader.ReadToEnd();
                if (!altResult.Contains("result")) return 0;
                altResult = altResult.Split(':')[1].Replace('}', ' ').Trim();
                return Int32.Parse(altResult);
            }
            catch
            {
                return 0;
            }
        }


        internal static async Task<double> LoadAltitudeFromATBWebService(Location location)
        {
            try
            {
                String url = String.Format(
                    SpongeJsUrlAltitude,
                    location.lon.ToString(CultureInfo.InvariantCulture),
                    location.lat.ToString(CultureInfo.InvariantCulture)
                );
                Stream webResponse = await ExecuteWebApiRequest(url);
                StreamReader altReader = new StreamReader(webResponse);
                String altResult = altReader.ReadToEnd();
                if (!altResult.Contains("result")) return 0;
                altResult = altResult.Split(':')[1].Replace('}', ' ').Trim();
                return Int32.Parse(altResult);
            }
            catch
            {
                return 0;
            }
        }


        internal static async Task<Stream> ExecuteWebApiRequest(String url)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Credentials = new NetworkCredential(SpongeJsUser, SpongeJsPass);
                req.CookieContainer = LoginCookieCache.getCookieContainer();
                WebResponse res = await Task<WebResponse>.Factory.FromAsync(req.BeginGetResponse, req.EndGetResponse, req);
                return res.GetResponseStream();
            }
            catch
            {
                return null;
            }
        }
    }
}
