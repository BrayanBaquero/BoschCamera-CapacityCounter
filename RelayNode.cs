using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpRuntimeCameo
{
    public class RelayNode
    {
        private Bosch.VideoSDK.Live.Relay m_relay = null;
        public RelayNode()
        {
        }

        public void SetRelay(Bosch.VideoSDK.Live.Relay relay)
        {
            //System.Windows.Forms.TreeView deviceTree = null;

            //deviceTree = MainForm.s_mainForm.GetDeviceTree();

            if (m_relay != null)
                m_relay.StateChanged -= new Bosch.VideoSDK.GCALib._IRelayEvents_StateChangedEventHandler(RelayNode_StateChanged);

            m_relay = relay;

            if (m_relay != null)
                m_relay.StateChanged += new Bosch.VideoSDK.GCALib._IRelayEvents_StateChangedEventHandler(RelayNode_StateChanged);
        }
        public void ToggleState()
        {
            try
            {
                if (m_relay.Enabled)
                    m_relay.SetState(!m_relay.GetState());
            }
            catch (Exception ex)
            {
                //Common.CheckException(ex, "Error setting relay state");
                Debug.WriteLine("Error");
            }
        }
        private void RelayNode_StateChanged(Bosch.VideoSDK.Live.Relay EventSource, bool State)
        {
            DevolverNombre();
        }

       
        public  void DevolverNombre()
        {
            bool state = m_relay.GetState();
            string name = m_relay.Name;

        }

        public void setstate(bool state)
        {
            try
            {
                if (m_relay.Enabled)
                    m_relay.SetState(state);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error 2");
            }
        }


    }

}
