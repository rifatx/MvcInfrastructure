using MvcService.Data;
using MvcService.Data.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;

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

    class MobileRequest<TReq> where TReq : class
    {
        private string _key = null;

        public HttpWebRequest Request { get; set; }

        public object Payload { private get; set; }

        public TReq Data
        {
            get
            {
                return this.Payload as TReq;
            }
        }

        public string Key
        {
            get
            {
                if (string.IsNullOrEmpty(this._key))
                {
                    this._key = Guid.NewGuid().ToString("N");
                }

                return this._key;
            }
        }

        public void DisposeRequest()
        {
            if (this.Request != null)
            {
                try
                {
                    this.Request.Abort();
                }
                catch { }
            }

            this.Request = null;
        }
    }

    public class MessagingService
    {
        private static object _lockObj = new object();
        private static MessagingService _current = null;

        private Uri _serviceUri = null;
        private Uri _address = null;
        private long _timeout = 15000;
        private Dictionary<string, Timer> _timerList = new Dictionary<string, Timer>();

        public delegate void AsyncGetResponseCaller<TReq, TRes>(TReq requestEntity, Action<TReq, TRes> successMethod, Action<TReq> errorMethod, Uri address, long timeout);

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

        private MessagingService this[Uri address, long timeout = 15000]
        {
            get
            {
                this._address = address;
                this._timeout = timeout;

                return _current;
            }
        }

        public MessagingService this[string serviceUrl, string methodName, long timeout = 15000]
        {
            get
            {
                return _current[new Uri(this._serviceUri = new Uri(serviceUrl), methodName), timeout];
            }
        }

        public MessagingService this[string methodName, long timeout = 15000]
        {
            get
            {
                return _current[new Uri(this._serviceUri, methodName), timeout];
            }
        }

        public void GetResponseAsync<TReq, TRes>(TReq requestEntity, Action<TReq, TRes> successMethod)
            where TReq : BaseEntity
            where TRes : BaseEntity
        {
            this.GetResponseAsync<TReq, TRes>(requestEntity, successMethod, (req) => { });
        }

        public void GetResponseAsync<TReq, TRes>(TReq requestEntity, Action<TReq, TRes> successMethod, Action<TReq> errorMethod)
            where TReq : BaseEntity
            where TRes : BaseEntity
        {
            new AsyncGetResponseCaller<TReq, TRes>(this.GetResponse).BeginInvoke(requestEntity, successMethod, errorMethod, this._address, this._timeout,
                (ar) =>
                {
                    ((AsyncGetResponseCaller<TReq, TRes>)((AsyncResult)ar).AsyncDelegate).EndInvoke(ar);
                },
                null);
        }

        public TRes GetResponse<TReq, TRes>(TReq requestEntity)
            where TReq : BaseEntity
            where TRes : BaseEntity
        {
            TRes responseEntity = null;
            SemaphoreSlim ss = new SemaphoreSlim(0, 1);

            this.GetResponse<TReq, TRes>(requestEntity, (req, res) =>
            {
                responseEntity = res;
                ss.Release();
            },
            (req) =>
            { });

            ss.Wait();
            return responseEntity;
        }

        public void GetResponse<TReq, TRes>(TReq requestEntity, Action<TReq, TRes> successMethod, Action<TReq> errorMethod)
            where TReq : BaseEntity
            where TRes : BaseEntity
        {
            this.GetResponse<TReq, TRes>(requestEntity, successMethod, errorMethod, this._address, this._timeout);
        }

        private void GetResponse<TReq, TRes>(TReq requestEntity, Action<TReq, TRes> successMethod, Action<TReq> errorMethod, Uri address, long timeout)
            where TReq : BaseEntity
            where TRes : BaseEntity
        {
            requestEntity.Session = SessionManager.Current.ActiveSession;

            MobileRequest<TReq> req = new MobileRequest<TReq>();

            req.Request = (HttpWebRequest)HttpWebRequest.Create(address);
            req.Request.Method = "POST";
            req.Request.ContentType = "application/json";
            req.Request.ContinueTimeout = 1;
            req.Payload = requestEntity;

            Timer t = null;
            t = new Timer(new TimerCallback((o) =>
                {
                    if (t != null)
                    {
                        t.Change(Timeout.Infinite, Timeout.Infinite);
                        t = null;

                        if (req != null && this._timerList.ContainsKey(req.Key))
                        {
                            this._timerList.Remove(req.Key);
                        }
                    }

                    if (req.Request != null)
                    {
                        req.DisposeRequest();
                        req = null;
                    }
                }), null, Timeout.Infinite, Timeout.Infinite);

            if (!this._timerList.ContainsKey(req.Key))
            {
                this._timerList.Add(req.Key, t);
            }

            t.Change(timeout, Timeout.Infinite);

            req.Request.BeginGetRequestStream((ar) =>
                {
                    MobileRequest<TReq> mr = (MobileRequest<TReq>)((dynamic)ar.AsyncState).Request;

                    using (Stream s = mr.Request.EndGetRequestStream(ar))
                    {
                        byte[] reqb = JsonHelper.ConvertToJson(req.Data).GetBytes();

                        s.Write(reqb, 0, reqb.Length);
                        s.Flush();
                    }

                    mr.Request.BeginGetResponse(new AsyncCallback((ar2) =>
                        {
                            try
                            {
                                using (HttpWebResponse resp = (HttpWebResponse)mr.Request.EndGetResponse(ar2))
                                {
                                    using (var rdr = new StreamReader(resp.GetResponseStream()))
                                    {
                                        //Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                                        //    () =>
                                        //    {
                                        TRes res;
                                        SessionManager.Current.ActiveSession = (res = (TRes)JsonHelper.ParseJson(rdr.ReadToEnd(), typeof(TRes))).Session;

                                        if (successMethod != null)
                                        {
                                            successMethod(requestEntity, res);
                                        }
                                        //});
                                    }
                                }
                            }
                            catch
                            {
                                if (errorMethod != null)
                                {
                                    errorMethod(requestEntity);
                                }
                            }
                            finally
                            {
                                //Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                                //() =>
                                //{
                                if (this._timerList.ContainsKey(mr.Key))
                                {
                                    Timer tt = this._timerList[mr.Key];
                                    tt.Change(Timeout.Infinite, Timeout.Infinite);
                                    tt.Dispose();
                                    tt = null;
                                    this._timerList.Remove(mr.Key);
                                }
                                //});
                            }
                        }), ar.AsyncState);
                },
            new { Request = req });
        }
    }
}
