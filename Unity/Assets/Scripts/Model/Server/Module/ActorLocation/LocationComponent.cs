using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET.Server
{
    [UniqueId(0, 50)]
    public static class LocationType
    {
        public const int Unit = 0;
        public const int Player = 1;
        public const int GateSession = 2;
        public const int Max = 50;
    }

    [ChildOf(typeof (LocationOneType))]
    public class LockInfo: Entity, IAwake<ActorId, CoroutineLock>, IDestroy
    {
        public ActorId lockActorId;

        public CoroutineLock CoroutineLock { get; set; }
    }

    [ChildOf(typeof (LocationManagerComoponent))]
    public class LocationOneType: Entity, IAwake<int>, ISerializeToEntity
    {
        public int locationType;

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<long, ActorId> locations = new();

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<long, LockInfo> lockInfos = new();
    }

    [ComponentOf(typeof (Scene))]
    public class LocationManagerComoponent: Entity, IAwake
    {
    }
}