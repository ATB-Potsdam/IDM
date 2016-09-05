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
    internal sealed class LoginCookieCache
    {
        private static readonly LoginCookieCache instance = new LoginCookieCache();
        private static CookieContainer cookieContainer = new CookieContainer();

        public CookieContainer getCookieContainer()
        {
            return cookieContainer;
        }

        public static LoginCookieCache Instance
        {
            get
            {
                return instance;
            }
        }
    }


    internal static class WebApiRequest
    {
        private const String SpongeJsAuthUrl = "https://agrohyd-api-dev.runlevel3.de/auth";
        private const String SpongeJsUrlDataLocation = "https://agrohyd-api-dev.runlevel3.de/ParameterSetData/getByTagTypeLocation/{0}/climate/{1}/{2}/date%3A{3}?end=date%3A{4}&step=date%3A{5}&format=csv";
        private const String SpongeJsUrlDataId = "https://agrohyd-api-dev.runlevel3.de/ParameterSetData/getByDataObjId/{0}/date%3A{1}?end=date%3A{2}&step=date%3A{3}&format=csv";
        private const String SpongeJsUrlAllIds = "https://agrohyd-api-dev.runlevel3.de/DataObjRaw/getIdNamesByType/climate?tag={0}&format=csv";
        private const String SpongeJsUrlAltitude = "https://agrohyd-api-dev.runlevel3.de/Tools/getAltitude/{0}/{1}";
        private const String SpongeJsUser = "irri_mod_user";
        private const String SpongeJsPass = "irri_mod_pass";
        private const string DefaultTag = "public_data";


        internal static async Task<Stream> LoadFromATBWebService(Location location, DateTime start, DateTime end, TimeStep step)
        {
            return await LoadFromATBWebService(location, DefaultTag, SpongeJsUser, SpongeJsPass, start, end, step);
        }

        internal static async Task<Stream> LoadFromATBWebService(Location location, String tag, DateTime start, DateTime end, TimeStep step)
        {
            return await LoadFromATBWebService(location, tag, SpongeJsUser, SpongeJsPass, start, end, step);
        }
        
        internal static async Task<Stream> LoadFromATBWebService(Location location, String tag, String user, String pass, DateTime start, DateTime end, TimeStep step)
        {
            try
            {
                String url = String.Format(
                    SpongeJsUrlDataLocation,
                    tag,
                    location.lon.ToString(CultureInfo.InvariantCulture),
                    location.lat.ToString(CultureInfo.InvariantCulture),
                    start.ToUniversalTime().ToString("s") + "Z",
                    end.ToUniversalTime().ToString("s") + "Z",
                    step.ToString()
                );
                return await WebApiRequest.ExecuteWebApiRequest(url, user, pass);
            }
            catch
            {
                return null;
            }
        }

        /*!
         * Loads data by dataObjId from the ATB web service.
         *
         * \author  Hunstock
         * \date    05.09.2016
         *
         * \param   dataObjId   Identifier for the data object.
         * \param   start       The start Date/Time.
         * \param   end         The end Date/Time.
         * \param   step        Amount to increment by.
         *
         * \return  The data that was read from a terabytes web service.
         */

        internal static async Task<Stream> LoadFromATBWebService(String dataObjId, DateTime start, DateTime end, TimeStep step)
        {
            return await LoadFromATBWebService(dataObjId, SpongeJsUser, SpongeJsPass, start, end, step);
        }

        internal static async Task<Stream> LoadFromATBWebService(String dataObjId, String user, String pass, DateTime start, DateTime end, TimeStep step)
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
                return await WebApiRequest.ExecuteWebApiRequest(url, user, pass);
            }
            catch
            {
                return null;
            }
        }

        internal static async Task<Stream> LoadFromATBWebService()
        {
            return await LoadFromATBWebService(DefaultTag, SpongeJsUser, SpongeJsPass);
        }

        internal static async Task<Stream> LoadFromATBWebService(String tag, String user, String pass)
        {
            try
            {
                String url = String.Format(
                    SpongeJsUrlAllIds,
                    tag
                );
                return await WebApiRequest.ExecuteWebApiRequest(url, user, pass);
            }
            catch
            {
                return null;
            }
        }


        internal static async Task<double> LoadAltitudeFromATBWebService(Location location)
        {
            return await LoadAltitudeFromATBWebService(location, SpongeJsUser, SpongeJsPass);
        }

        internal static async Task<double> LoadAltitudeFromATBWebService(Location location, String user, String pass)
        {
            try
            {
                String url = String.Format(
                    SpongeJsUrlAltitude,
                    location.lon.ToString(CultureInfo.InvariantCulture),
                    location.lat.ToString(CultureInfo.InvariantCulture)
                );
                Stream webResponse = await ExecuteWebApiRequest(url, user, pass);
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


        internal static async Task<Stream> ExecuteWebApiRequest(String url, String user, String pass)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Credentials = new NetworkCredential(user, pass);
                req.CookieContainer = LoginCookieCache.Instance.getCookieContainer();
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
