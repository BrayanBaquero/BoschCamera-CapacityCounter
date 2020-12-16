using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Bosch.VideoSDK.GCALib;
using Bosch.VideoSDK.Live;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;


namespace CSharpRuntimeCameo
{
	public partial class MainWindow : Form
	{
		private enum HRESULT : uint
		{
			S_OK = 0,
			E_FAIL = 0x80004005,
			E_UNEXPECTED = 0x8000FFFF,
			E_NOTIMPL = 0x80004001,
			E_INVALIDARG = 0x80070057,
			IgnoreAndFixLater = 0xFFFFFFFF
		};

		private enum State
		{
			Disconnected,
			Connecting,
			Connected,
			Disconnecting
		}

		public MainWindow()
		{
			InitializeComponent();
		}
		private State m_state = State.Disconnected;
		private Bosch.VideoSDK.Device.DeviceConnector m_deviceConnector = new Bosch.VideoSDK.Device.DeviceConnectorClass();
		private Bosch.VideoSDK.Device.DeviceProxy m_deviceProxy = null;
		private Bosch.VideoSDK.AxCameoLib.AxCameo m_axCameo = null;
		private Bosch.VideoSDK.CameoLib.Cameo m_cameo = null;
        
        private Bosch.VideoSDK.GCALib._IVideoInputVCAEvents_Event m_videoInputVCAEvents = null;
        

        private int entrada=0;
        private int salida=0;
        private int aforo = 0;
        private IManagedMqttClient client;
        private int ocpmax;
        private string Message_Max;
        private string Mess_Norm;
        private bool Out_Ext;
        RelayNode relayNode = new RelayNode();
        private string clientId;
        private bool EstadoCloud;

        private void MainWindow_Load(object sender, EventArgs e)
		{
			m_axCameo = new Bosch.VideoSDK.AxCameoLib.AxCameo();
			PanelCameo.Controls.Add(m_axCameo);
			m_axCameo.Dock = DockStyle.Fill;
			m_cameo = (Bosch.VideoSDK.CameoLib.Cameo)m_axCameo.GetOcx();
			
			m_deviceConnector.ConnectResult += new Bosch.VideoSDK.GCALib._IDeviceConnectorEvents_ConnectResultEventHandler(DeviceConnector_ConnectResult);
            CheckForIllegalCrossThreadCalls = false;
            
            UpdateGUI();
            label6.Text=Properties.Settings.Default.SetPoint.ToString();
            ocpmax =Int32.Parse(Properties.Settings.Default.SetPoint);
            entrada = int.Parse(Properties.Settings.Default.TaskInt.ToString());
            salida = int.Parse(Properties.Settings.Default.TaskOut.ToString());
        }

        private void M_relay_StateChanged(Relay pEventSource, bool State)
        {
            Debug.WriteLine("Relay:",State);
        }

        private void Relay_StateChanged(Relay pEventSource, bool State)
        {
            Debug.WriteLine("Relay:",State);
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			if ((m_state == State.Connecting) || (m_state == State.Disconnecting))
				e.Cancel = true;
		}

		private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
		{
			m_deviceConnector.ConnectResult -= new Bosch.VideoSDK.GCALib._IDeviceConnectorEvents_ConnectResultEventHandler(DeviceConnector_ConnectResult);
			if (m_state == State.Connected)
			{
				m_deviceProxy.ConnectionStateChanged -= new Bosch.VideoSDK.GCALib._IDeviceProxyEvents_ConnectionStateChangedEventHandler(DeviceProxy_ConnectionStateChanged);
				m_deviceProxy.Disconnect();
			}
			m_axCameo.Dispose();
		}

		


