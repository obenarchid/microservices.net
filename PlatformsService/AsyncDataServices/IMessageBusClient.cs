using PlatformsService.Dtos;

namespace PlatformsService.AsyncDataServices
{
    public interface IMessageBusClient
    {
        void PublishNewPlatform(PlatformPublishedDto platformPublishedDto);
    }
}
