using System;
using System.IO;
using ProtoBuf;


public abstract class ASerialisation
{
    public byte[] Serialize()
    {
        byte[] result;

        using (var stream = new MemoryStream())
        {
            Serializer.Serialize(stream, this);
            result = stream.ToArray();
        }
        return (result);
    }
}

[ProtoContract]
public class Crypto<T> : ASerialisation
{
    [ProtoMember(1)]
    T msg;

    public Crypto()
    { }

    public Crypto(T msg)
    {
        this.msg = msg;
    }

    public static Crypto<T> Deserialize(Byte[] ByteArray)
    {
        Crypto<T> result;

        using (var stream = new MemoryStream(ByteArray))
        {
            result = Serializer.Deserialize<Crypto<T>>(stream);
        }
        return (result);
    }

    public T GetMessage()
    {
        return (msg);
    }
}