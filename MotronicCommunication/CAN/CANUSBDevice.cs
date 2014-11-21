using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MotronicCommunication.CAN;
using System.IO;

namespace MotronicCommunication.KWP
{
    /// <summary>
    /// CANUSBDevice is an implementation of ICANDevice for the Lawicel CANUSB device
    /// (www.canusb.com). 
    /// In this implementation the open method autmatically detects if the device is connected
    /// to a T8 I-bus or P-bus. The autodetection is primarily done by listening for the 0x280
    /// message (sent on both busses) but if the device is started after an interrupted flashing
    /// session there is no such message available on the bus. There fore the open method sends
    /// a message to set address and length for flashing. If there is a reply there is connection.
    /// 
    /// All incomming messages are published to registered ICANListeners.
    /// </summary>
    public class CANUSBDevice : ICANDevice
    {
        /*public delegate void ReceivedAdditionalInformationFrame(object sender, InformationFrameEventArgs e);
        public event CANUSBDevice.ReceivedAdditionalInformationFrame onReceivedAdditionalInformationFrame;

        private bool _useOnlyPBus = true;

        public bool UseOnlyPBus
        {
            get { return _useOnlyPBus; }
            set { _useOnlyPBus = value; }
        }*/
        private int bitsPerSecond = 0;

        public override int BitsPerSecond
        {
            get { return bitsPerSecond; }
            set { bitsPerSecond = value; }
        }
        static uint m_deviceHandle = 0;
        Thread m_readThread;
        Object m_synchObject = new Object();
        bool m_endThread = false;

        private int m_forcedBaudrate = 38400;

        public override int ForcedBaudrate
        {
            get
            {
                return m_forcedBaudrate;
            }
            set
            {
                m_forcedBaudrate = value;
            }
        }

        private string m_forcedComport = string.Empty;

        public override string ForcedComport
        {
            get
            {
                return m_forcedComport;
            }
            set
            {
                m_forcedComport = value;
            }
        }


        /*private bool m_EnableCanLog = false;

        public bool EnableCanLog
        {
            get { return m_EnableCanLog; }
            set { m_EnableCanLog = value; }
        }*/
        /// <summary>
        /// Constructor for CANUSBDevice.
        /// </summary>
        public CANUSBDevice()
        {
            //m_readThread = new Thread(readMessages);
            //m_readThread.Priority = ThreadPriority.Highest;
        }

        // not supported by lawicel
        public override float GetADCValue(uint channel)
        {
            return 0F;
        }

        // not supported by lawicel
        public override float GetThermoValue()
        {
            return 0F;
        }

        /// <summary>
        /// Destructor for CANUSBDevice.
        /// </summary>
        ~CANUSBDevice()
        {
            lock (m_synchObject)
            {
                m_endThread = true;
            }
            close();
        }

        public override void Flush()
        {
            LAWICEL.canusb_Flush(m_deviceHandle, 0x01);
            LAWICEL.canusb_Flush(m_deviceHandle, 0x03);
        }

