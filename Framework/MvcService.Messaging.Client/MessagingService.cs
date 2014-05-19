using MvcService.Data;
using MvcService.Data.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MvcService.Messaging.Client
{
    class SessionManager
    {
        private static object _lockObj = new object();
        private static SessionManager _current = null;

        private Session _session = null;

        private SessionManager()
        {
            this._session = null;
        }

        public static SessionManager Current
        {
            get
            {
                if (_current == null)
                {
                    lock (_lockObj)
                    {
                        if (_current == null)
                        {
                            _current = new SessionManager();
                        }
                    }
                }

                return _current;
            }
        }

        public Session ActiveSession
        {
            get
            {
                return this._session;
            }

            internal set
            {
                this._session = value;
            }
        }
    }

    public class MessagingService
    {
        private static object _lockObj = new object();
        private static MessagingService _current = null;

        private Uri _serviceUri = null;
        private Uri _address = null;

        private MessagingService()
        { }

        public static MessagingService Current
        {
            get
            {
                if (_current == null)
                {
                    lock (_lockObj)
                    {
                        if (_current == null)
                        {
                            _current = new MessagingService();
                        }
                    }
                }

                return _current;
            }
        }

        public Session ActiveSession
        {
            get
            {
                return SessionManager.Current.ActiveSession;
            }
        }

        private MessagingService this[Uri address]
        {
            get
            {
                this._address = address;

                return _current;
            }
        }

        public MessagingService this[string serviceUrl, string methodName]
        {
            get
            {
                return _current[new Uri(this._serviceUri = new Uri(serviceUrl), methodName)];
            }
        }

        public MessagingService this[string methodName]
        {
            get
            {
                return _current[new Uri(this._serviceUri, methodName)];
            }
        }

        public TRes GetResponseAsync<TReq, TRes>(TReq requestEntity, Func<TReq, TRes, TRes> successMethod, Func<Func<TReq, Func<TReq, TRes, TRes>, TRes>, TRes> asyncRunner)
            where TReq : BaseEntity
            where TRes : BaseEntity
        {
            asyncRunner.BeginInvoke(this.GetResponse<TReq, TRes>,
                (ar) =>
                {
                    //AsyncResult result = (AsyncResult)ar;
                    //AsyncMethodCaller caller = (AsyncMethodCaller)result.AsyncDelegate;
                                       
                    //string returnValue = caller.EndInvoke(out threadId, ar);
                }, null);

            return this.GetResponse<TReq, TRes>(requestEntity, (req, res) => { return res; });
        }

        public TRes GetResponse<TReq, TRes>(TReq requestEntity)
            where TReq : BaseEntity
            where TRes : BaseEntity
        {
            return this.GetResponse<TReq, TRes>(requestEntity, (req, res) => { return res; });
        }

        public TRes GetResponse<TReq, TRes>(TReq requestEntity, Func<TReq, TRes, TRes> successMethod)
            where TReq : BaseEntity
            where TRes : BaseEntity
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(this._address);

            req.Method = "POST";
            req.ContentType = "application/json";

            using (Stream reqs = req.GetRequestStream())
            {
                requestEntity.Session = SessionManager.Current.ActiveSession;

                byte[] reqb = JsonHelper.ConvertToJson(requestEntity).GetBytes();
                reqs.Write(reqb, 0, reqb.Length);
                reqs.Flush();

                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    using (Stream resps = resp.GetResponseStream())
                    {
                        using (StreamReader rdr = new StreamReader(resps))
                        {
                            TRes res = (TRes)JsonHelper.ParseJson(rdr.ReadToEnd(), typeof(TRes));

                            SessionManager.Current.ActiveSession = res.Session;

                            return successMethod(requestEntity, res);
                        }
                    }
                }
            }
        }
    }
}
