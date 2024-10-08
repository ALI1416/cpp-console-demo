using ConsoleDemo.Model;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;
using log4net;

namespace ConsoleDemo.Service
{

    /// <summary>
    /// socket服务
    /// </summary>
    public class SocketService
    {

        /// <summary>
        /// 日志实例
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(SocketService));

        /// <summary>
        /// 服务器
        /// </summary>
        private SocketServer server;
        /// <summary>
        /// 客户端列表
        /// </summary>
        private List<SocketClient> clientList;
        /// <summary>
        /// 服务器关闭回调函数
        /// </summary>
        private Action serviceCloseCallback;
        /// <summary>
        /// 客户端上下线回调函数 客户端,上线或下线
        /// </summary>
        private Action<SocketClient, bool> clientCallback;
        /// <summary>
        /// 响应回调函数 客户端
        /// </summary>
        private Action<SocketClient> responseCallback;

        /// <summary>
        /// 获取客户端列表
        /// </summary>
        /// <returns>客户端列表</returns>
        public SocketClient[] ClientList()
        {
            return clientList.ToArray();
        }

        /// <summary>
        /// 获取在线客户端列表
        /// </summary>
        /// <returns>在线客户端列表</returns>
        public List<SocketClient> ClientOnlineList()
        {
            return clientList.FindAll(e => e.Client != null);
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="serviceCloseCallback">服务器关闭回调函数</param>
        /// <param name="clientCallback">客户端上下线回调函数 客户端,上线或下线</param>
        /// <param name="responseCallback">响应回调函数 客户端</param>
        /// <returns>是否启动成功</returns>
        public bool Start(IPAddress ip, int port, Action serviceCloseCallback, Action<SocketClient, bool> clientCallback, Action<SocketClient> responseCallback)
        {
            try
            {
                // 新建服务器
                server = new SocketServer();
                // 指定IP地址和端口号
                server.Server.Bind(new IPEndPoint(ip, port));
                // 设置监听数量
                server.Server.Listen(10);
                // 异步监听客户端请求
                server.Server.BeginAccept(Handle, null);
            }
            // 端口号冲突
            catch
            {
                server.Close();
                server = null;
                log.Error("socket服务器端口号冲突");
                return false;
            }
            if (clientList == null)
            {
                clientList = new List<SocketClient>();
            }
            this.serviceCloseCallback = serviceCloseCallback;
            this.clientCallback = clientCallback;
            this.responseCallback = responseCallback;
            log.Info("socket服务器已启动");
            return true;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            if (server != null)
            {
                foreach (SocketClient client in ClientOnlineList())
                {
                    ClientOffline(client);
                }
                server.Close();
            }
        }

        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private void Handle(IAsyncResult ar)
        {
            try
            {
                // 继续异步监听客户端请求
                server.Server.BeginAccept(Handle, null);
            }
            // 主动关闭服务器
            catch
            {
                // 服务器关闭回调函数
                serviceCloseCallback();
                log.Info("主动关闭socket服务器");
                return;
            }
            // 客户端上线
            ClientOnline(server.Server.EndAccept(ar));
        }

        /// <summary>
        /// 客户端上线
        /// </summary>
        /// <param name="socket">Socket</param>
        private void ClientOnline(Socket socket)
        {
            // 已存在
            if (clientList.Exists(e => e.Client == socket))
            {
                return;
            }
            SocketClient client = null;
            try
            {
                client = new SocketClient(socket);
                // 接收消息
                socket.BeginReceive(client.Buffer, 0, SocketClient.MAX_BUFFER_LENGTH, SocketFlags.None, Recevice, client);
                clientList.Add(client);
                // 客户端上线回调函数
                clientCallback(client, true);
            }
            catch
            {
                if (client != null)
                {
                    client.Close();
                }
                return;
            }
        }

        /// <summary>
        /// 客户端下线
        /// </summary>
        /// <param name="client">客户端</param>
        private void ClientOffline(SocketClient client)
        {
            // 不存在
            if (!ClientOnlineList().Contains(client))
            {
                return;
            }
            client.Close();
            // 客户端下线回调函数
            clientCallback(client, false);
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private void Recevice(IAsyncResult ar)
        {
            // 获取当前客户端
            SocketClient client = ar.AsyncState as SocketClient;
            try
            {
                // 获取接收数据长度
                int length = client.Client.EndReceive(ar);
                // 客户端主动断开连接时，会发送0字节消息
                if (length == 0)
                {
                    ClientOffline(client);
                    return;
                }
                client.Length += length;
                // 消息全部接收完毕
                if (client.Client.Available == 0)
                {
                    // 响应回调函数
                    responseCallback(client);
                    client.Length = 0;
                    // 继续接收消息
                    client.Client.BeginReceive(client.Buffer, 0, SocketClient.MAX_BUFFER_LENGTH, SocketFlags.None, Recevice, client);
                }
                // 消息还未全部接收
                else
                {
                    // 计算可用容量
                    int available = SocketClient.MAX_BUFFER_LENGTH - length;
                    // 丢弃溢出消息
                    if (available == 0)
                    {
                        client.Client.BeginReceive(new byte[SocketClient.MAX_BUFFER_LENGTH], 0, SocketClient.MAX_BUFFER_LENGTH, SocketFlags.None, Recevice, client);
                    }
                    // 继续接收消息
                    else
                    {
                        client.Client.BeginReceive(client.Buffer, client.Length, available, SocketFlags.None, Recevice, client);
                    }
                }
            }
            // 超时后失去连接、未知错误
            catch
            {
                ClientOffline(client);
                return;
            }
        }

        /// <summary>
        /// 发送消息 给所有在线客户端
        /// </summary>
        /// <param name="data">消息</param>
        public void Send(byte[] data)
        {
            foreach (SocketClient client in ClientOnlineList())
            {
                Send(client, data);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="data">消息</param>
        public void Send(SocketClient client, byte[] data)
        {
            try
            {
                // 发送消息
                client.Client.BeginSend(data, 0, data.Length, SocketFlags.None, asyncResult =>
                {
                    try
                    {
                        int length = client.Client.EndSend(asyncResult);
                        log.Info("向客户端 " + client.Ip + " 发送 " + length + " 字节的消息");
                    }
                    // 已失去连接
                    catch
                    {
                        ClientOffline(client);
                        return;
                    }
                }, null);
            }
            // 未知错误
            catch
            {
                ClientOffline(client);
                return;
            }
        }

    }
}