        /// <summary>
        /// readMessages is the "run" method of this class. It reads all incomming messages
        /// and publishes them to registered ICANListeners.
        /// </summary>
        public void readMessages()
        {
            int readResult = 0;
            LAWICEL.CANMsg r_canMsg = new LAWICEL.CANMsg();
            CANMessage canMessage = new CANMessage();
            Console.WriteLine("readMessages started");
            while (true)
            {
                lock (m_synchObject)
                {
                    if (m_endThread)
                    {
                        Console.WriteLine("readMessages ended");
                        return;
                    }
                }
                readResult = LAWICEL.canusb_Read(m_deviceHandle, out r_canMsg);
                if (readResult == LAWICEL.ERROR_CANUSB_OK)
                {
                    //Console.WriteLine(r_canMsg.id.ToString("X6") + " " + r_canMsg.data.ToString("X16"));
                    //if (MessageContainsInformationForRealtime(r_canMsg.id))
                    {
                        canMessage.setID(r_canMsg.id);
                        canMessage.setLength(r_canMsg.len);
                        canMessage.setTimeStamp(r_canMsg.timestamp);
                        canMessage.setFlags(r_canMsg.flags);
                        canMessage.setData(r_canMsg.data);
                        lock (m_listeners)
                        {
                            bitsPerSecond += 109;
                            AddToCanTrace("RX: " + r_canMsg.id.ToString("X6") + " " + r_canMsg.data.ToString("X16"));
                            rxCount++;
                            foreach (ICANListener listener in m_listeners)
                            {
                                //while (listener.messagePending()) ; // dirty, make this better
                                listener.handleMessage(canMessage);
                            }
                            CastInformationEvent("", rxCount, txCount, errCount); // <GS-05042011> re-activated this function
                        }
                        //Thread.Sleep(1);
                    }

                    // cast event to application to process message
                    //if (MessageContainsInformationForRealtime(r_canMsg.id))
                    //{
                        //TODO: process all other known msg id's into the realtime view
                      //  CastInformationEvent(canMessage); // <GS-05042011> re-activated this function
                    //}
                }
                else if (readResult == LAWICEL.ERROR_CANUSB_NO_MESSAGE)
                {
                   // Console.WriteLine("No message");
                    Thread.Sleep(1);
                }
                else
                {
                    Console.WriteLine("Result: " + readResult.ToString("X8"));
                }
                /*int stat = LAWICEL.canusb_Status(m_deviceHandle);
                if (stat != 0)
                {
                    Console.WriteLine("status: " + stat.ToString("X4"));
                }*/
            }
            
        }

        /*private void CastInformationEvent(CANMessage message)
        {
            if (onReceivedAdditionalInformationFrame != null)
            {
                onReceivedAdditionalInformationFrame(this, new InformationFrameEventArgs(message));
            }
        }

        private bool MessageContainsInformationForRealtime(uint msgId)
        {
            bool retval = false;
            switch (msgId)
            {
                case 0x1A0:         //1A0h - Engine information
                case 0x280:         //280h - Pedals, reverse gear
                case 0x290:         //290h - Steering wheel and SID buttons
                case 0x2F0:         //2F0h - Vehicle speed
                case 0x320:         //320h - Doors, central locking and seat belts
                case 0x370:         //370h - Mileage
                case 0x3A0:         //3A0h - Vehicle speed
                case 0x3B0:         //3B0h - Head lights
                case 0x3E0:         //3E0h - Automatic Gearbox
                case 0x410:         //410h - Light dimmer and light sensor
                case 0x430:         //430h - SID beep request (interesting for Knock indicator?)
                case 0x460:         //460h - Engine rpm and speed
                case 0x4A0:         //4A0h - Steering wheel, Vehicle Identification Number
                case 0x520:         //520h - ACC, inside temperature
                case 0x530:         //530h - ACC
                case 0x5C0:         //5C0h - Coolant temperature, air pressure
                case 0x630:         //630h - Fuel usage
                case 0x640:         //640h - Mileage
                case 0x7A0:         //7A0h - Outside temperature
                    retval = true; 
                    break;
            }
            return retval;
        }*/

        /// <summary>
        /// The open method tries to connect to both busses to see if one of them is connected and
        /// active. The first strategy is to listen for any CAN message. If this fails there is a
        /// check to see if the application is started after an interrupted flash session. This is
        /// done by sending a message to set address and length (only for P-bus).
        /// </summary>
        /// <returns>OpenResult.OK is returned on success. Otherwise OpenResult.OpenError is
        /// returned.</returns>
        override public OpenResult open(bool is500KB)
        {
            Console.WriteLine("******* CANUSB: Opening CANUSB");
            rxCount = 0;
            txCount = 0;
            errCount = 0;

            LAWICEL.CANMsg msg = new LAWICEL.CANMsg();
            //Check if I bus is connected
            if (m_deviceHandle != 0)
            {
                close();
            }
            Thread.Sleep(200);
            m_readThread = new Thread(readMessages);

            if (m_deviceHandle != 0)
            {
                close();
            }
            m_endThread = false;
            string speed = "250";
            if (is500KB) speed = "500";
            Console.WriteLine(speed);
            m_deviceHandle = LAWICEL.canusb_Open(IntPtr.Zero,
            speed,              
            LAWICEL.CANUSB_ACCEPTANCE_CODE_ALL,
            LAWICEL.CANUSB_ACCEPTANCE_MASK_ALL,
            LAWICEL.CANUSB_FLAG_TIMESTAMP);
            Console.WriteLine("Checking box presence");
            Console.WriteLine("Handle: "+ m_deviceHandle.ToString("X8"));
            if (m_deviceHandle == 0x00000000)
            {
                return OpenResult.OpenError;
            }
            if (boxIsThere())
            {
                Console.WriteLine("Box is there, starting thread");
                if (m_readThread.ThreadState == ThreadState.Unstarted)
                    m_readThread.Start();
                return OpenResult.OK;
            }
            Console.WriteLine("Box not there");
            close();
            return OpenResult.OpenError;
        }

