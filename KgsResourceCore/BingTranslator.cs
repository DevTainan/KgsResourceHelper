using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace KgsResourceCore
{
    public class BingTranslator
    {
        AdmAccessToken admToken;
        string headerValue;

        public BingTranslator()
        {
            //Get Client Id and Client Secret from https://datamarket.azure.com/developer/applications/
            //Refer obtaining AccessToken (http://msdn.microsoft.com/en-us/library/hh454950.aspx) 
            AdmAuthentication admAuth = new AdmAuthentication("KgsResourceHelper", "QUFdjQbYu17Lg4Aw/bTDvih2dIbnW/Heyz9b2m0GbZc=");
            try
            {
                admToken = admAuth.GetAccessToken();
                // Create a header with the access_token property of the returned token
                headerValue = "Bearer " + admToken.access_token;
                //TranslateMethod(headerValue);
            }
            catch (WebException e)
            {
                string exMsg = ProcessWebException(e);
                throw new WebException(exMsg, e);
            }
            catch (Exception ex)
            {
                throw new Exception("BingTranslator has Error!", ex);
            }
        }

        //private static void TranslateMethod(string authToken)
        public string TranslateMethod(string translateText, LanguageEnum fromLanguage, LanguageEnum toLanguage)
        {
            string text = translateText;
            string from = ProcessLanguage(fromLanguage); // zh-CHT   zh-CHS  en  de
            string to = ProcessLanguage(toLanguage);

            string uri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + System.Web.HttpUtility.UrlEncode(text) + "&from=" + from + "&to=" + to;

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            //httpWebRequest.Headers.Add("Authorization", authToken);
            httpWebRequest.Headers.Add("Authorization", headerValue);
            WebResponse response = null;
            try
            {
                response = httpWebRequest.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    System.Runtime.Serialization.DataContractSerializer dcs = new System.Runtime.Serialization.DataContractSerializer(Type.GetType("System.String"));
                    string translation = (string)dcs.ReadObject(stream);
                    return translation;

                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }

        private string ProcessLanguage(LanguageEnum language)
        {
            string languageStr = string.Empty;
            if (language == LanguageEnum.zh_CHS)
            {
                languageStr = "zh-CHS";
            }
            else if(language == LanguageEnum.zh_CHT)
            {
                languageStr = "zh-CHT";
            }
            else if (language == LanguageEnum.tlh_Qaak)
            {
                languageStr = "tlh-Qaak";
            }
            else
            {
                languageStr = Enum.GetName(typeof(LanguageEnum), language);
            }
            return languageStr;
        }

        //private static void ProcessWebException(WebException e)
        private string ProcessWebException(WebException e)
        {
            string exMsg = string.Format("{0}", e.ToString());

            // Obtain detailed error information
            string strResponse = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)e.Response)
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(responseStream, System.Text.Encoding.ASCII))
                    {
                        strResponse = sr.ReadToEnd();
                    }
                }
            }
            return exMsg + Environment.NewLine + string.Format("Http status code={0}, error message={1}", e.Status, strResponse);
        }

        [DataContract]
        public class AdmAccessToken
        {
            [DataMember]
            public string access_token { get; set; }
            [DataMember]
            public string token_type { get; set; }
            [DataMember]
            public string expires_in { get; set; }
            [DataMember]
            public string scope { get; set; }
        }

        //public class AdmAuthentication
        protected internal class AdmAuthentication
        {
            public static readonly string DatamarketAccessUri = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
            private string clientId;
            private string cientSecret;
            private string request;

            public AdmAuthentication(string clientId, string clientSecret)
            {
                this.clientId = clientId;
                this.cientSecret = clientSecret;
                //If clientid or client secret has special characters, encode before sending request
                this.request = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=http://api.microsofttranslator.com", HttpUtility.UrlEncode(clientId), HttpUtility.UrlEncode(clientSecret));
            }

            public AdmAccessToken GetAccessToken()
            {
                return HttpPost(DatamarketAccessUri, this.request);
            }

            private AdmAccessToken HttpPost(string DatamarketAccessUri, string requestDetails)
            {
                //Prepare OAuth request 
                WebRequest webRequest = WebRequest.Create(DatamarketAccessUri);
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.Method = "POST";
                byte[] bytes = Encoding.ASCII.GetBytes(requestDetails);
                webRequest.ContentLength = bytes.Length;
                using (Stream outputStream = webRequest.GetRequestStream())
                {
                    outputStream.Write(bytes, 0, bytes.Length);
                }
                using (WebResponse webResponse = webRequest.GetResponse())
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AdmAccessToken));
                    //Get deserialized object from JSON stream
                    AdmAccessToken token = (AdmAccessToken)serializer.ReadObject(webResponse.GetResponseStream());
                    return token;
                }
            }
        }

        /// <summary>
        /// LanguageEnum
        /// </summary>
        /// <remarks>
        /// http://msdn.microsoft.com/en-us/library/hh456380.aspx
        /// </remarks>
        public enum LanguageEnum
        {
            ar,      // Arabic
            bg,      // Bulgarian
            ca,      // Catalan
            zh_CHS,      // Chinese Simplified, zh-CHS
            zh_CHT,      // Chinese Traditional, zh-CHT
            cs,      // Czech
            da,      // Danish
            nl,      // Dutch
            en,      // English
            et,      // Estonian
            fi,      // Finnish
            fr,      // French
            de,      // German
            el,      // Greek
            ht,      // Haitian Creole
            he,      // Hebrew
            hi,      // Hindi
            mww,        // Hmong Daw
            hu,      // Hungarian
            id,      // Indonesian
            it,      // Italian
            ja,      // Japanese
            tlh,        // Klingon
            tlh_Qaak,      // Klingon (pIqaD), tlh-Qaak
            ko,      // Korean
            lv,      // Latvian
            lt,      // Lithuanian
            ms,      // Malay
            mt,      // Maltese
            no,      // Norwegian
            fa,      // Persian
            pl,      // Polish
            pt,      // Portuguese
            ro,      // Romanian
            ru,      // Russian
            sk,      // Slovak
            sl,      // Slovenian
            es,      // Spanish
            sv,      // Swedish
            th,      // Thai
            tr,      // Turkish
            uk,      // Ukrainian
            ur,      // Urdu
            vi,      // Vietnamese
            cy,      // Welsh
        };
    }
}
