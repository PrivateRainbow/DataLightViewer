using System;

namespace DataLightViewer.Mediator
{
    public sealed class Messenger
    {
        private readonly MultiDictionary<MessageType, Action<Object>> _messageListeners = new MultiDictionary<MessageType, Action<Object>>();

        private static readonly Lazy<Messenger> _instance = new Lazy<Messenger>();
        public static Messenger Instance => _instance.Value;

        public void Subscribe(MessageType type, Action<Object> callback) => _messageListeners.AttachItem(type, callback);
        public void Notify(MessageType type, object message)
        {
            if (_messageListeners.ContainsKey(type))
                _messageListeners[type].ForEach(listener => listener(message));
        }      
    }
}
