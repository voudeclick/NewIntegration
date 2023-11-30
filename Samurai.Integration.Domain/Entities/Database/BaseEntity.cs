using System;

namespace Samurai.Integration.Domain.Entities.Database
{
    public abstract class BaseEntity
    {
        public long Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public BaseEntity()
        {
            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Brazilian Standard Time");
            DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);

            CreationDate = cstTime;
            UpdateDate = cstTime;
        }
    }
}
