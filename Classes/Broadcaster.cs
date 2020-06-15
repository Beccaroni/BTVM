using BTVM.Interfaces;
// My using Declarations
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Forms;


namespace BTVM.Classes
{
    public class Broadcaster
    {
        private readonly Collection<IMessageListener1> _listeners =
            new Collection<IMessageListener1>();

        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="sender"></param>
        /// <remarks></remarks>
        [DebuggerStepThrough()]
        public void Broadcast(List<string> listMessage, string strMessage, Form sender)
        {
            foreach (IMessageListener1 listener in _listeners)
            {
                listener.OnListen(listMessage, strMessage, sender);
            }
        }

        /// <summary>
        /// Add a Listener to the Collection of Listeners
        /// </summary>
        /// <param name="listener"></param>
        public void AddListener(IMessageListener1 listener)
        {
            _listeners.Add(listener);
        }
        /// <summary>
        /// Remove a Listener from the collection
        /// </summary>
        /// <param name="listener"></param>
        public void RemoveListener(IMessageListener1 listener)
        {

            for (int index = 0; index < _listeners.Count; index++)
            {
                if (_listeners[index].Equals(listener))
                {
                    _listeners.Remove(_listeners[index]);
                }
            }
        }

        
    }
}