        private void DeviceConnector_ConnectResult(Bosch.VideoSDK.Device.ConnectResultEnum connectResult, string url, Bosch.VideoSDK.Device.DeviceProxy deviceProxy)
		{
			bool success = false;

			if (connectResult == Bosch.VideoSDK.Device.ConnectResultEnum.creInitialized)
			{
				if (url.ToLower().IndexOf("file") == 0)
				{
					Bosch.VideoSDK.MediaDatabase.PlaybackController pc = new Bosch.VideoSDK.MediaDatabase.PlaybackController();
					Bosch.VideoSDK.MediaSession session = deviceProxy.MediaDatabase.GetMediaSession(-1, pc);

					success = true;

					try
					{
						m_cameo.SetVideoStream(session.GetVideoStream());
						pc.Play(100);
					}
					catch (Exception ex)
					{
						CheckException(ex, "Failed to render file video stream of {0}", url);
						success = false;
					}
				}
				else
				{
					if (deviceProxy.VideoInputs.Count > 0)
					{
						success = true;

						try
						{
							m_cameo.SetVideoStream(deviceProxy.VideoInputs[1].Stream);
						}
						catch (Exception ex)
						{
							CheckException(ex, "Failed to render first video stream of {0}", url);
							success = false;
						}
					}
				}
			}

			if (success)
			{
				m_deviceProxy = deviceProxy;
				m_deviceProxy.ConnectionStateChanged += new Bosch.VideoSDK.GCALib._IDeviceProxyEvents_ConnectionStateChangedEventHandler(DeviceProxy_ConnectionStateChanged);
                //m_state = State.Connected;

                try
                {
                    m_videoInputVCAEvents = (Bosch.VideoSDK.GCALib._IVideoInputVCAEvents_Event)m_deviceProxy.VideoInputs[1];
                    m_videoInputVCAEvents.MotionDetectorsStateChanged += new Bosch.VideoSDK.GCALib._IVideoInputVCAEvents_MotionDetectorsStateChangedEventHandler(m_videoInputVCAEvents_MotionDetectorsStateChanged);

                    if (deviceProxy.Relays != null)
                    {
                       
                        foreach (Bosch.VideoSDK.Live.Relay relay in deviceProxy.Relays)
                        {
                            relayNode.SetRelay(relay);
                            
                            // relaysNode.Nodes.Add(relayNode);
                        }
                    }
                  
                }
                catch (System.InvalidCastException)
                {
                    m_videoInputVCAEvents = null;
                }
                m_state = State.Connected;
            }

               


                else
			    {
				    if (deviceProxy != null)
					    deviceProxy.Disconnect();
				    m_state = State.Disconnected;
				    MessageBox.Show("Failed to connect to \"" + url + "\".");
			    }

			UpdateGUI();
		}

         void m_videoInputVCAEvents_MotionDetectorsStateChanged(Bosch.VideoSDK.Live.VideoInput pEventSource, int ConfigId, int DetectorsState) {
            Debug.WriteLine(DetectorsState);

            if (DetectorsState == entrada) {
                aforo = aforo + 1;
               
            }
            if ( DetectorsState == salida)
            {
                if (aforo > 0) {
                    aforo = aforo - 1;
                    
                }
                
            }

            

           
            label2.Text= aforo.ToString();
            if (DetectorsState==1 || DetectorsState == 2)
            {
                if (aforo >= ocpmax)
                {
                    label1.Text = Properties.Settings.Default.MessMax.ToString();
                    groupBox1.BackColor = Color.Red;
                    if(Out_Ext ==true)
                        relayNode.setstate(true);

                }
                else
                {
                    label1.Text = Properties.Settings.Default.MessNorm.ToString();
                    groupBox1.BackColor = Color.Green;
                    if(Out_Ext==true)
                        relayNode.setstate(false);

                }
                try
                {
                    if (client.IsConnected)
                        SendMqtt();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("MQTTnet no conectado");
                }

            }

        }

        private async void SendMqtt()
        {
            var datos = new Customer()
            {
                data = new Data { conteo = aforo, setpoint=ocpmax }
            };


            string json = JsonConvert.SerializeObject(datos);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic("losant/"+clientId+"/state")
                .WithPayload(json)
                .Build();

            
            await client.PublishAsync(message);
        }

