using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using SpeakerGroupingTool.Models;
using static SpeakerGroupingTool.Models.LabelData;

namespace SpeakerGroupingTool
{
    public class WebsocketService
    {
        public Dictionary<string, WebSocket> WSTable { get; set; } = [];
        public Dictionary<string, string[]> ParquetTable { get; set; } = [];
        Thread thread ;
        public WebsocketService() 
        {
            //thread = new Thread(AutoClear);
            //thread.Start();
        }

        public void AutoClear()
        {
            while (true)
            {
                lock (WSTable)
                {
                    foreach (var ws in WSTable)
                    {
                        if (ws.Value.State != WebSocketState.Open)
                        {
                            WSTable.Remove(ws.Key);
                        }
                    }
                }
                Thread.Sleep(100000);
            }
        }

        public async Task<string> AddWebSocket(WebSocket ws)
        {
            // 断连时自动删除

            var data = await GetWsResponse(ws);
            var initdata = JsonSerializer.Deserialize<Init>(data);
             lock (WSTable)
            {
                if (WSTable.ContainsKey(initdata.data.username))
                {
                    WSTable.Remove(initdata.data.username);
                }
                WSTable.Add(initdata.data.username, ws);

                if (ParquetTable.ContainsKey(initdata.data.username))
                {
                    ParquetTable.Remove(initdata.data.username);
                }
                ParquetTable.Add(initdata.data.username, initdata.data.parquets);

            }
             return initdata.data.username;

        }

        public async Task RemoveWebSocket(string name)
        {
            WSTable.Remove(name);
            ParquetTable.Remove(name);

        }

        public WebSocket GetWebSocket(string name)
        {
            lock (WSTable)
            {
                if (WSTable.ContainsKey(name))
                {
                    return WSTable[name];
                }
            }
            return null;
        }
        public string[] GetParquets(string name)
        {
            lock (WSTable)
            {
                if (ParquetTable.ContainsKey(name))
                {
                    return ParquetTable[name];
                }
            }
            return null;
        }

        public async Task<Story[]> GetStories(string name, string path, string[] pre)
        {
            try
            {

                if (WSTable.ContainsKey(name))
                {
                    var ws = WSTable[name];
                    GetStory getStory = new GetStory();
                    getStory.path = path;
                    getStory.type = "load_dataset";
                    getStory.previous_dataset = pre ?? [];

                    await ws.SendAsync(JsonSerializer.SerializeToUtf8Bytes(getStory), WebSocketMessageType.Text, true, CancellationToken.None);

                    string? data = await GetWsResponse(ws);

                    if (data != null)
                    {

                        var getstory = JsonSerializer.Deserialize<GetStoryResponse>(data);
                        return getstory.data;
                    }


                }
            }
            catch (Exception e) { }
            
            return [];

        }

        public async Task<Story[]> UpdateStories(string name, Story[] story)
        {
            try { 
            if (WSTable.ContainsKey(name))
            {
                var ws = WSTable[name];
                UpdateStory getStory = new();
                getStory.type = "modify_dataset";
                getStory.data = story.Select(s => new LabelData
                {
                    id = s.RowIndex,
                    人物 = s.人物
                }).ToArray();

                await ws.SendAsync(JsonSerializer.SerializeToUtf8Bytes(getStory), WebSocketMessageType.Text, true, CancellationToken.None);

                string? data = await GetWsResponse(ws);

                if (data != null)
                {

                    var getstory = JsonSerializer.Deserialize<UpdateStoryResponse>(data);
                    return getstory.data;
                }


            }
            }
            catch (Exception e) { }
            return [];
        }

        public async Task Save(string name)
        {
            try
            {
                if (WSTable.ContainsKey(name))
                {
                    var ws = WSTable[name];

                    await ws.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new { type = "save_dataset" }), WebSocketMessageType.Text, true, CancellationToken.None);

                    //    string? data = await GetWsResponse(ws);

                    //    if (data != null)
                    //    {

                    //        var getstory = JsonSerializer.Deserialize<GetStoryResponse>(data);
                    //        return getstory.data;
                    //    }
                    //}
                }
            }
            catch (Exception e) { }
            
            }

        private static async Task<string?> GetWsResponse(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 1024 * 10];
            var aa = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

            var data = Encoding.UTF8.GetString(buffer, 0, aa.Count);
            return data;
        }
    }
}
