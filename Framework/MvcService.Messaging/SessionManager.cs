using MvcService.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcService.Messaging
{
    public class SessionManager
    {
        private static object _lockObj = new object();
        private static SessionManager _current = null;

        private Dictionary<string, Session> _sessionDictionary = null;

        private SessionManager()
        {
            this._sessionDictionary = new Dictionary<string, Session>();
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

        public Session this[string sessionId]
        {
            get
            {
                if (!this._sessionDictionary.ContainsKey(sessionId))
                {
                    this._sessionDictionary.Add(sessionId, new Session() { Id = sessionId });
                }

                return this._sessionDictionary[sessionId];
            }
        }
    }
}