		private void DeviceProxy_ConnectionStateChanged(object eventSource, Bosch.VideoSDK.Device.ConnectResultEnum state)
		{
			if (state == Bosch.VideoSDK.Device.ConnectResultEnum.creConnectionTerminated)
			{
				m_cameo.SetVideoStream(null);
				m_deviceProxy.ConnectionStateChanged -= new Bosch.VideoSDK.GCALib._IDeviceProxyEvents_ConnectionStateChangedEventHandler(DeviceProxy_ConnectionStateChanged);
				m_deviceProxy = null;
				m_state = State.Disconnected;

				UpdateGUI();
			}
		}


		private void UpdateGUI()
		{
            
            if (EstadoCloud)
            {
                 label4.Text = "Conectado";
                 panel2.BackColor = Color.Green;
            }else
            {
                 label4.Text = "Desconectado";
                 panel2.BackColor = Color.Red;
            }
            
            if (m_state == State.Disconnected)
			{
                conectarToolStripMenuItem.Text = "Conectar";
                labelStatus.Text = "Desconectado";
                panel1.BackColor = Color.Red;
                conectarToolStripMenuItem.Enabled = true;
                //ButtonConnect.Enabled = (TextBoxUrl.Text.Length > 0);

            }
			else if (m_state == State.Connecting)
			{
                conectarToolStripMenuItem.Text = "Conectando";
                conectarToolStripMenuItem.Enabled = false;
			}
			else if (m_state == State.Connected)
			{
				conectarToolStripMenuItem.Text = "Desconectar";
                conectarToolStripMenuItem.Enabled = true;
                labelStatus.Text = "Conectado";
                panel1.BackColor = Color.Green;
            }
			else // if (m_state == State.Disconnecting)
			{
                conectarToolStripMenuItem.Text = "Conectando";
                conectarToolStripMenuItem.Enabled = false;
			}
		}

		private HRESULT CheckException(Exception ex, string format, params object[] args)
		{
			string message = string.Format(format, args) + ": " + ex.Message;
			if (ex.GetType() == typeof(System.Runtime.InteropServices.COMException))
			{
				uint errorCode = (uint)((System.Runtime.InteropServices.COMException)ex).ErrorCode;
				if (errorCode == (uint)HRESULT.E_FAIL)
					return HRESULT.E_FAIL;
				else if (errorCode == (uint)HRESULT.E_UNEXPECTED)
					return HRESULT.E_UNEXPECTED;
			}
			else if (ex.GetType() == typeof(System.NotImplementedException))
				return HRESULT.E_NOTIMPL;
			else if (ex.GetType() == typeof(System.ArgumentException))
				return HRESULT.E_INVALIDARG;

			if (MessageBox.Show(message + "\n\nTerminate application?", "Unexpected Exception", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
			{
				System.Diagnostics.Process.GetCurrentProcess().Kill();
				throw ex;
			}
			else
				return HRESULT.IgnoreAndFixLater;
		}

        private void PanelCameo_Paint(object sender, PaintEventArgs e)
        {

        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            UpdateGUI();
        }

        private void ConectarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_state == State.Disconnected)
            {
                CamaraConfig formulario = new CamaraConfig();
                formulario.pasado += new CamaraConfig.sendCred(conn);

                formulario.ShowDialog();
            }
            else if (m_state == State.Connected)
            {
                m_state = State.Disconnecting;
                m_deviceProxy.Disconnect();
            }
            UpdateGUI();

        }

