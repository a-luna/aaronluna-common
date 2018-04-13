namespace AaronLuna.Common.Network
{
    using System;
    using System.Text;
    using System.Net.Sockets;

    using Result;

    enum Verbs
    {
        WILL = 251,
        WONT = 252,
        DO = 253,
        DONT = 254,
        IAC = 255
    }

    enum Options
    {
        SGA = 3
    }

    public class BasicTelnet
    {
        readonly TcpClient _tcpClient;
        int _timeoutMs;

        public BasicTelnet(string hostName, int port)
        {
            _tcpClient = new TcpClient(hostName, port);
        }

        public Result<string> Login(string Username, string Password, int LoginTimeOutMs)
        {
            int oldTimeOutMs = _timeoutMs;
            _timeoutMs = LoginTimeOutMs;

            string s = Read();
            if (!s.TrimEnd().EndsWith(":", StringComparison.Ordinal))
            {
                return Result.Fail<string>("Failed to connect : no login prompt");
            }
            WriteLine(Username);

            s += Read();
            if (!s.TrimEnd().EndsWith(":", StringComparison.Ordinal))
            {
                return Result.Fail<string>("Failed to connect : no password prompt");
            }

            WriteLine(Password);

            s += Read();
            _timeoutMs = oldTimeOutMs;

            return Result.Ok(s);
        }

        public void WriteLine(string cmd)
        {
            Write(cmd + "\n");
        }

        public void Write(string cmd)
        {
            if (!_tcpClient.Connected) return;
            byte[] buf = Encoding.ASCII.GetBytes(cmd.Replace("\0xFF", "\0xFF\0xFF"));
            _tcpClient.GetStream().Write(buf, 0, buf.Length);
        }

        public string Read()
        {
            if (!_tcpClient.Connected) return null;
            StringBuilder sb = new StringBuilder();
            do
            {
                ParseTelnet(sb);
                System.Threading.Thread.Sleep(_timeoutMs);
            } while (_tcpClient.Available > 0);
            return sb.ToString();
        }

        void ParseTelnet(StringBuilder sb)
        {
            while (_tcpClient.Available > 0)
            {
                int input = _tcpClient.GetStream().ReadByte();
                switch (input)
                {
                    case -1:
                        break;

                    case (int)Verbs.IAC:
                        // interpret as command
                        DoSomething(sb);
                        break;

                    default:
                        sb.Append((char)input);
                        break;
                }
            }
        }

        void DoSomething(StringBuilder sb)
        {
            int inputverb = _tcpClient.GetStream().ReadByte();
            if (inputverb == -1)
            {
                return;
            }

            switch (inputverb)
            {
                case (int)Verbs.IAC:
                    //literal IAC = 255 escaped, so append char 255 to string
                    sb.Append(inputverb);
                    break;

                case (int)Verbs.DO:
                case (int)Verbs.DONT:
                case (int)Verbs.WILL:
                case (int)Verbs.WONT:
                    // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                    DoSomethingElse(inputverb);
                    break;
            }
        }

        void DoSomethingElse(int inputverb)
        {
            int inputoption = _tcpClient.GetStream().ReadByte();
            if (inputoption == -1)
            {
                return;
            }

            _tcpClient.GetStream().WriteByte((byte)Verbs.IAC);

            if (inputoption == (int)Options.SGA)
            {
                _tcpClient.GetStream().WriteByte(inputverb == (int)Verbs.DO
                                                 ? (byte)Verbs.WILL
                                                 : (byte)Verbs.DO);
            }
            else
            {
                _tcpClient.GetStream().WriteByte(inputverb == (int)Verbs.DO
                                                 ? (byte)Verbs.WONT
                                                 : (byte)Verbs.DONT);
            }

            _tcpClient.GetStream().WriteByte((byte)inputoption);
        }
    }
}
