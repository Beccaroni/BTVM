using System;
using System.Windows.Forms;
using System.Collections.Generic;


namespace BTVM.Interfaces
{
    public interface IMessageListener1
    {
        void OnListen(List<string> listMessage, string strMessage, Form Type);
        
    }
}