        public override string getVersion()
        {
            StringBuilder verinfo = new StringBuilder();
            verinfo.Length = 128;
            string retval = string.Empty;
            if (m_deviceHandle > 0)
            {
                LAWICEL.canusb_VersionInfo(m_deviceHandle, verinfo);
                retval = verinfo.ToString();
            }
            return retval;

        }

        /// <summary>
        /// The close method closes the CANUSB device.
        /// </summary>
        /// <returns>CloseResult.OK on success, otherwise CloseResult.CloseError.</returns>
        override public CloseResult close()
        {
            Console.WriteLine("******* CANUSB: Closing CANUSB");

            int res = 0;
            rxCount = 0;
            txCount = 0;
            errCount = 0;
            try
            {
                res = LAWICEL.canusb_Close(m_deviceHandle);
            }
            catch(DllNotFoundException e)
            {
                Console.WriteLine("CANUSBDevice::close: " + e.Message);
                return CloseResult.CloseError;
            }
            m_endThread = true;
            /*if (m_readThread.ThreadState != ThreadState.Unstarted)
                m_readThread.Abort();*/

            m_deviceHandle = 0;
            if (LAWICEL.ERROR_CANUSB_OK == res)
            {
                return CloseResult.OK;
            }
            else
            {
                return CloseResult.CloseError;
            }
        }

        /// <summary>
        /// isOpen checks if the device is open.
        /// </summary>
        /// <returns>true if the device is open, otherwise false.</returns>
        override public bool isOpen()
        {
            if (m_deviceHandle > 0)
                return true;
            else
                return false;
        }

        /*private void AddToCanTrace(string line)
        {
            if (m_EnableCanLog)
            {
                DateTime dtnow = DateTime.Now;
                using (StreamWriter sw = new StreamWriter(System.Windows.Forms.Application.StartupPath + "\\CanTraceCANUSBDevice.txt", true))
                {
                    sw.WriteLine(dtnow.ToString("dd/MM/yyyy HH:mm:ss") + " - " + line);
                }
            }
        }*/
        /// <summary>
        /// sendMessage send a CANMessage.
        /// </summary>
        /// <param name="a_message">A CANMessage.</param>
        /// <returns>true on success, othewise false.</returns>
        override public bool sendMessage(CANMessage a_message)
        {
            LAWICEL.CANMsg msg = new LAWICEL.CANMsg();
            msg.id = a_message.getID();
            msg.len = a_message.getLength();
            msg.flags = a_message.getFlags();
            msg.flags = LAWICEL.CANMSG_EXTENDED; // Test for now
            msg.data = a_message.getData();
            int writeResult;
            //AddToCanTrace("Sending message");
            AddToCanTrace("TX: " + msg.id.ToString("X6") + " " + msg.data.ToString("X16"));
            writeResult = LAWICEL.canusb_Write(m_deviceHandle, ref msg);
            if (writeResult == LAWICEL.ERROR_CANUSB_OK)
            {
                //AddToCanTrace("Message sent successfully");
                txCount++;
                bitsPerSecond += 109;
                return true;
            }
            else
            {
                errCount++;
                switch (writeResult)
                {
                    case LAWICEL.ERROR_CANUSB_COMMAND_SUBSYSTEM:
                        AddToCanTrace("Message failed to send: ERROR_CANUSB_COMMAND_SUBSYSTEM");
                        break;
                    case LAWICEL.ERROR_CANUSB_INVALID_PARAM:
                        AddToCanTrace("Message failed to send: ERROR_CANUSB_INVALID_PARAM");
                        break;
                    case LAWICEL.ERROR_CANUSB_NO_MESSAGE:
                        AddToCanTrace("Message failed to send: ERROR_CANUSB_NO_MESSAGE");
                        break;
                    case LAWICEL.ERROR_CANUSB_NOT_OPEN:
                        AddToCanTrace("Message failed to send: ERROR_CANUSB_NOT_OPEN");
                        break;
                    case LAWICEL.ERROR_CANUSB_OPEN_SUBSYSTEM:
                        AddToCanTrace("Message failed to send: ERROR_CANUSB_OPEN_SUBSYSTEM");
                        break;
                    case LAWICEL.ERROR_CANUSB_TX_FIFO_FULL:
                        AddToCanTrace("Message failed to send: ERROR_CANUSB_TX_FIFO_FULL");
                        break;
                    default:
                        AddToCanTrace("Message failed to send: " + writeResult.ToString());
                        break;
                }
                return false;
            }
        }