        public void conn(string dato,int ent,int outt)
        {
            entrada = ent;
            salida = outt;
           
            if (m_state == State.Disconnected)
            {
                try
                {
                    m_state = State.Connecting;
                    m_deviceConnector.ConnectAsync(dato, "GCA.VIP.DeviceProxy");
                }
                catch (Exception ex)
                {
                    if (HRESULT.IgnoreAndFixLater != CheckException(ex, "Failed to start asynchronous connection attempt to \"{0}\"", dato))
                        MessageBox.Show("Invalid IP address or progID! \n\nIP address:  " + dato+ "\nProgID: GCA.VIP.DeviceProxy  Invalid Argument");

                    m_state = State.Disconnected;
                    UpdateGUI();
                }
            }
            else if (m_state == State.Connected)
            {
                m_state = State.Disconnecting;
                m_deviceProxy.Disconnect();
            }

            UpdateGUI();
            
        }
        private void NivelMaximoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPoint sp = new SetPoint();
            sp.pasado += new SetPoint.sendCred(Ocupacion);
            sp.ShowDialog();
        }

        private void Ocupacion(string sp, string Messmx, string Messnorm, bool outX)
        {
            ocpmax = Convert.ToInt32(sp);
            Message_Max = Messmx;
            Mess_Norm = Messnorm;
            Out_Ext = outX;
            label6.Text = sp;
        }
    

       
           

        private void ConfiguracionToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (EstadoCloud)
            {
                configuracionToolStripMenuItem1.Text = "Conectar";
                client.StopAsync();
            }
            else
            {
                configuracionToolStripMenuItem1.Text = "Desconectar";
                Cloud cloud = new Cloud();
                cloud.pasado += new Cloud.sendCred(cloudCred);
                cloud.ShowDialog();
            }
            
        }

      


        private async void cloudCred(string di, string ak, string ass)
        {
            clientId = di;
            string mqttURI = "broker.losant.com";
            string mqttUser = ak;
            string mqttPassword = ass;
            int mqttPort = 1883;
            bool mqttSecure = false;
            var messageBuilder = new MqttClientOptionsBuilder()
            .WithClientId(clientId)
            .WithCredentials(mqttUser, mqttPassword)
            .WithTcpServer(mqttURI, mqttPort)
            .WithCleanSession();
            var options = mqttSecure
              ? messageBuilder
                .WithTls()
                .Build()
              : messageBuilder
                .Build();
            var managedOptions = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(options)
                .Build();
            client = new MqttFactory().CreateManagedMqttClient();
            client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(connected);
            client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(disconnected);
            client.UseApplicationMessageReceivedHandler(e => { HandleMessageReceived(e.ApplicationMessage); });
            await client.StartAsync(managedOptions);
        }

        private void disconnected(MqttClientDisconnectedEventArgs obj)
        {
            Debug.WriteLine("MQTT Desconectado");
            EstadoCloud = false;
            UpdateGUI();
        }

        private void connected(MqttClientConnectedEventArgs obj)
        {
            Debug.WriteLine("MQTT Conectado");
            EstadoCloud = true;
            subtopic();
            UpdateGUI();

        }
        private async void subtopic() {
            await client.SubscribeAsync(new TopicFilterBuilder()
                .WithTopic("losant/"+clientId+"/command").Build());
        }

        private void HandleMessageReceived(MqttApplicationMessage applicationMessage)
        {
            Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
            Console.WriteLine($"+ Topic = {applicationMessage.Topic}");

            Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(applicationMessage.Payload)}");
            Console.WriteLine($"+ QoS = {applicationMessage.QualityOfServiceLevel}");
            Console.WriteLine($"+ Retain = {applicationMessage.Retain}");

            Dictionary<string, object> obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(Encoding.UTF8.GetString(applicationMessage.Payload));

            Console.WriteLine(obj["name"]);
            if (obj["name"].ToString()=="setpoint")
            {
                ocpmax = Int32.Parse(obj["payload"].ToString());
                label6.Text = obj["payload"].ToString();
            }

            Console.WriteLine(Encoding.UTF8.GetString(applicationMessage.Payload).GetType());
        }

        private void MainWindow_DoubleClick(object sender, EventArgs e)
        {
            //Debug.WriteLine("Hola Hola");
            if (FormBorderStyle == FormBorderStyle.None)
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.None;
                
            }
            
        }
    }
}
