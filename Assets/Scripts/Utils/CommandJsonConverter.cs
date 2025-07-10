using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Utils
{
    public class CommandJsonConverter : JsonConverter
    {
        #region Json Commands
        public class CommandListJson
        {
            public CommandJson[] commands;
        }

        [JsonConverter(typeof(CommandJsonConverter))]
        public class CommandJson
        {
            public string command;
        }
        public class CommandAudioJson : CommandJson
        {
            public string file;
            public bool block;
        }
        public class CommandTalkJson : CommandJson { public string text; }
        public class CommandWaitJson : CommandJson { public float time; }
        public class CommandEmotionJson : CommandJson { public string emotion; }
        public class CommandMotionJson : CommandJson { public string member; public string direction; }
        public class CommandListenJson : CommandJson { public string state; }
        public class CommandQRCodeJson : CommandJson { }
        public class CommandUserEmotionJson : CommandJson { }
        public class CommandLedAnimationJson : CommandJson { public string color; }
        public class CommandLightJson : CommandJson { public string color; public string state; }
        public class CommandEndJson : CommandJson { }
        #endregion

        public override bool CanWrite { get { return false; } }
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CommandJson);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            string command = jObject["command"]?.ToString();

            CommandJson commandJson = null;
            switch (command)
            {
                case "Talk":
                    commandJson = new CommandTalkJson();
                    break;
                case "Wait":
                    commandJson = new CommandWaitJson();
                    break;
                case "Emotion":
                    commandJson = new CommandEmotionJson();
                    break;
                case "Motion":
                    commandJson = new CommandMotionJson();
                    break;
                case "Listen":
                    commandJson = new CommandListenJson();
                    break;
                case "Audio":
                    commandJson = new CommandAudioJson();
                    break;
                case "QR_Read":
                    commandJson = new CommandQRCodeJson();
                    break;
                case "User_emotion":
                    commandJson = new CommandUserEmotionJson();
                    break;
                case "Led_animation":
                    commandJson = new CommandLedAnimationJson();
                    break;
                case "Light":
                    commandJson = new CommandLightJson();
                    break;
                case "End":
                    commandJson = new CommandEndJson();
                    break;
            }

            serializer.Populate(jObject.CreateReader(), commandJson);
            return commandJson;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