        /// <summary>
        /// waitForMessage waits for a specific CAN message give by a CAN id.
        /// </summary>
        /// <param name="a_canID">The CAN id to listen for</param>
        /// <param name="timeout">Listen timeout</param>
        /// <param name="r_canMsg">The CAN message with a_canID that we where listening for.</param>
        /// <returns>The CAN id for the message we where listening for, otherwise 0.</returns>
        public override uint waitForMessage(uint a_canID, uint timeout, out CANMessage canMsg)
        {
            /*
            int readResult = 0;
            int nrOfWait = 0;
            while (nrOfWait < timeout)
            {
                LAWICEL.CANMsg r_canMsg = new LAWICEL.CANMsg();
                canMsg = new CANMessage();
                readResult = LAWICEL.canusb_Read(m_deviceHandle, out r_canMsg);
                if (readResult == LAWICEL.ERROR_CANUSB_OK)
                {
                    //Console.WriteLine("rx id: 0x" + r_canMsg.id.ToString("X4"));
                    if (r_canMsg.id != a_canID)
                    {
                        nrOfWait++;
                        continue;
                    }
                    else
                    {
                        canMsg.setID(r_canMsg.id);
                        canMsg.setData(r_canMsg.data);
                        canMsg.setFlags(r_canMsg.flags);
                        return (uint)r_canMsg.id;
                    }
                }
                else if (readResult == LAWICEL.ERROR_CANUSB_NO_MESSAGE)
                {
                    Thread.Sleep(1);
                    nrOfWait++;
                }
            }
            canMsg = new CANMessage();
            return 0;*/
            LAWICEL.CANMsg r_canMsg;
            canMsg = new CANMessage();
            int readResult = 0;
            int nrOfWait = 0;
            while (nrOfWait < timeout)
            {
                r_canMsg = new LAWICEL.CANMsg();
                readResult = LAWICEL.canusb_Read(m_deviceHandle, out r_canMsg);
                if (readResult == LAWICEL.ERROR_CANUSB_OK)
                {
                    Thread.Sleep(1);
                    AddToCanTrace("rx: 0x" + r_canMsg.id.ToString("X4") + r_canMsg.data.ToString("X16"));
                    if (r_canMsg.id == 0x00)
                    {
                        nrOfWait++;
                    }
                    else if (r_canMsg.id != a_canID)
                        continue;
                    canMsg.setData(r_canMsg.data);
                    canMsg.setID(r_canMsg.id);
                    canMsg.setLength(r_canMsg.len);
                    return (uint)r_canMsg.id;
                }
                else if (readResult == LAWICEL.ERROR_CANUSB_NO_MESSAGE)
                {
                    Thread.Sleep(1);
                    nrOfWait++;
                }
            }
            r_canMsg = new LAWICEL.CANMsg();
            return 0;
        }

