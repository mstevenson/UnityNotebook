// https://github.com/keijiro/AICommand

namespace AICommand.OpenAI
{
    [System.Serializable]
    public struct ResponseMessage
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public struct ResponseChoice
    {
        public int index;
        public ResponseMessage message;
    }

    [System.Serializable]
    public struct Response
    {
        public string id;
        public ResponseChoice[] choices;
    }

    [System.Serializable]
    public struct RequestMessage
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public struct Request
    {
        public string model;
        public RequestMessage[] messages;
    }
}