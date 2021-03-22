using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ExtraRolesMod.Utility
{
    public static class BinarySerializer
    {
        public static byte[] Serialize<T>(T data)
        {
            using (var ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, data);
                return ms.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(ms);
            }
        }
    }
}