        /// <summary>
        /// waitForMessage waits for a specific CAN message give by a CAN id.
        /// </summary>
        /// <param name="a_canID">The CAN id to listen for</param>
        /// <param name="timeout">Listen timeout</param>
        /// <param name="r_canMsg">The CAN message with a_canID that we where listening for.</param>
        /// <returns>The CAN id for the message we where listening for, otherwise 0.</returns>
        private uint waitForMessage(uint a_canID, uint timeout, out LAWICEL.CANMsg r_canMsg)
        {
            int readResult = 0;
            int nrOfWait = 0;
            while (nrOfWait < timeout)
            {
                readResult = LAWICEL.canusb_Read(m_deviceHandle, out r_canMsg);
                if (readResult == LAWICEL.ERROR_CANUSB_OK)
                {
                    if (r_canMsg.id == 0x00)
                    {
                        nrOfWait++;
                    }
                    else if (r_canMsg.id != a_canID)
                        continue;
                    return (uint)r_canMsg.id;
                }
                else if (readResult == LAWICEL.ERROR_CANUSB_NO_MESSAGE)
                {
                    Thread.Sleep(1);
                    nrOfWait++;
                }
            }
            r_canMsg = new LAWICEL.CANMsg(); 
            return 0;
        }

        /// <summary>
        /// waitAnyMessage waits for any message to be received.
        /// </summary>
        /// <param name="timeout">Listen timeout</param>
        /// <param name="r_canMsg">The CAN message that was first received</param>
        /// <returns>The CAN id for the message received, otherwise 0.</returns>
        private uint waitAnyMessage(uint timeout, out LAWICEL.CANMsg r_canMsg)
        {
            int readResult = 0;
            int nrOfWait = 0;
            while (nrOfWait < timeout)
            {
                readResult = LAWICEL.canusb_Read(m_deviceHandle, out r_canMsg);
                if (readResult == LAWICEL.ERROR_CANUSB_OK)
                {
                    return (uint)r_canMsg.id;
                }
                else if (readResult == LAWICEL.ERROR_CANUSB_NO_MESSAGE)
                {
                    Thread.Sleep(1);
                    nrOfWait++;
                }
            }
            r_canMsg = new LAWICEL.CANMsg();
            return 0;
        }

        /// <summary>
        /// Check if there is connection with a CAN bus.
        /// </summary>
        /// <returns>true on connection, otherwise false</returns>
        private bool boxIsThere()
        {
            return true;
           /* LAWICEL.CANMsg msg = new LAWICEL.CANMsg();
            Console.WriteLine("in Box is there");
            if (waitAnyMessage(2000, out msg) != 0)
            {
                Console.WriteLine("A message was seen");
                return true;
            }
            if (sendSessionRequest())
            {
                Console.WriteLine("Session request success");

                return true;
            }
            Console.WriteLine("Box not there");

            return false;*/
        }
        


        /// <summary>
        /// Send a message that starts a session. This is used to test if there is 
        /// a connection.
        /// </summary>
        /// <returns></returns>
        private bool sendSessionRequest()
        {
            Console.WriteLine("Sending session request");
            // 0x220 is for T7
            // 0x7E0 is for T8
            CANMessage msg1 = new CANMessage(0x7E0, 0, 8);
            LAWICEL.CANMsg msg = new LAWICEL.CANMsg();
            msg1.setData(0x000040021100813f);

            if (!sendMessage(msg1))
            {
                Console.WriteLine("Unable to send session request");
                return false;
            }
            if (waitForMessage(0x7E8, 1000, out msg) == 0x7E8)
            {
                Console.WriteLine("Message 0x7E8 seen");
                //Ok, there seems to be a ECU somewhere out there.
                //Now, sleep for 10 seconds to get a session timeout. This is needed for
                //applications on higher level. Otherwise there will be no reply when the
                //higher level application tries to start a session.
                Thread.Sleep(10000); 
                Console.WriteLine("sendSessionRequest: TRUE");

                return true;
            }
            Console.WriteLine("sendSessionRequest: FALSE");
            return false;
        }

       /* public class InformationFrameEventArgs : System.EventArgs
        {
            private CANMessage _message;

            internal CANMessage Message
            {
                get { return _message; }
                set { _message = value; }
            }

            public InformationFrameEventArgs(CANMessage message)
            {
                this._message = message;
            }
        }
       */
    }


           
}
