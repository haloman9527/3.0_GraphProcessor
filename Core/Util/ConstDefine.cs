using System;

namespace Jiange.GraphProcessor
{
    public static class ConstDefine
    {
        public readonly static Snowflake IdGenerator = new Snowflake(0, new Snowflake.UtcMSDateTimeProvider(new DateTime(2020, 1, 1, 0, 0, 0)));
    }
}