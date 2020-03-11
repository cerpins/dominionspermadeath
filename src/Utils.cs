using ProtoBuf;
using System.IO;
using Vintagestory.API.Server;

namespace dominionspermadeath.src
{
    static class Utils
    {
        //Utilities, move these out of scope
        static byte[] ValToByteArr<T>(T val)
        {
            // unsure whether this is the best way about it
            MemoryStream stream = new MemoryStream();
            Serializer.Serialize<T>(stream, val);

            return stream.ToArray();
        }

        static T ByteArrToVal<T>(byte[] arr)
        {
            MemoryStream stream = new MemoryStream(arr);
            return Serializer.Deserialize<T>(stream);
        }
        //Utilities end

        //Cleaner looking methods
        public static bool HasSaveData(ISaveGame save, string key)
        {
            return (save.GetData(@key) != null);
        }
        public static T GetSaveData<T>(ISaveGame save, string key)
        {
            return ByteArrToVal<T>(save.GetData(@key));
        }
        public static void SetSaveData<T>(ISaveGame save, string key, T data)
        {
            save.StoreData(@key, ValToByteArr(data));
        }

    }
}
