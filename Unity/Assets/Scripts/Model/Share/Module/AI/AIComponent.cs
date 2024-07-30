namespace ET
{
    // 客户端挂在ClientScene上，服务端挂在Unit上
    [ComponentOf(typeof (Scene))]
    public class AIComponent: Entity, IAwake<int>, IDestroy
    {
        public int current;
        public int aIConfigId;

        public long Timer;
    }
}